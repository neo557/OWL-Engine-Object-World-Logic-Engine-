
#include "pch.h"
#include "NativeFunction.h"
#include <vector>
#include <fstream>
#include <sstream>
#include <cstring>
#include <cmath>
#include <tuple>
#include <map>

#define NOMINMAX          
#include <Windows.h>
#undef max              // これが必須
#undef min
#include <algorithm>
struct FaceIndex {
    int v;
    int vt;
    int vn;
};

static FaceIndex ParseFaceElement(const std::string& token)
{
    FaceIndex fi = { -1, -1, -1 };

    std::stringstream ss(token);
    std::string vStr, vtStr, vnStr;

    std::getline(ss, vStr, '/');
    std::getline(ss, vtStr, '/');
    std::getline(ss, vnStr, '/');

    if (!vStr.empty())  fi.v = std::stoi(vStr) - 1;
    if (!vtStr.empty()) fi.vt = std::stoi(vtStr) - 1;
    if (!vnStr.empty()) fi.vn = std::stoi(vnStr) - 1;

    return fi;
}

extern "C" __declspec(dllexport)
bool LoadOBJFull(
    const char* path,
    float** vertices, int* vertexCount,
    float** uvs, int* uvCount,
    float** normals, int* normalCount,
    int** indices, int* indexCount
)
{
    std::ifstream file(path);
    if (!file.is_open())
        return false;

    std::vector<float> vList;
    std::vector<float> vtList;
    std::vector<float> vnList;
    std::vector<FaceIndex> faces;

    std::string line;
    while (std::getline(file, line))
    {
        std::stringstream ss(line);
        std::string type;
        ss >> type;

        if (type == "v")
        {
            float x, y, z;
            ss >> x >> y >> z;
            vList.push_back(x);
            vList.push_back(y);
            vList.push_back(z);
        }
        else if (type == "vt")
        {
            float u, v;
            ss >> u >> v;
            vtList.push_back(u);
            vtList.push_back(v);
        }
        else if (type == "vn")
        {
            float nx, ny, nz;
            ss >> nx >> ny >> nz;
            vnList.push_back(nx);
            vnList.push_back(ny);
            vnList.push_back(nz);
        }
        else if (type == "f")
        {
            std::vector<std::string> tokens;
            std::string tok;

            while (ss >> tok)
                tokens.push_back(tok);

            if (tokens.size() == 3)
            {
                faces.push_back(ParseFaceElement(tokens[0]));
                faces.push_back(ParseFaceElement(tokens[1]));
                faces.push_back(ParseFaceElement(tokens[2]));
            }
            else if (tokens.size() == 4)
            {
                FaceIndex f1 = ParseFaceElement(tokens[0]);
                FaceIndex f2 = ParseFaceElement(tokens[1]);
                FaceIndex f3 = ParseFaceElement(tokens[2]);
                FaceIndex f4 = ParseFaceElement(tokens[3]);

                faces.push_back(f1);
                faces.push_back(f2);
                faces.push_back(f3);

                faces.push_back(f1);
                faces.push_back(f3);
                faces.push_back(f4);
            }
        }
    }

    if (vList.empty())
        return false;

    // スケール正規化（今のままでOK）
    float minX = vList[0], maxX = vList[0];
    float minY = vList[1], maxY = vList[1];
    float minZ = vList[2], maxZ = vList[2];

    for (size_t i = 0; i < vList.size(); i += 3)
    {
        float x = vList[i + 0];
        float y = vList[i + 1];
        float z = vList[i + 2];

        if (x < minX) minX = x;
        if (x > maxX) maxX = x;
        if (y < minY) minY = y;
        if (y > maxY) maxY = y;
        if (z < minZ) minZ = z;
        if (z > maxZ) maxZ = z;
    }

    float sizeX = maxX - minX;
    float sizeY = maxY - minY;
    float sizeZ = maxZ - minZ;

    float maxSize = std::max(sizeX, std::max(sizeY, sizeZ));
    if (maxSize <= 0.0f)
        maxSize = 1.0f;

    float targetSize = 1.0f;
    float scale = targetSize / maxSize;

    for (size_t i = 0; i < vList.size(); i += 3)
    {
        vList[i + 0] *= scale;
        vList[i + 1] *= scale;
        vList[i + 2] *= scale;
    }

    // インデックスリスト作成（faces を使う）
    std::vector<int> idxList;
    idxList.reserve(faces.size());
    for (const auto& f : faces)
    {
        idxList.push_back(f.v);
    }

    // スムース法線生成
    std::vector<float> normalsVec(vList.size(), 0.0f);

    for (size_t i = 0; i < idxList.size(); i += 3)
    {
        int i1 = idxList[i + 0] * 3;
        int i2 = idxList[i + 1] * 3;
        int i3 = idxList[i + 2] * 3;

        float x1 = vList[i1 + 0], y1 = vList[i1 + 1], z1 = vList[i1 + 2];
        float x2 = vList[i2 + 0], y2 = vList[i2 + 1], z2 = vList[i2 + 2];
        float x3 = vList[i3 + 0], y3 = vList[i3 + 1], z3 = vList[i3 + 2];

        float ux = x2 - x1, uy = y2 - y1, uz = z2 - z1;
        float vx = x3 - x1, vy = y3 - y1, vz = z3 - z1;

        float nx = uy * vz - uz * vy;
        float ny = uz * vx - ux * vz;
        float nz = ux * vy - uy * vx;

        normalsVec[i1 + 0] += nx;
        normalsVec[i1 + 1] += ny;
        normalsVec[i1 + 2] += nz;

        normalsVec[i2 + 0] += nx;
        normalsVec[i2 + 1] += ny;
        normalsVec[i2 + 2] += nz;

        normalsVec[i3 + 0] += nx;
        normalsVec[i3 + 1] += ny;
        normalsVec[i3 + 2] += nz;
    }

    for (size_t i = 0; i < normalsVec.size(); i += 3)
    {
        float nx = normalsVec[i + 0];
        float ny = normalsVec[i + 1];
        float nz = normalsVec[i + 2];

        float len = std::sqrt(nx * nx + ny * ny + nz * nz);
        if (len > 0.00001f)
        {
            normalsVec[i + 0] /= len;
            normalsVec[i + 1] /= len;
            normalsVec[i + 2] /= len;
        }
    }

    // 出力用バッファ
    std::vector<float> outVertices = vList;
    std::vector<float> outUVs;      // まだ未対応
    std::vector<float> outNormals = normalsVec;
    std::vector<int>   outIndices = idxList;

    // C# に渡す
    *vertexCount = static_cast<int>(outVertices.size() / 3);
    *uvCount = static_cast<int>(outUVs.size() / 2);
    *normalCount = static_cast<int>(outNormals.size() / 3);
    *indexCount = static_cast<int>(outIndices.size());

    *vertices = new float[outVertices.size()];
    std::memcpy(*vertices, outVertices.data(), outVertices.size() * sizeof(float));

    *uvs = new float[outUVs.size()];
    std::memcpy(*uvs, outUVs.data(), outUVs.size() * sizeof(float));

    *normals = new float[outNormals.size()];
    std::memcpy(*normals, outNormals.data(), outNormals.size() * sizeof(float));

    *indices = new int[outIndices.size()];
    std::memcpy(*indices, outIndices.data(), outIndices.size() * sizeof(int));

    return true;
}

