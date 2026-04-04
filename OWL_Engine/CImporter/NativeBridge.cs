using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.CImporter
{
    public class NativeBridge
    {
        [DllImport("OWL_Native.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadOBJFull(
            string path,
            out IntPtr vertices, out int vertexCount,
            out IntPtr uvs, out int uvCount,
            out IntPtr normals, out int normalCount,
            out IntPtr indices, out int indexCount,
            out IntPtr colorPtr);


        public static MeshGeometry3D CreateMeshFromOBJFull(string path)
        {
            // C++ 側からデータを受け取る
            bool ok = LoadOBJFull(
                path,
                out IntPtr vPtr, out int vCount,
                out IntPtr uvPtr, out int uvCount,
                out IntPtr nPtr, out int nCount,
                out IntPtr iPtr, out int iCount,
                out IntPtr colorPtr
            );

            if (!ok)
                return new MeshGeometry3D();

            var mesh = new MeshGeometry3D();

            Color? diffuseColor = null;
            if (colorPtr != IntPtr.Zero)
            {
                float[] col = new float[3];
                Marshal.Copy(colorPtr, col, 0, 3);

                byte r = (byte)Math.Clamp(col[0] * 255.0f, 0, 255);
                byte g = (byte)Math.Clamp(col[1] * 255.0f, 0, 255);
                byte b = (byte)Math.Clamp(col[2] * 255.0f, 0, 255);

                diffuseColor = Color.FromRgb(r, g, b);
            }

            // -------------------------
            // 頂点
            // -------------------------
            if (vPtr != IntPtr.Zero && vCount > 0)
            {
                float[] vArray = new float[vCount * 3];
                Marshal.Copy(vPtr, vArray, 0, vArray.Length);

                for (int i = 0; i < vCount; i++)
                {
                    mesh.Positions.Add(new Point3D(
                        vArray[i * 3 + 0],
                        vArray[i * 3 + 1],
                        vArray[i * 3 + 2]
                    ));
                }
            }
            
            // -------------------------
            // UV
            // -------------------------
            if (uvPtr != IntPtr.Zero && uvCount > 0)
            {
                float[] uvArray = new float[uvCount * 2];
                Marshal.Copy(uvPtr, uvArray, 0, uvArray.Length);

                for (int i = 0; i < uvCount; i++)
                {
                    mesh.TextureCoordinates.Add(new Point(
                        uvArray[i * 2 + 0],
                        1.0 - uvArray[i * 2 + 1]   // Blender は上下反転
                    ));
                }
            }

            // -------------------------
            // 法線
            // -------------------------
            if (nPtr != IntPtr.Zero && nCount > 0)
            {
                float[] nArray = new float[nCount * 3];
                Marshal.Copy(nPtr, nArray, 0, nArray.Length);

                for (int i = 0; i < nCount; i++)
                {
                    mesh.Normals.Add(new Vector3D(
                        nArray[i * 3 + 0],
                        nArray[i * 3 + 1],
                        nArray[i * 3 + 2]
                    ));
                }
            }

            // -------------------------
            // インデックス
            // -------------------------
            if (iPtr != IntPtr.Zero && iCount > 0)
            {
                int[] iArray = new int[iCount];
                Marshal.Copy(iPtr, iArray, 0, iArray.Length);

                for (int i = 0; i < iCount; i++)
                {
                    mesh.TriangleIndices.Add(iArray[i]);
                }
            }


            return mesh;
        }

    }
}

