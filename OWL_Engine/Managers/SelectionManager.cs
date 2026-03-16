using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace OWL_Engine.Managers
{
    public class SelectionManager
    {
        public int? SelectedId { get; private set; }

        public bool HasSelection => SelectedId.HasValue;

        public void Select(int id)
        {
            SelectedId = id;
        }

        public void Clear()
        {
            SelectedId = null;
        }

        public bool IsSelected(int id)
        {
            return SelectedId == id;
        }

        
    }
}
