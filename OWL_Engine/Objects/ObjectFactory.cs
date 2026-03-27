using System;
using System.Collections.Generic;
using System.Text;
using OWL_Engine.Worlds;

namespace OWL_Engine.Objects
{
    public enum PrimitiveType
    {
        Cube,Rectangle,Triangle,Sphere,Cylinder
    }
    public static class ObjectFactory
    {
        public static WorldObject Create(PrimitiveType type) 
        {
            switch (type)
            {
                case PrimitiveType.Cube:
                    return new CubeObject();
                case PrimitiveType.Rectangle:
                    return new RectangleObject();
                case PrimitiveType.Triangle:
                    return new TriangleObject();
                // 追加予定
                case PrimitiveType.Sphere:
                    throw new NotImplementedException("Sphere is not implemented yet.");

                case PrimitiveType.Cylinder:
                    throw new NotImplementedException("Cylinder is not implemented yet.");

                default:
                    throw new NotImplementedException($"Unknown primitive type: {type}");
            }
        }

    }
}
