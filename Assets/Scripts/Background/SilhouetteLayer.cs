using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Background
{
    /// <summary>One flat-colour background layer: a range of hills with trees on the ridge.</summary>
    public class SilhouetteLayer : ProceduralMeshBehaviour
    {
        [SerializeField]
        private Color color = new Color(0.36f, 0.39f, 0.56f);

        [SerializeField]
        private int seed = 1;

        [SerializeField]
        private HillSettings hills = HillSettings.Default;

        [SerializeField]
        private TreeSettings trees = TreeSettings.Default;

        protected override void Build(List<Vector3> vertices, List<int> triangles, List<Color> colors)
        {
            SilhouetteBuilder.BuildHills(hills, seed, vertices, triangles);
            SilhouetteBuilder.BuildTrees(hills, trees, seed, vertices, triangles);
            FillColor(colors, vertices.Count, color);
        }
    }
}
