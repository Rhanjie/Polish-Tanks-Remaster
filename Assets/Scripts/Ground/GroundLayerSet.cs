using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Ground
{
    /// <summary>
    /// The kinds of ground a map can be painted with. <see cref="Textures"/> is baked in the editor from every layer's
    /// variants: colour in RGB and height in alpha, which the ground shader uses to decide which layer shows through
    /// where two meet. In a set that was never baked layer <c>i</c> reads slice <c>i</c>; in a baked set a layer without
    /// variants reads slice 0, so it never shows another layer's texture or reads past the array.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Ground Layer Set", fileName = "GroundLayers")]
    public class GroundLayerSet : ScriptableObject
    {
        public const int MaxLayers = 16;

        [SerializeField] private Texture2DArray textures;

        [Tooltip("Side of every baked texture slice in pixels. Variants of another size are resampled.")]
        [SerializeField] private int sliceSize = 512;

        [SerializeField] private List<GroundLayer> layers = new List<GroundLayer>();

        public Texture2DArray Textures => textures;
        public int SliceSize => sliceSize;
        public IReadOnlyList<GroundLayer> Layers => layers;
        public int Count => layers.Count;

        public bool BlocksVehicles(int layer) => IsValid(layer) && layers[layer].BlocksVehicles;

        public float TextureSize(int layer) => IsValid(layer) ? layers[layer].TextureSize : 1f;

        public int FirstSlice(int layer)
        {
            if (IsValid(layer) && layers[layer].VariantCount > 0)
                return layers[layer].FirstSlice;
            return IsBaked ? 0 : Mathf.Max(layer, 0);
        }

        public int VariantCount(int layer) => IsValid(layer) ? Mathf.Max(layers[layer].VariantCount, 1) : 1;

        public void SetBakedTextures(Texture2DArray baked) => textures = baked;

        private bool IsValid(int layer) => layer >= 0 && layer < layers.Count;

        private bool IsBaked
        {
            get
            {
                foreach (var layer in layers)
                {
                    if (layer.VariantCount > 0)
                        return true;
                }
                return false;
            }
        }
    }
}
