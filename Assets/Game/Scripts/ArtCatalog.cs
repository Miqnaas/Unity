using UnityEngine;

namespace Sunnyland
{
    // References point to the original Sunnyland artwork, preserving package GUIDs.
    public sealed class ArtCatalog : ScriptableObject
    {
        public Texture2D tileset;
        public Sprite background, middle, house, sign, spikes, tree, rock;
        public Sprite[] idle, run, jump, hurt, opossum, frog, eagle, cherry, gem;
        public AudioClip music;
    }
}
