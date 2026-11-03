using UnityEngine;

namespace Sunnyland
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float speed = 6, jumpSpeed = 12;
        public bool Grounded { get; private set; }
        public int Hearts { get; private set; } = 3;
        Rigidbody2D body;
        BoxCollider2D box;
        SpriteRenderer visual;
        Flipbook animationFrames;
        float horizontal, jumpBufferedUntil = -1, lastGroundTime = -1;
        float invulnerableUntil, hurtUntil;
        bool jumpReleased;
        GameRoot game;
        readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            visual = GetComponent<SpriteRenderer>();
            animationFrames = GetComponent<Flipbook>();
            game = GameRoot.Instance;
        }
        void Update()
        {
            if (!game || !game.IsPlaying) return;
            horizontal = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                QueueJump();
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) jumpReleased = true;
            if (Mathf.Abs(horizontal) > .1f) visual.flipX = horizontal < 0;
            var art = game.Art;
            if (Time.time < hurtUntil) animationFrames.Play(art.hurt);
            else if (!Grounded) animationFrames.Show(art.jump[body.velocity.y > 0 ? 1 : 0]);
            else animationFrames.Play(Mathf.Abs(horizontal) > .1f ? art.run : art.idle);
            visual.color = Time.time < invulnerableUntil && Mathf.FloorToInt(Time.time * 12) % 2 == 0
                ? new Color(1, 1, 1, .35f) : Color.white;
            if (transform.position.y < -7) game.Fall();
        }
        public void QueueJump() { jumpBufferedUntil = Time.time + .12f; }
        void FixedUpdate()
        {
            if (!game || !game.IsPlaying) return;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(1 << 8);
            filter.useTriggers = false;
            int count = box.Cast(Vector2.down, filter, groundHits, .09f);
            Grounded = false;
            for (int i = 0; i < count; i++)
                if (groundHits[i].normal.y > .6f && body.velocity.y <= .1f) Grounded = true;
            if (Grounded) lastGroundTime = Time.time;
            if (Time.time >= hurtUntil) body.velocity = new Vector2(horizontal * speed, body.velocity.y);
            // Coyote time and a short input buffer make edge jumps forgiving.
            if (Time.time <= jumpBufferedUntil && Time.time - lastGroundTime <= .1f)
            {
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                jumpBufferedUntil = -1;
                lastGroundTime = -1;
                Grounded = false;
                game.Sound(520, .07f);
            }
            if (jumpReleased && body.velocity.y > 0) body.velocity = new Vector2(body.velocity.x, body.velocity.y * .5f);
            jumpReleased = false;
            if (body.velocity.y < -18) body.velocity = new Vector2(body.velocity.x, -18);
        }
        public void Hurt(Vector2 source)
        {
            if (!game.IsPlaying || Time.time < invulnerableUntil) return;
            Hearts--;
            invulnerableUntil = Time.time + 1.5f;
            hurtUntil = Time.time + .25f;
            body.velocity = new Vector2(transform.position.x < source.x ? -5 : 5, 7);
            game.Sound(130, .18f);
            if (Hearts <= 0) game.LoseLife();
        }
        public void Bounce() { body.velocity = new Vector2(body.velocity.x, 9); }
        public bool CanStomp(Transform enemy) { return body.velocity.y <= .5f && box.bounds.min.y > enemy.position.y + .1f; }
        public void Respawn(Vector2 position)
        {
            transform.position = position;
            body.velocity = Vector2.zero;
            Hearts = 3;
            invulnerableUntil = Time.time + 1.5f;
            jumpBufferedUntil = lastGroundTime = -1;
            horizontal = 0;
        }
    }
}
