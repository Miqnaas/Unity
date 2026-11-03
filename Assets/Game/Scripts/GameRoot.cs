using System;
using UnityEngine;

namespace Sunnyland
{
    public enum GameScreen { Menu, Playing, Paused, Complete, GameOver, Help, Settings }

    public sealed class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }
        public ArtCatalog Art { get; private set; }
        public GameScreen ScreenState { get; private set; } = GameScreen.Menu;
        public bool IsPlaying { get { return ScreenState == GameScreen.Playing; } }
        public int initialLevel;
        public int LevelIndex { get; private set; }
        public int Score { get; private set; }
        public int Gems { get; private set; }
        public int Cherries { get; private set; }
        public int Lives { get; private set; } = 3;
        public int Stars { get; private set; }
        public float Elapsed { get; private set; }
        public LevelData Current { get; private set; }
        public PlayerController Player { get; private set; }
        public int Unlocked { get { return Mathf.Clamp(PlayerPrefs.GetInt("sunny.unlocked", 1), 1, 3); } }
        public float Volume { get; private set; }
        public string Toast { get; private set; }
        float toastUntil;
        GameObject world;
        CameraFollow follow;
        AudioSource music, effects;
        AudioClip tone;
        Vector2 checkpoint;
        int deaths;
        bool ready;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1;
            Art = Resources.Load<ArtCatalog>("SunnylandCatalog");
            if (!Art) { Debug.LogError("Missing SunnylandCatalog. Restore the supplied Resources asset."); enabled = false; return; }
            // Explicit layer rules avoid triggers or actors changing grounded state.
            Physics2D.IgnoreLayerCollision(11, 8, true);
            Physics2D.IgnoreLayerCollision(11, 11, true);
            Physics2D.IgnoreLayerCollision(11, 10, true);
            Physics2D.IgnoreLayerCollision(9, 11, false);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
            var camera = cameraObject.GetComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.orthographic = true; camera.orthographicSize = 5.6f;
            camera.backgroundColor = new Color(.08f, .17f, .24f);
            cameraObject.transform.position = new Vector3(10, 4.5f, -10);
            follow = cameraObject.GetComponent<CameraFollow>();
            cameraObject.transform.SetParent(transform);
            music = gameObject.AddComponent<AudioSource>();
            music.clip = Art.music; music.loop = true;
            effects = gameObject.AddComponent<AudioSource>();
            Volume = PlayerPrefs.GetFloat("sunny.volume", .55f);
            SetVolume(Volume);
            if (music.clip) music.Play();
            tone = AudioClip.Create("Soft pickup tone", 4410, 1, 44100, false);
            var samples = new float[4410];
            for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 440 / 44100) * .15f * (1 - (float)i / samples.Length);
            tone.SetData(samples, 0);
            gameObject.AddComponent<GameHUD>();
            ready = true;
        }
        void Start()
        {
            if (!ready) return;
            if (initialLevel > 0) StartLevel(initialLevel, true);
            else Menu();
        }
        void Update()
        {
            if (IsPlaying) Elapsed += Time.deltaTime;
            if (Toast != null && Time.unscaledTime > toastUntil) Toast = null;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPlaying) Pause();
                else if (ScreenState == GameScreen.Paused) Resume();
                else if (ScreenState == GameScreen.Help || ScreenState == GameScreen.Settings) Menu();
            }
            if (IsPlaying && Input.GetKeyDown(KeyCode.R)) Fall();
        }
        public static LevelData ReadLevel(int index)
        {
            var text = Resources.Load<TextAsset>("Level" + index);
            if (!text) throw new InvalidOperationException("Missing level " + index);
            return JsonUtility.FromJson<LevelData>(text.text);
        }
        public void StartLevel(int index, bool allowLocked = false)
        {
            if (index < 1 || index > 3 || (!allowLocked && index > Unlocked)) return;
            Time.timeScale = 1;
            LevelIndex = index; Score = Gems = Cherries = deaths = 0;
            Lives = 3; Elapsed = 0; Toast = null;
            Current = ReadLevel(index);
            checkpoint = Current.spawn;
            Rebuild();
            ScreenState = GameScreen.Playing;
            Notify(Current.subtitle);
        }
        void Rebuild()
        {
            if (world) { world.SetActive(false); Destroy(world); }
            PlayerController actor;
            world = WorldBuilder.Build(Current, Art, out actor);
            Player = actor;
            follow.target = Player.transform;
            follow.levelWidth = Current.width;
            follow.Snap();
        }
        public void Menu()
        {
            Time.timeScale = 1;
            ScreenState = GameScreen.Menu;
            Current = ReadLevel(1);
            Rebuild();
            Player.gameObject.SetActive(false);
            follow.target = null;
            follow.transform.position = new Vector3(10, 4.5f, -10);
        }
        public void Pause() { if (!IsPlaying) return; ScreenState = GameScreen.Paused; Time.timeScale = 0; }
        public void Resume() { ScreenState = GameScreen.Playing; Time.timeScale = 1; }
        public void ShowHelp() { ScreenState = GameScreen.Help; }
        public void ShowSettings() { ScreenState = GameScreen.Settings; }
        public void Collect(bool gem)
        {
            if (gem) { Gems++; Score += 250; } else { Cherries++; Score += 25; }
            Sound(gem ? 880 : 660, .08f);
        }
        public void AddScore(int amount) { Score += amount; }
        public void Checkpoint(Vector2 point) { checkpoint = point; Notify("Checkpoint saved!"); Sound(770, .1f); }
        public void Fall() { if (IsPlaying) LoseLife(); }
        public void LoseLife()
        {
            if (!IsPlaying) return;
            deaths++; Lives--;
            if (Lives <= 0) { ScreenState = GameScreen.GameOver; Time.timeScale = 0; return; }
            Player.Respawn(checkpoint);
            follow.Snap();
            Notify("Back at checkpoint - " + Lives + " lives left");
        }
        public void TryFinish()
        {
            if (!IsPlaying) return;
            if (Gems < Current.requiredGems)
            {
                Notify("Find " + (Current.requiredGems - Gems) + " more gems, then return to the house.");
                return;
            }
            Stars = 1 + (deaths == 0 ? 1 : 0) + (Elapsed <= Current.parTime ? 1 : 0);
            Score += Lives * 100 + Mathf.Max(0, Mathf.RoundToInt(Current.parTime - Elapsed)) * 5;
            PlayerPrefs.SetInt("sunny.unlocked", Mathf.Max(Unlocked, Mathf.Min(3, LevelIndex + 1)));
            string key = "sunny.level." + LevelIndex;
            PlayerPrefs.SetInt(key + ".stars", Mathf.Max(PlayerPrefs.GetInt(key + ".stars", 0), Stars));
            PlayerPrefs.SetInt(key + ".score", Mathf.Max(PlayerPrefs.GetInt(key + ".score", 0), Score));
            float previous = PlayerPrefs.GetFloat(key + ".time", float.MaxValue);
            PlayerPrefs.SetFloat(key + ".time", Mathf.Min(previous, Elapsed));
            PlayerPrefs.Save();
            ScreenState = GameScreen.Complete; Time.timeScale = 0;
        }
        public void SetVolume(float volume)
        {
            Volume = Mathf.Clamp01(volume);
            if (music) music.volume = Volume * .45f;
            if (effects) effects.volume = Volume;
        }
        public void SaveSettings() { PlayerPrefs.SetFloat("sunny.volume", Volume); PlayerPrefs.Save(); }
        public void Notify(string text) { Toast = text; toastUntil = Time.unscaledTime + 4; }
        public void Sound(float frequency, float duration)
        {
            if (!effects || !tone) return;
            effects.pitch = frequency / 440;
            effects.PlayOneShot(tone, Mathf.Clamp01(duration * 8));
        }
        public void Quit()
        {
            SaveSettings();
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        void OnDestroy()
        {
            Time.timeScale = 1;
            if (world) Destroy(world);
            if (tone) Destroy(tone);
            if (Instance == this) Instance = null;
        }
    }
}
