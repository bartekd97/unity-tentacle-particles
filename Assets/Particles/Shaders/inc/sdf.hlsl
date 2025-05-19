#ifndef INC_SDF_INCLUDED
#define INC_SDF_INCLUDED

RWStructuredBuffer<float4> SDFSpheres;
int SDFSphereCount;

float GetSignedDistance(float3 position)
{
    float dist = 99999;
    for (int i = 0; i < SDFSphereCount; i++)
    {
        float4 sphere = SDFSpheres[i];
        float d = distance(position, sphere.xyz) - sphere.w;
        dist = min(dist, d);
    }
    return dist;
}

float3 GetSDFSeparationDirection(float3 position, float sd, float3x3 tbn)
{
    float aSD = abs(sd);
    float a = GetSignedDistance(position + tbn[0] * aSD) - sd;
    float b = GetSignedDistance(position + tbn[1] * aSD) - sd;
    float c = GetSignedDistance(position + tbn[2] * aSD) - sd;

    float3 weights = float3(a, b, c);
    weights = normalize(weights);

    float3 direction = 0;
    direction += tbn[0] * weights.x;
    direction += tbn[1] * weights.y;
    direction += tbn[2] * weights.z;
    direction = normalize(direction);

    return direction;
}

#endif