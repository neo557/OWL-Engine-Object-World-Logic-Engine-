using Microsoft.Win32;
using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace OWL_Engine.Asset
{
    public class SaveData
    {
        public List<SaveObject> Objects { get; set; } = new();
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
    }
}
