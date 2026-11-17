using System;
using UnityEngine;

namespace PolishTanks.Editor.Ground
{
    /// <summary>The texture variants of one ground layer, kept in <see cref="GroundLayerSources"/> under the same index as the layer.</summary>
    [Serializable]
    public class GroundLayerVariants
    {
        [Tooltip("Only a label for the inspector; layers are matched by position.")]
        [SerializeField] private string name;

        [Tooltip("Textures of this ground that tile with each other. Colour in RGB, height in alpha; opaque textures get a height from their brightness when baked.")]
        [SerializeField] private Texture2D[] textures = Array.Empty<Texture2D>();

        public GroundLayerVariants() { }

        public GroundLayerVariants(string name, Texture2D[] textures)
        {
            this.name = name;
            this.textures = textures;
        }

        public string Name => name;
        public Texture2D[] Textures => textures;
    }
}
