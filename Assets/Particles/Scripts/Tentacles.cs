using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Particles
{
    public class Tentacles : MonoBehaviour
    {
        [SerializeField] int gridSize = 10;
        [SerializeField] float offset = 0.5f;
        [SerializeField] int tentacleSegments = 50;
        [SerializeField] float segmentLength = 0.25f;
        [SerializeField] float radius = 0.19f;
        [SerializeField] int pipeSegments = 6;
        [SerializeField] ComputeShader shader;

        [Space]
        [SerializeField] bool showDebugMesh;
        [SerializeField] Mesh debugMesh;
        [SerializeField] Material debugMaterial;

        [Space]
        [SerializeField] Mesh mesh;
        [SerializeField] Material material;


        [StructLayout(LayoutKind.Sequential)]
        struct Particle
        {
            public static int SIZEOF = 4 + 12 * 5 + 4 * 3 + 16 * 2;

            public int parent;
            public Vector3 origin;
            public Vector3 position;
            public Vector3 direction;
            public Vector3 normal;
            public Vector3 velocity;
            public float distance;
            public float radius;
            public float segment;

            public Vector4 random;
            public Vector4 debug;
        }

        Particle[] _particles;
        ComputeBuffer _particlesBuffer;
        ComputeBuffer _triangleBuffer;
        ComputeBuffer _commandBufferDebug;
        ComputeBuffer _commandBufferMesh;
        uint[] _commandBufferDebugArgs = new uint[5] { 0, 0, 0, 0, 0 };
        uint[] _commandBufferMeshArgs = new uint[5] { 0, 0, 0, 0, 0 };
        RenderParams _renderParamsDebug;
        RenderParams _renderParamsMesh;
        MaterialPropertyBlock _mpb;

        private void Start()
        {
            _particles = new Particle[gridSize * gridSize * tentacleSegments];

            var index = 0;
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    var normal = (new Vector3(Random.value - 0.5f, 0.0f, Random.value - 0.5f)).normalized;
                    for (int s = 0; s < tentacleSegments; s++)
                    {
                        float prog = (float)s / (tentacleSegments - 1);
                        _particles[index].parent = s == 0 ? -1 : index - 1;
                        _particles[index].origin = new Vector3(x * offset, -segmentLength * s, y * offset);
                        _particles[index].position = _particles[index].origin;
                        _particles[index].direction = Vector3.up;
                        _particles[index].normal = normal;
                        _particles[index].velocity = Vector3.zero;
                        _particles[index].distance = segmentLength;
                        _particles[index].radius = radius * Mathf.Pow(1.0f - prog, 0.25f);
                        _particles[index].segment = s;
                        _particles[index].random = new Vector4(
                            Random.value, Random.value, Random.value, Random.value
                            );
                        index++;
                    }
                }
            }

            _particlesBuffer = new(_particles.Length, Particle.SIZEOF, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            _particlesBuffer.SetData(_particles);

            _triangleBuffer = new(gridSize * gridSize * tentacleSegments * pipeSegments * 2, (8*4)*3, ComputeBufferType.Append, ComputeBufferMode.Immutable);

            _commandBufferDebug = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            _commandBufferDebugArgs[0] = (uint)debugMesh.GetIndexCount(0);
            _commandBufferDebugArgs[1] = (uint)_particles.Length;
            _commandBufferDebug.SetData(_commandBufferDebugArgs);

            _commandBufferMesh = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            _commandBufferMeshArgs[0] = (uint)mesh.GetIndexCount(0);
            _commandBufferMeshArgs[1] = (uint)_particles.Length;
            _commandBufferMesh.SetData(_commandBufferMeshArgs);

            _renderParamsDebug = new();
            _renderParamsDebug.material = debugMaterial;
            _renderParamsDebug.worldBounds = new Bounds(Vector3.zero, Vector3.one * 100);

            _renderParamsMesh = new();
            _renderParamsMesh.material = material;
            _renderParamsMesh.worldBounds = new Bounds(Vector3.zero, Vector3.one * 100);

            _mpb = new();
        }

        private void OnDestroy()
        {
            _particlesBuffer.Dispose();
            _triangleBuffer.Dispose();
            _commandBufferDebug.Dispose();
            _commandBufferMesh.Dispose();
        }

        private void Update()
        {
            shader.SetFloat("Time", Time.time);
            shader.SetFloat("DeltaTime", Time.deltaTime);

            {
                var kernel = shader.FindKernel("UpdateRoot");
                shader.SetBuffer(kernel, "Particles", _particlesBuffer);
                shader.SetVector("RootOffset", transform.position);
                shader.Dispatch(kernel, _particles.Length / 64, 1, 1);
            }

            {
                var kernel = shader.FindKernel("Process");
                shader.SetBuffer(kernel, "Particles", _particlesBuffer);

                SDFManager.Instance.SetupShader(shader, kernel);


                shader.SetFloat("DeltaTime", Time.deltaTime * 0.25f);
                shader.Dispatch(kernel, _particles.Length / 64, 1, 1);
                shader.Dispatch(kernel, _particles.Length / 64, 1, 1);
                shader.Dispatch(kernel, _particles.Length / 64, 1, 1);
                shader.Dispatch(kernel, _particles.Length / 64, 1, 1);
            }

            {
                var kernel = shader.FindKernel("GenerateMesh");
                shader.SetBuffer(kernel, "Particles", _particlesBuffer);
                shader.SetBuffer(kernel, "Triangles", _triangleBuffer);
                shader.SetInt("PipeSegments", pipeSegments);

                _triangleBuffer.SetCounterValue(0);
                shader.Dispatch(kernel, _particles.Length / 64, 1, 1);

                ComputeBuffer.CopyCount(_triangleBuffer, _commandBufferMesh, 4);
            }



            debugMaterial.SetBuffer("BuffParticles", _particlesBuffer);
            material.SetBuffer("BuffTriangles", _triangleBuffer);


            if (showDebugMesh)
            {
                Graphics.DrawMeshInstancedIndirect(debugMesh, 0, debugMaterial,
                    new Bounds(Vector3.zero, Vector3.one * 100),
                    _commandBufferDebug, 0, _mpb);
            }
            else
            {

                Graphics.DrawMeshInstancedIndirect(mesh, 0, material,
                    new Bounds(Vector3.zero, Vector3.one * 100),
                    _commandBufferMesh, 0, _mpb);

                /*
                Graphics.DrawProceduralIndirect(material,
                    new Bounds(Vector3.zero, Vector3.one * 100),
                    MeshTopology.Triangles,
                    _commandBufferMesh, 0, null, _mpb);
                */
            }
        }
    }
}