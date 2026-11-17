using System.Collections.Generic;
using PolishTanks.Ground;
using UnityEngine;

namespace PolishTanks.Editor.Ground
{
    /// <summary>
    /// Editor-only list of the source textures behind a <see cref="GroundLayerSet"/>: entry <c>i</c> holds the variants
    /// of layer <c>i</c>. Kept apart from the set so a build packs only the baked texture array, not the sources.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Ground Layer Sources", fileName = "GroundLayerSources")]
    public class GroundLayerSources : ScriptableObject
    {
        [SerializeField] private GroundLayerSet target;
        [SerializeField] private List<GroundLayerVariants> layers = new List<GroundLayerVariants>();

        public GroundLayerSet Target => target;
        public IReadOnlyList<GroundLayerVariants> Layers => layers;

        public Texture2D[] VariantsOf(int layer) =>
            layer >= 0 && layer < layers.Count && layers[layer].Textures != null ? layers[layer].Textures : System.Array.Empty<Texture2D>();
    }
}
