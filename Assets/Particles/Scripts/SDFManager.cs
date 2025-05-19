using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Particles
{
    public class SDFManager : MonoBehaviour
    {
        public static SDFManager Instance { get; private set; }
        public static List<SDFSphere> Spheres { get; private set; } = new();

        int _lastUpdate;
        ComputeBuffer _bufferSpheres;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Update()
        {
            if (_bufferSpheres == null || _bufferSpheres.count != Spheres.Count)
            {
                _bufferSpheres?.Dispose();
                if (Spheres.Count > 0)
                    _bufferSpheres = new(Spheres.Count, 16, ComputeBufferType.Structured);
                else
                    _bufferSpheres = null;
            }

            var data = Spheres.Select(s => s.GetInfo()).ToArray();
            _bufferSpheres?.SetData(data);
        }

        public void SetupShader(ComputeShader shader, int kernel)
        {
            if (_bufferSpheres != null)
            {
                shader.SetBuffer(kernel, "SDFSpheres", _bufferSpheres);
                shader.SetInt("SDFSphereCount", _bufferSpheres.count);
            }
            else
            {
                shader.SetInt("SDFSphereCount", 0);
            }
        }
    }
}
