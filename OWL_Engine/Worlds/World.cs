using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using DOESUE.Math;
using OWL_Engine.Worlds;
using DOESUE.Core;

namespace OWL_Engine.Worlds
{
    public class World
    {
        private int nextObjectId = 1;

        private ObjectRegistry registry = new ObjectRegistry();
        private GridMap gridMap = new GridMap();

        public int CreateObject(IntVector3 pos)
        {
            int id = nextObjectId++;
            
            var node = new GridTransFormNode(id);

            node.SetLocalPosition(pos);
            registry.RegisterObject(node);
            gridMap.AddObject(pos, id);
            return id;
        }

        public GridTransFormNode? GetObject(int id)
        {
            registry.TryGetObject(id, out var node);

            return node;
        }

        public IEnumerable<int> GetObjects() 
        {
            return registry.GetAllIds();
        }
    }
}
