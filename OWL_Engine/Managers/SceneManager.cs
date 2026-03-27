using OWL_Engine.CImporter;
using OWL_Engine.Render;
using OWL_Engine.Worlds;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Managers
{
    public class SceneManager
    {
        private _3DRenderer _renderer;

        public SceneManager(_3DRenderer renderer)
        {
            _renderer = renderer;
        }

        public void LoadOBJModel(string path)
        {
            MeshGeometry3D mesh = NativeBridge.CreateMeshFromOBJFull(path);

            WorldObject obj = new ImportedObject();
            obj.SetMesh(mesh);
            obj.Position = new Point3D(0, 0, 0);
            obj.Color = Colors.White;

            _renderer.AddObject(obj);
        }

        public void AddImportedMesh(MeshGeometry3D mesh)
        {
            WorldObject obj = new ImportedObject();
            obj.SetMesh(mesh);
            obj.Position = new Point3D(0, 0, 0);
            obj.Color = Colors.White;

            _renderer.AddObject(obj);
        }
    }
}
