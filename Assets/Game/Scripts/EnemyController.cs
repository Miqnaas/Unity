using UnityEngine;

namespace Sunnyland
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        public string kind;
        public float left, right, speed = 1.5f;
        Rigidbody2D body;
        SpriteRenderer visual;
        float direction = 1, baseY, phase;
        bool defeated;
        void Awake() { body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); }
        void Start() { baseY = transform.position.y; phase = transform.position.x; }
        void FixedUpdate()
        {
            if (!GameRoot.Instance.IsPlaying || defeated) return;
            float x = body.position.x;
            if (x >= right) direction = -1;
            if (x <= left) direction = 1;
            float y = baseY;
            if (kind == "eagle") y += Mathf.Sin(Time.time * 2 + phase) * .65f;
            if (kind == "frog") y += Mathf.Abs(Mathf.Sin(Time.time * 2 + phase)) * 1.2f;
            body.MovePosition(new Vector2(Mathf.Clamp(x + direction * speed * Time.fixedDeltaTime, left, right), y));
            // Sunnyland enemies face left in their source sprites.
            visual.flipX = direction > 0;
        }
        void OnCollisionEnter2D(Collision2D collision) { Contact(collision.collider); }
        void OnCollisionStay2D(Collision2D collision) { Contact(collision.collider); }
        void Contact(Collider2D collider)
        {
            if (defeated) return;
            var player = collider.GetComponent<PlayerController>();
            if (!player) return;
            if (player.CanStomp(transform))
            {
                defeated = true;
                player.Bounce();
                GameRoot.Instance.AddScore(100);
                GameRoot.Instance.Sound(240, .09f);
                Destroy(gameObject);
            }
            else player.Hurt(transform.position);
        }
    }
}
