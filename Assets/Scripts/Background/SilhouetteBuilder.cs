using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Background
{
    /// <summary>
    /// Builds flat-colour silhouette geometry (a hill ridge with trees standing on it) as plain vertex/index lists.
    /// Everything is driven by a seed, so the same settings always give the same landscape.
    /// </summary>
    public static class SilhouetteBuilder
    {
        private const int PineTiers = 3;
        private const int RoundCrownSegments = 14;
        // Trees are sunk slightly into the ridge so they never float above a slope.
        private const float RootDepthRatio = 0.08f;

        public static float RidgeHeight(float x, in HillSettings hills, int seed)
        {
            var phase = SeedPhases(seed);
            var f = hills.frequency;
            var wave = 0.6f * Mathf.Sin(x * f + phase.x)
                       + 0.3f * Mathf.Sin(x * f * 2.3f + phase.y)
                       + 0.1f * Mathf.Sin(x * f * 5.1f + phase.z);
            return hills.baseHeight + hills.amplitude * wave;
        }

        public static void BuildHills(in HillSettings hills, int seed, List<Vector3> vertices, List<int> triangles)
        {
            var segments = Mathf.Max(1, hills.segments);
            var left = -hills.width * 0.5f;
            var step = hills.width / segments;

            for (var i = 0; i <= segments; i++)
            {
                var x = left + i * step;
                var start = vertices.Count;
                vertices.Add(new Vector3(x, hills.bottom, 0f));
                vertices.Add(new Vector3(x, Mathf.Max(hills.bottom, RidgeHeight(x, hills, seed)), 0f));

                if (i == 0)
                    continue;

                // Quad between the previous column (start-2, start-1) and this one (start, start+1), clockwise.
                triangles.Add(start - 2);
                triangles.Add(start - 1);
                triangles.Add(start + 1);
                triangles.Add(start - 2);
                triangles.Add(start + 1);
                triangles.Add(start);
            }
        }

        public static void BuildTrees(in HillSettings hills, in TreeSettings trees, int seed, List<Vector3> vertices, List<int> triangles)
        {
            if (trees.count <= 0)
                return;

            var random = new System.Random(seed * 7919 + 17);
            var minHeight = Mathf.Min(trees.minHeight, trees.maxHeight);
            var maxHeight = Mathf.Max(trees.minHeight, trees.maxHeight);
            var slot = hills.width / trees.count;

            for (var i = 0; i < trees.count; i++)
            {
                // Jittered slots spread trees evenly while still looking random.
                var x = -hills.width * 0.5f + slot * (i + (float)random.NextDouble());
                var height = Mathf.Lerp(minHeight, maxHeight, (float)random.NextDouble());
                var groundY = RidgeHeight(x, hills, seed) - height * RootDepthRatio;
                var round = random.NextDouble() < trees.roundTreeShare;

                if (round)
                    AppendRoundTree(new Vector2(x, groundY), height, height * trees.widthRatio * 1.3f, vertices, triangles);
                else
                    AppendPine(new Vector2(x, groundY), height, height * trees.widthRatio, vertices, triangles);
            }
        }

        public static void AppendPine(Vector2 root, float height, float width, List<Vector3> vertices, List<int> triangles)
        {
            var trunkHeight = height * 0.2f;
            AppendRect(root.x - width * 0.06f, root.y, width * 0.12f, trunkHeight, vertices, triangles);

            var crownBottom = root.y + trunkHeight * 0.6f;
            var crownHeight = height - (crownBottom - root.y);
            var tierHeight = crownHeight / PineTiers * 1.45f;
            for (var t = 0; t < PineTiers; t++)
            {
                var baseY = crownBottom + t * (crownHeight - tierHeight) / (PineTiers - 1);
                var halfWidth = width * 0.5f * (1f - t * 0.24f);
                AppendTriangle(
                    new Vector2(root.x - halfWidth, baseY),
                    new Vector2(root.x, baseY + tierHeight),
                    new Vector2(root.x + halfWidth, baseY),
                    vertices, triangles);
            }
        }

        public static void AppendRoundTree(Vector2 root, float height, float width, List<Vector3> vertices, List<int> triangles)
        {
            var trunkHeight = height * 0.45f;
            AppendRect(root.x - width * 0.07f, root.y, width * 0.14f, trunkHeight, vertices, triangles);

            var radiusX = width * 0.5f;
            var radiusY = (height - trunkHeight) * 0.6f;
            var center = new Vector2(root.x, root.y + height - radiusY);

            var centerIndex = vertices.Count;
            vertices.Add(center);
            for (var s = 0; s < RoundCrownSegments; s++)
            {
                var angle = 2f * Mathf.PI * s / RoundCrownSegments;
                vertices.Add(new Vector3(center.x + Mathf.Cos(angle) * radiusX, center.y + Mathf.Sin(angle) * radiusY, 0f));
            }
            for (var s = 0; s < RoundCrownSegments; s++)
            {
                var next = (s + 1) % RoundCrownSegments;
                // Counter-clockwise angles, so the winding is flipped to stay clockwise like the rest.
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + 1 + next);
                triangles.Add(centerIndex + 1 + s);
            }
        }

        private static void AppendRect(float x, float y, float width, float height, List<Vector3> vertices, List<int> triangles)
        {
            var start = vertices.Count;
            vertices.Add(new Vector3(x, y, 0f));
            vertices.Add(new Vector3(x, y + height, 0f));
            vertices.Add(new Vector3(x + width, y + height, 0f));
            vertices.Add(new Vector3(x + width, y, 0f));
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        private static void AppendTriangle(Vector2 a, Vector2 b, Vector2 c, List<Vector3> vertices, List<int> triangles)
        {
            var start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }

        private static Vector3 SeedPhases(int seed)
        {
            var random = new System.Random(seed);
            return new Vector3(
                (float)(random.NextDouble() * Math.PI * 2.0),
                (float)(random.NextDouble() * Math.PI * 2.0),
                (float)(random.NextDouble() * Math.PI * 2.0));
        }
    }
}
