using System.Collections.Generic;
using UnityEngine;

namespace Sunnyland
{
    // Destroy only generated native objects; never the imported Sunnyland assets.
    public sealed class RuntimeAssets : MonoBehaviour
    {
        readonly List<Object> owned = new List<Object>();
        public void Track(Object value) { owned.Add(value); }
        void OnDestroy()
        {
            foreach (var value in owned) if (value) Destroy(value);
        }
    }
}
