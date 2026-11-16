using System;
using UnityEngine;

namespace PolishTanks.Ground
{
    /// <summary>
    /// One kind of ground in a <see cref="GroundLayerSet"/>: how many world units one texture repeat covers, whether
    /// vehicles may drive over it (water does not), and which slices of the baked texture array hold its variants
    /// (<see cref="VariantCount"/> slices from <see cref="FirstSlice"/>). The ground shader picks one variant at random
    /// for every texture repeat. The source textures live in an editor-only asset, so builds carry only the baked array.
    /// </summary>
    [Serializable]
    public class GroundLayer
    {
        [SerializeField] private string name = "Grass";

        [Tooltip("World units covered by one repeat of the texture.")]
        [SerializeField, Min(0.1f)] private float textureSize = 4f;

        [Tooltip("Cells of this layer get a collider that stops vehicles, e.g. water.")]
        [SerializeField] private bool blocksVehicles;

        [SerializeField, HideInInspector] private int firstSlice;
        [SerializeField, HideInInspector] private int variantCount;

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
        public int FirstSlice => firstSlice;
        public int VariantCount => variantCount;

        public void AssignSlices(int first, int count)
        {
            firstSlice = first;
            variantCount = count;
        }
    }
}
