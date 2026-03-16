using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using DOESUE.Math;

namespace OWL_Engine.Raycaster
{
    public class MouseGridRaycaster
    {
        public static IntVector3? GetGridPosition(Point mousePos, Viewport3D viewport,PerspectiveCamera camera, int layerY)
        {
            PointHitTestParameters hitParam = new PointHitTestParameters(mousePos);

            RayHitTestParameters ray = BuildRay(mousePos, viewport, camera);

            if (ray == null) return null;

            double t = (layerY - ray.Origin.Y) / ray.Direction.Y;

            if (t < 0) return null;

            Point3D hit = ray.Origin + ray.Direction * t;

            int x = (int)Math.Floor(hit.X);
            int z = (int)Math.Floor(hit.Z);

            return new IntVector3(x, layerY, z);
        }

        static RayHitTestParameters BuildRay(
            Point mousePos, Viewport3D viewport, PerspectiveCamera camera)
        {
            double px =  (mousePos.X / viewport.ActualWidth) * 2 - 1;
            double py = 1 - (mousePos.Y / viewport.ActualHeight) * 2;

            Vector3D forward = camera.LookDirection;
            forward.Normalize();

            Vector3D right = Vector3D.CrossProduct(forward,camera.UpDirection);
            right.Normalize();

            Vector3D up = Vector3D.CrossProduct(right, forward);

            double fov = camera.FieldOfView * Math.PI / 180;
            double aspect = viewport.ActualWidth / viewport.ActualHeight;

            double tan = Math.Tan(fov / 2);

            Vector3D dir = forward + right * px * tan * aspect + up * py * tan;
            dir.Normalize();

            return new RayHitTestParameters(camera.Position, dir);
        }
        
    }
}
