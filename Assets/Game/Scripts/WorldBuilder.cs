using UnityEngine;
using UnityEngine.Tilemaps;

namespace Sunnyland
{
    public static class WorldBuilder
    {
        public static GameObject Build(LevelData data, ArtCatalog art, out PlayerController player)
        {
            var root = new GameObject("World - " + data.title);
            var owned = root.AddComponent<RuntimeAssets>();
            var grid = new GameObject("Grid", typeof(Grid));
            grid.transform.SetParent(root.transform);
            var terrain = new GameObject("Ground Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
            terrain.transform.SetParent(grid.transform);
            terrain.layer = 8;
            var map = terrain.GetComponent<Tilemap>();
            map.GetComponent<TilemapRenderer>().sortingOrder = 0;
            var grass = Tile(art.tileset, new Rect(48, 336, 16, 16));
            var dirt = Tile(art.tileset, new Rect(64, 304, 16, 16));
            owned.Track(grass); owned.Track(grass.sprite);
            owned.Track(dirt); owned.Track(dirt.sprite);
            foreach (var span in data.ground)
                for (int x = span.x; x < span.x + span.width; x++)
                    for (int y = span.y - span.depth + 1; y <= span.y; y++)
                        map.SetTile(new Vector3Int(x, y, 0), y == span.y ? grass : dirt);
            terrain.AddComponent<TilemapCollider2D>().usedByComposite = true;
            var groundBody = terrain.AddComponent<Rigidbody2D>();
            groundBody.bodyType = RigidbodyType2D.Static;
            terrain.AddComponent<CompositeCollider2D>();

            // Sprites retain original source PPU; rescale only the scene objects.
            for (int x = 0; x < data.width; x += 18)
            {
                var sky = Sprite(root.transform, "Sky", art.background, new Vector2(x + 9, 5), 1, -30);
                sky.transform.localScale = new Vector3(18.1f / art.background.bounds.size.x, 15 / art.background.bounds.size.y, 1);
                if (art.middle)
                {
                    var trees = Sprite(root.transform, "Distant canopy", art.middle, new Vector2(x + 7, 4), 1, -20);
                    trees.transform.localScale = Vector3.one * (12 / art.middle.bounds.size.y);
                }
            }
            foreach (var span in data.ground)
            {
                if (span.width < 7 || span.y != 0) continue;
                var tree = Sprite(root.transform, "Tree", art.tree, new Vector2(span.x + 3, 1), 6.25f, -2);
                tree.transform.position += Vector3.up * tree.GetComponent<SpriteRenderer>().bounds.extents.y;
                var rock = Sprite(root.transform, "Rock", art.rock, new Vector2(span.x + span.width - 2, 1), 6.25f, -1);
                rock.transform.position += Vector3.up * rock.GetComponent<SpriteRenderer>().bounds.extents.y;
            }
            foreach (var point in data.cherries) Trigger(root.transform, art.cherry, point, TriggerKind.Cherry);
            foreach (var point in data.gems) Trigger(root.transform, art.gem, point, TriggerKind.Gem);
            foreach (var point in data.spikes) Trigger(root.transform, new[] { art.spikes }, point, TriggerKind.Spikes);
            foreach (var point in data.checkpoints) Trigger(root.transform, new[] { art.sign }, point, TriggerKind.Checkpoint);
            Trigger(root.transform, new[] { art.house }, data.exit, TriggerKind.Exit);
            foreach (var spawn in data.enemies)
            {
                var frames = spawn.kind == "frog" ? art.frog : spawn.kind == "eagle" ? art.eagle : art.opossum;
                var enemy = Sprite(root.transform, spawn.kind, frames[0], new Vector2(spawn.x, spawn.y), 6.25f, 3);
                enemy.layer = 11;
                var body = enemy.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.useFullKinematicContacts = true;
                var box = enemy.AddComponent<BoxCollider2D>();
                box.size = new Vector2(.16f, .12f);
                var flipbook = enemy.AddComponent<Flipbook>(); flipbook.frames = frames;
                var controller = enemy.AddComponent<EnemyController>();
                controller.kind = spawn.kind; controller.left = spawn.left; controller.right = spawn.right;
                controller.speed = spawn.kind == "eagle" ? 2.4f : spawn.kind == "frog" ? 1.1f : 1.6f;
            }
            var fox = Sprite(root.transform, "Player - Fox", art.idle[0], data.spawn, 1, 5);
            fox.layer = 9;
            // Collider dimensions below are local units; 6.25x restores 16 pixels/unit.
            var renderer = fox.GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Simple;
            fox.transform.localScale = Vector3.one * 6.25f;
            var rb = fox.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3; rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = fox.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(.11f, .19f); collider.offset = new Vector2(0, -.025f);
            var material = new PhysicsMaterial2D("Frictionless Fox") { friction = 0, bounciness = 0 };
            owned.Track(material);
            collider.sharedMaterial = material;
            fox.AddComponent<Flipbook>().frames = art.idle;
            player = fox.AddComponent<PlayerController>();
            return root;
        }
        static Tile Tile(Texture2D atlas, Rect rect)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = UnityEngine.Sprite.Create(atlas, rect, new Vector2(.5f, .5f), 16);
            tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
            tile.hideFlags = HideFlags.DontSave;
            return tile;
        }
        static GameObject Sprite(Transform root, string name, Sprite sprite, Vector2 at, float scale, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = at;
            go.transform.localScale = Vector3.one * scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sortingOrder = order;
            return go;
        }
        static void Trigger(Transform root, Sprite[] frames, Vector2 point, TriggerKind kind)
        {
            var go = Sprite(root, kind.ToString(), frames[0], point, 6.25f, 2);
            go.layer = 10;
            var box = go.AddComponent<BoxCollider2D>(); box.isTrigger = true;
            box.size = kind == TriggerKind.Exit ? new Vector2(.3f, .35f) : kind == TriggerKind.Spikes ? new Vector2(.14f, .07f) : new Vector2(.13f, .16f);
            if (kind == TriggerKind.Exit) box.offset = new Vector2(0, -.35f);
            if (frames.Length > 1) go.AddComponent<Flipbook>().frames = frames;
            go.AddComponent<WorldTrigger>().kind = kind;
        }
    }
}
