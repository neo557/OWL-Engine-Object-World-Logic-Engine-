#pragma once

extern "C" __declspec(dllexport)
bool LoadOBJFull(
    const char* path,
    float** vertices, int* vertexCount,
    float** uvs, int* uvCount,
    float** normals, int* normalCount,
    int** indices, int* indexCount,
    float** diffuseColor
);