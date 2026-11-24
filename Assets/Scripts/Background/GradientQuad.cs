using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Background
{
    /// <summary>Rectangle with a vertical colour/alpha gradient. Used for the sky and for the fog bands between layers.</summary>
    public class GradientQuad : ProceduralMeshBehaviour
    {
        [SerializeField]
        private Vector2 size = new Vector2(30f, 4f);

        [SerializeField]
        private Color topColor = new Color(1f, 1f, 1f, 0f);

        [SerializeField]
        private Color bottomColor = new Color(1f, 1f, 1f, 0.5f);

        protected override void Build(List<Vector3> vertices, List<int> triangles, List<Color> colors)
        {
            var half = size * 0.5f;
            vertices.Add(new Vector3(-half.x, -half.y, 0f));
            vertices.Add(new Vector3(-half.x, half.y, 0f));
            vertices.Add(new Vector3(half.x, half.y, 0f));
            vertices.Add(new Vector3(half.x, -half.y, 0f));
            colors.Add(bottomColor);
            colors.Add(topColor);
            colors.Add(topColor);
            colors.Add(bottomColor);
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(3);
        }
    }
}
