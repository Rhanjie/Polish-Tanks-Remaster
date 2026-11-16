using System.Collections.Generic;
using System.IO;
using PolishTanks.Ground;
using UnityEditor;
using UnityEngine;

namespace PolishTanks.Editor.Ground
{
    /// <summary>
    /// Bakes the texture variants listed in <see cref="GroundLayerSources"/> into one compressed texture array saved next to
    /// the target <see cref="GroundLayerSet"/>, records which slices belong to which layer and refreshes every ground surface
    /// in the open scenes. Variants are read straight from their image files (PNG or JPG), so their import settings do not
    /// matter, and resampled to the set's slice size. A variant without transparency gets its height (alpha) from its
    /// brightness, so plain opaque textures can be dropped in as they are. Nothing on the set changes unless the bake succeeds.
    /// </summary>
    public static class GroundLayerSetBaker
    {
        public static string Bake(GroundLayerSources sources)
        {
            var set = sources.Target;
            if (set == null)
                throw new System.InvalidOperationException($"{sources.name}: no ground layer set to bake into.");

            var size = Mathf.Max(set.SliceSize, 4);
            var slices = new List<Texture2D>();
            var ranges = new (int first, int count)[set.Count];
            var empty = new List<string>();
            try
            {
                for (var layer = 0; layer < set.Count; layer++)
                {
                    var first = slices.Count;
                    foreach (var variant in sources.VariantsOf(layer))
                    {
                        if (variant != null)
                            slices.Add(BuildSlice(variant, size));
                    }
                    ranges[layer] = (first, slices.Count - first);
                    if (ranges[layer].count == 0)
                        empty.Add(set.Layers[layer].Name);
                }
                if (slices.Count == 0)
                    throw new System.InvalidOperationException($"{sources.name}: no variants to bake.");

                var array = new Texture2DArray(size, size, slices.Count, slices[0].format, true, false)
                {
                    name = set.name + "_Textures",
                    filterMode = FilterMode.Trilinear,
                    wrapMode = TextureWrapMode.Repeat,
                    anisoLevel = 4,
                };
                for (var i = 0; i < slices.Count; i++)
                {
                    for (var mip = 0; mip < array.mipmapCount; mip++)
                        array.SetPixelData(slices[i].GetPixelData<byte>(mip), mip, i);
                }
                array.Apply(false, false);

                var path = Path.ChangeExtension(AssetDatabase.GetAssetPath(set), null) + "_Textures.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(array, existing);
                    Object.DestroyImmediate(array);
                    array = existing;
                }
                else
                {
                    AssetDatabase.CreateAsset(array, path);
                }

                Undo.RecordObject(set, "Bake ground textures");
                for (var layer = 0; layer < set.Count; layer++)
                    set.Layers[layer].AssignSlices(ranges[layer].first, ranges[layer].count);
                set.SetBakedTextures(array);
                EditorUtility.SetDirty(set);
                EditorUtility.SetDirty(array);
                AssetDatabase.SaveAssets();

                foreach (var surface in Object.FindObjectsByType<GroundSurface>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    surface.Rebuild();

                var report = $"{set.name}: baked {slices.Count} slices of {size} px for {set.Count} layers into {path}.";
                if (empty.Count > 0)
                    Debug.LogWarning($"{set.name}: layers without variants show slice 0: {string.Join(", ", empty)}.", sources);
                return report;
            }
            finally
            {
                foreach (var slice in slices)
                    Object.DestroyImmediate(slice);
            }
        }

        private static Texture2D BuildSlice(Texture2D variant, int size)
        {
            var file = Path.GetFullPath(AssetDatabase.GetAssetPath(variant));
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
            try
            {
                if (!File.Exists(file) || !source.LoadImage(File.ReadAllBytes(file)))
                    throw new System.InvalidOperationException($"Ground variant '{variant.name}' at {file} is not a readable PNG or JPG.");

                var pixels = new Color[size * size];
                var opaque = true;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var c = source.GetPixelBilinear((x + 0.5f) / size, (y + 0.5f) / size);
                    opaque &= c.a > 0.99f;
                    pixels[y * size + x] = c;
                }
                if (opaque)
                    HeightFromBrightness(pixels, size);

                var slice = new Texture2D(size, size, TextureFormat.RGBA32, true, false);
                slice.SetPixels(pixels);
                slice.Apply(true);
                EditorUtility.CompressTexture(slice, TextureFormat.BC7, TextureCompressionQuality.Normal);
                return slice;
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        private static void HeightFromBrightness(Color[] pixels, int size)
        {
            var luminance = new float[pixels.Length];
            var mean = 0f;
            for (var i = 0; i < pixels.Length; i++)
            {
                luminance[i] = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
                mean += luminance[i];
            }
            mean /= pixels.Length;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var sum = 0f;
                for (var dy = -1; dy <= 1; dy++)
                for (var dx = -1; dx <= 1; dx++)
                    sum += luminance[(y + dy + size) % size * size + (x + dx + size) % size];
                pixels[y * size + x].a = Mathf.Clamp01(0.45f + (sum / 9f - mean) * 3.5f);
            }
        }
    }
}
