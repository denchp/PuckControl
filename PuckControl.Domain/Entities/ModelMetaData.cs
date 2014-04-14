using System.Collections.Generic;

namespace PuckControl.Domain.Entities
{
    public class ModelMetadata : AbstractEntity
    {
        public string ModelFile { get; set; }
        public IList<ModelMaterial> Materials { get; private set; }
        public bool IsGameWorld { get; set; }
        public ModelMaterial ActiveMaterial { get; set; }
        public ModelMaterial InactiveMaterial { get; set; }
        public ModelMetadata()
        {
            Materials = new List<ModelMaterial>();
        }
    }
}
