using System;
using System.Collections.Generic;
using System.Text;

namespace OWL_Engine.Asset
{
    public class SaveObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = ""; // Cube, Rectangle, Triangle, OBJ
        public string? ObjPath { get; set; }   // OBJ の場合だけ
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int ParentId { get; set; }
    }
}
