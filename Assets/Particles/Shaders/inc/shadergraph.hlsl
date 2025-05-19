#ifndef INC_SHADERGRAPH_INCLUDED
#define INC_SHADERGRAPH_INCLUDED

#include "structs.hlsl"

StructuredBuffer<Particle> BuffParticles;
StructuredBuffer<Triangle> BuffTriangles;


void GetParticleCenter_float(in float instance, out float3 center)
{
    #ifdef SHADERGRAPH_PREVIEW
        center = float3(0, 0, 0);
    #else
        uint index = uint(round(instance));
        Particle p = BuffParticles[index];
        center = p.position;
    #endif
}

void GetParticleDebug_float(in float instance, out float4 debug)
{
    #ifdef SHADERGRAPH_PREVIEW
        debug = float4(0, 0, 0, 0);
    #else
        uint index = uint(round(instance));
        Particle p = BuffParticles[index];
        debug = p.debug;
    #endif
}

void GetProceduralTriangle_float(in float instance, in float vertex, out float3 position, out float3 normal, out float2 uv)
{
    #ifdef SHADERGRAPH_PREVIEW
        position = float3(0, 0, 0);
        normal = float3(0, 0, 0);
        uv = float2(0, 0);
    #else
        uint index = uint(round(instance));
        uint v = uint(round(vertex)) % 3;

        Triangle t = BuffTriangles[index];
        position = t.position[v];
        normal = t.normal[v];
        uv = t.uv[v];
    #endif
}

void passthroughVec3_float(in float3 In, out float3 Out)
{
    Out = In;
}

void setup()
{
    //

}

#endif