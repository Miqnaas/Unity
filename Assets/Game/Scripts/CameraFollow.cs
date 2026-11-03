using UnityEngine;

namespace Sunnyland
{
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float levelWidth = 80;
        Vector3 velocity;
        public void Snap()
        {
            if (!target) return;
            transform.position = Desired(); velocity = Vector3.zero;
        }
        Vector3 Desired()
        {
            float half = GetComponent<Camera>().orthographicSize * GetComponent<Camera>().aspect;
            float x = half * 2 >= levelWidth ? levelWidth / 2 : Mathf.Clamp(target.position.x + 1.5f, half, levelWidth - half);
            return new Vector3(x, Mathf.Clamp(target.position.y + 1.2f, 4.5f, 8), -10);
        }
        void LateUpdate()
        {
            if (target && GameRoot.Instance.IsPlaying)
                transform.position = Vector3.SmoothDamp(transform.position, Desired(), ref velocity, .16f);
        }
    }
}
