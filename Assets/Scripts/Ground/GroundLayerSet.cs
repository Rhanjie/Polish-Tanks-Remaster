using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Ground
{
    /// <summary>
    /// The kinds of ground a map can be painted with. Slice <c>i</c> of <see cref="Textures"/> holds layer <c>i</c>:
    /// colour in RGB and height in alpha, which the ground shader uses to decide which layer shows through where two meet.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Ground Layer Set", fileName = "GroundLayers")]
    public class GroundLayerSet : ScriptableObject
    {
        public const int MaxLayers = 16;

        [SerializeField] private Texture2DArray textures;
        [SerializeField] private List<GroundLayer> layers = new List<GroundLayer>();

        public Texture2DArray Textures => textures;
        public IReadOnlyList<GroundLayer> Layers => layers;
        public int Count => layers.Count;

        public bool BlocksVehicles(int layer) => layer >= 0 && layer < layers.Count && layers[layer].BlocksVehicles;

        public float TextureSize(int layer) => layer >= 0 && layer < layers.Count ? layers[layer].TextureSize : 1f;
    }
}
