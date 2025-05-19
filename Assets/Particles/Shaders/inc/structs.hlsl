#ifndef INC_STRUCTS_INCLUDED
#define INC_STRUCTS_INCLUDED

struct Particle
{
    int parent;
    float3 origin;
    float3 position;
    float3 direction;
    float3 normal;
    float3 velocity;
    float distance;
    float radius;
    float segment;

    float4 random;
    float4 debug;
};


struct Triangle
{
    float3 position[3];
    float3 normal[3];
    float2 uv[3];
};

#endif