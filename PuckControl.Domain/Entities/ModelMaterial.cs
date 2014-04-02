using System;
using System.Windows.Media;

namespace PuckControl.Domain.Entities
{
    public class ModelMaterial : AbstractEntity
    {
        public Int16 MeshIndex { get; set; }
        public Double Opacity { get; set; }
        public string TextureFile { get; set; }
        public Color DiffuseColor { get; set; }
        public Color SpecularColor { get; set; }
        public double SpecularPower { get; set; }
        public Color EmissiveColor { get; set; }

        public ModelMaterial()
        {
            MeshIndex = 0;
            Opacity = 1;
            DiffuseColor = Color.FromRgb(255, 255, 255);
            SpecularColor = Color.FromRgb(255, 255, 255);
            SpecularPower = 1000;
            EmissiveColor = Color.FromRgb(255, 255, 255);
        }

        
    }
}
