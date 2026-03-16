using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Media3D;
using DOESUE.Math;
using System.Windows.Controls;

namespace OWL_Engine.Raycaster
{
    public class ObjectRaycaster
    {
        public static GeometryModel3D? Raycast(Point mousePos, Viewport3D viewport)
        {
            GeometryModel3D? hitModel = null;
            double closest = double.MaxValue;

            VisualTreeHelper.HitTest(viewport, null, result =>
            {
                if (result is RayHitTestResult rayResult && rayResult is RayMeshGeometry3DHitTestResult meshResult && meshResult.DistanceToRayOrigin < closest)
                {
                    closest = meshResult.DistanceToRayOrigin;
                    hitModel = meshResult.ModelHit as GeometryModel3D;
                }
                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(mousePos));

            return hitModel;
        }
    }
}
