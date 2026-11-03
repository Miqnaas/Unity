using UnityEngine;

namespace Sunnyland
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Flipbook : MonoBehaviour
    {
        public Sprite[] frames;
        public float fps = 9;
        float elapsed;
        SpriteRenderer target;
        void Awake() { target = GetComponent<SpriteRenderer>(); }
        public void Play(Sprite[] next)
        {
            if (frames == next) return;
            frames = next;
            elapsed = 0;
        }
        public void Show(Sprite frame) { frames = null; target.sprite = frame; }
        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            elapsed += Time.deltaTime;
            target.sprite = frames[(int)(elapsed * fps) % frames.Length];
        }
    }
}
