using System;
using UnityEngine;

namespace PolishTanks.Ground
{
    /// <summary>
    /// One kind of ground in a <see cref="GroundLayerSet"/>: its slice in the texture array, how many world units one
    /// texture repeat covers, and whether vehicles may drive over it (water does not).
    /// </summary>
    [Serializable]
    public class GroundLayer
    {
        [SerializeField] private string name = "Grass";

        [Tooltip("World units covered by one repeat of the texture.")]
        [SerializeField, Min(0.1f)] private float textureSize = 4f;

        [Tooltip("Cells of this layer get a collider that stops vehicles, e.g. water.")]
        [SerializeField] private bool blocksVehicles;

        public GroundLayer() { }

        public GroundLayer(string name, float textureSize, bool blocksVehicles)
        {
            this.name = name;
            this.textureSize = textureSize;
            this.blocksVehicles = blocksVehicles;
        }

        public string Name => name;
        public float TextureSize => textureSize;
        public bool BlocksVehicles => blocksVehicles;
    }
}
