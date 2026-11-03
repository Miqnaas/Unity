using System;
using UnityEngine;

namespace Sunnyland
{
    [Serializable] public sealed class GroundSpan
    {
        public int x, y, width, depth = 3;
    }
    [Serializable] public sealed class EnemySpawn
    {
        public string kind = "opossum";
        public float x, y, left, right;
    }
    [Serializable] public sealed class LevelData
    {
        public string title, subtitle;
        public int width, requiredGems;
        public float parTime;
        public Vector2 spawn, exit;
        public GroundSpan[] ground;
        public Vector2[] cherries, gems, spikes, checkpoints;
        public EnemySpawn[] enemies;
    }
}
