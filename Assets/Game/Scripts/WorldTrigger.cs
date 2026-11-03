using UnityEngine;

namespace Sunnyland
{
    public enum TriggerKind { Cherry, Gem, Spikes, Checkpoint, Exit }
    public sealed class WorldTrigger : MonoBehaviour
    {
        public TriggerKind kind;
        bool collected;
        Vector3 home;
        void Start() { home = transform.position; }
        void Update()
        {
            if (kind == TriggerKind.Cherry || kind == TriggerKind.Gem)
                transform.position = home + Vector3.up * Mathf.Sin(Time.time * 3 + home.x) * .08f;
        }
        void OnTriggerEnter2D(Collider2D other) { Touch(other); }
        void OnTriggerStay2D(Collider2D other) { if (kind == TriggerKind.Spikes) Touch(other); }
        void Touch(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            var game = GameRoot.Instance;
            if (!player || collected || !game.IsPlaying) return;
            switch (kind)
            {
                case TriggerKind.Cherry:
                    collected = true; game.Collect(false); Destroy(gameObject); break;
                case TriggerKind.Gem:
                    collected = true; game.Collect(true); Destroy(gameObject); break;
                case TriggerKind.Spikes: player.Hurt(transform.position); break;
                case TriggerKind.Checkpoint:
                    collected = true;
                    game.Checkpoint(new Vector2(transform.position.x, transform.position.y + .7f));
                    GetComponent<SpriteRenderer>().color = new Color(.4f, 1, .6f);
                    break;
                case TriggerKind.Exit: game.TryFinish(); break;
            }
        }
    }
}
