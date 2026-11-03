using UnityEngine;

namespace Sunnyland
{
    // IMGUI keeps this small teaching project self-contained: no font downloads or UI packages.
    public sealed class GameHUD : MonoBehaviour
    {
        GameRoot game;
        GUIStyle heading, subheading, body, small, button;
        int selection, buttonIndex, previousCount;
        GameScreen previous;
        bool moveFocus;
        readonly Color ink = new Color(.035f, .08f, .12f, .94f);
        readonly Color cream = new Color(.96f, .94f, .83f);
        readonly Color mint = new Color(.54f, .88f, .63f);
        void Awake() { game = GetComponent<GameRoot>(); }
        void Update()
        {
            if (previous != game.ScreenState) { selection = 0; moveFocus = true; previous = game.ScreenState; }
            if (game.IsPlaying || previousCount == 0) return;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Tab))
            { selection = (selection + 1) % previousCount; moveFocus = true; }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            { selection = (selection + previousCount - 1) % previousCount; moveFocus = true; }
        }
        void Styles()
        {
            if (body != null) return;
            heading = new GUIStyle(GUI.skin.label) { fontSize = 54, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            heading.normal.textColor = cream;
            subheading = new GUIStyle(heading) { fontSize = 28 };
            body = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true, alignment = TextAnchor.MiddleLeft };
            body.normal.textColor = cream;
            small = new GUIStyle(body) { fontSize = 17 };
            button = new GUIStyle(GUI.skin.button) { fontSize = 23, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            button.normal.textColor = cream;
            button.hover.textColor = Color.white;
        }
        void OnGUI()
        {
            if (!game.Art) return;
            Styles();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            buttonIndex = 0;
            if (game.IsPlaying)
            {
                Panel(new Rect(20, 20, 1240, 78), ink);
                Label(40, 30, 320, 30, game.Current.title, body);
                Label(40, 64, 370, 24, "Hearts " + game.Player.Hearts + "/3    Lives " + game.Lives + "/3", small);
                Label(420, 28, 420, 30, "Gems " + game.Gems + "/" + game.Current.requiredGems + "    Cherries " + game.Cherries, body);
                Label(420, 65, 400, 24, "Score " + game.Score + "    " + TimeText(game.Elapsed), small);
                if (Button(new Rect(1070, 36, 165, 44), "Pause [Esc]")) game.Pause();
                Panel(new Rect(420, 91, 400, 4), new Color(.2f, .3f, .3f));
                Panel(new Rect(420, 91, 400 * Mathf.Clamp01((float)game.Gems / game.Current.requiredGems), 4), mint);
                Label(25, 674, 1100, 30, "A/D or arrows: move   |   Space / W / Up: jump   |   R: respawn (costs a life)   |   Esc: pause", small);
            }
            else
            {
                Panel(new Rect(0, 0, 1280, 720), new Color(.01f, .04f, .08f, .56f));
                Panel(new Rect(250, 35, 780, 650), ink);
                switch (game.ScreenState)
                {
                    case GameScreen.Menu: Menu(); break;
                    case GameScreen.Paused:
                        Title("TAKE A BREATHER", "Your progress is safe while paused.");
                        if (Button(new Rect(440, 265, 400, 54), "Resume")) game.Resume();
                        if (Button(new Rect(440, 335, 400, 54), "Restart stage")) game.StartLevel(game.LevelIndex);
                        if (Button(new Rect(440, 405, 400, 54), "Main menu")) game.Menu();
                        break;
                    case GameScreen.Complete:
                        Title(game.LevelIndex == 3 ? "ADVENTURE COMPLETE!" : "STAGE COMPLETE!", new string('*', game.Stars) + "   " + game.Stars + " / 3 stars");
                        Label(340, 230, 620, 120, "Score: " + game.Score + "     Time: " + TimeText(game.Elapsed) + "\nCherries: " + game.Cherries + "     Gems: " + game.Gems + "\nStars: finish + no lives lost + beat " + TimeText(game.Current.parTime), body);
                        if (game.LevelIndex < 3 && Button(new Rect(440, 390, 400, 54), "Next stage")) game.StartLevel(game.LevelIndex + 1);
                        if (Button(new Rect(440, 460, 400, 54), "Play again")) game.StartLevel(game.LevelIndex);
                        if (Button(new Rect(440, 530, 400, 54), "Main menu")) game.Menu();
                        break;
                    case GameScreen.GameOver:
                        Title("TRY ANOTHER TRAIL", "All lives used. The stage can be restarted.");
                        Label(380, 240, 520, 70, "Score: " + game.Score + "     Time: " + TimeText(game.Elapsed), body);
                        if (Button(new Rect(440, 350, 400, 54), "Retry stage")) game.StartLevel(game.LevelIndex);
                        if (Button(new Rect(440, 420, 400, 54), "Main menu")) game.Menu();
                        break;
                    case GameScreen.Help:
                        Title("HOW TO PLAY", "Find the gems. Reach the little house.");
                        Label(325, 210, 650, 350,
                            "A / D or Left / Right - move\nSpace, W or Up - jump; release early for a small hop\nEsc - pause / resume     R - respawn (uses a life)\n\nJump onto enemies to defeat them. Avoid spikes.\nCherries add score; collect every required gem.\nTouch wooden signs to set a checkpoint.\nThree hearts per life, three lives per stage.\n\nEarn stars by finishing, losing no lives and beating par.\nStage unlocks, best times and stars save automatically.", body);
                        if (Button(new Rect(440, 580, 400, 54), "Back")) game.Menu();
                        break;
                    case GameScreen.Settings:
                        Title("SOUND SETTINGS", "Adjust music and sound effects together.");
                        Label(390, 260, 500, 50, "Volume: " + Mathf.RoundToInt(game.Volume * 100) + "%", body);
                        game.SetVolume(GUI.HorizontalSlider(new Rect(390, 340, 500, 35), game.Volume, 0, 1));
                        if (Button(new Rect(440, 430, 400, 54), game.Volume > 0 ? "Mute" : "Enable sound")) game.SetVolume(game.Volume > 0 ? 0 : .55f);
                        if (Button(new Rect(440, 510, 400, 54), "Save and back")) { game.SaveSettings(); game.Menu(); }
                        break;
                }
                Label(320, 644, 640, 25, "Sunnyland Adventure  /  HCI project  /  Arrows + Enter", small);
            }
            if (game.Toast != null && game.IsPlaying)
            {
                Panel(new Rect(260, 118, 760, 68), ink);
                Label(280, 125, 720, 54, game.Toast, body);
            }
            previousCount = buttonIndex;
            if (moveFocus && Event.current.type == EventType.Repaint)
            { GUI.FocusControl("action-" + selection); moveFocus = false; }
        }
        void Menu()
        {
            Title("SUNNYLAND ADVENTURE", "Three trails. One curious fox. A pocket full of gems.");
            for (int i = 1; i <= 3; i++)
            {
                string key = "sunny.level." + i;
                bool unlocked = i <= game.Unlocked;
                GUI.enabled = unlocked;
                string title = i == 1 ? "01  -  Meadow Trail" : i == 2 ? "02  -  Mossy Crossing" : "03  -  Eagle Heights";
                if (Button(new Rect(370, 220 + (i - 1) * 82, 540, 54), unlocked ? title : title + "  [Locked]")) game.StartLevel(i);
                GUI.enabled = true;
                Label(390, 276 + (i - 1) * 82, 520, 24,
                    "Best: " + PlayerPrefs.GetInt(key + ".score", 0) + "   Stars: " + PlayerPrefs.GetInt(key + ".stars", 0) + "/3", small);
            }
            if (Button(new Rect(370, 490, 260, 50), "How to play")) game.ShowHelp();
            if (Button(new Rect(650, 490, 260, 50), "Sound")) game.ShowSettings();
            if (Button(new Rect(500, 560, 280, 46), "Quit")) game.Quit();
        }
        void Title(string title, string subtitle)
        {
            Label(285, 62, 710, 120, title, heading);
            Label(315, 170, 650, 42, subtitle, small);
        }
        bool Button(Rect rect, string text)
        {
            int index = buttonIndex++;
            GUI.SetNextControlName("action-" + index);
            bool focused = index == selection && !game.IsPlaying;
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = focused ? new Color(.22f, .65f, .43f) : new Color(.18f, .31f, .35f);
            bool result = GUI.Button(rect, text, button);
            GUI.backgroundColor = old;
            if (GUI.enabled && focused && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            { Event.current.Use(); result = true; }
            return result;
        }
        static void Panel(Rect rect, Color color)
        {
            Color old = GUI.color; GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old;
        }
        static void Label(float x, float y, float w, float h, string text, GUIStyle style) { GUI.Label(new Rect(x, y, w, h), text, style); }
        public static string TimeText(float seconds) { return Mathf.FloorToInt(seconds / 60).ToString("00") + ":" + Mathf.FloorToInt(seconds % 60).ToString("00"); }
    }
}
