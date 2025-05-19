using UnityEngine;

namespace Particles
{
    [RequireComponent(typeof(SphereCollider))]
    public class SDFSphere : MonoBehaviour
    {
        SphereCollider _collider;
        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            SDFManager.Spheres.Add(this);
        }
        private void OnDisable()
        {
            SDFManager.Spheres.Remove(this);
        }

        public Vector4 GetInfo()
        {
            var info = (Vector4)transform.position;
            info.w = _collider.radius * transform.lossyScale.x;
            return info;
        }
    }
}
