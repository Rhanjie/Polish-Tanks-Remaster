using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// One block of a building compound as plain data: an axis-aligned rectangle in the compound's space,
    /// eave height and roof form. <see cref="CompoundLayout"/> works on these, so it needs no scene.
    /// </summary>
    public readonly struct BlockShape
    {
        public readonly Rect Rect;
        public readonly float Height;
        public readonly RoofShape Roof;
        public readonly float RidgeHeight;

        public BlockShape(Rect rect, float height, RoofShape roof = RoofShape.Flat, float ridgeHeight = 0f)
        {
            Rect = rect;
            Height = height;
            Roof = roof;
            RidgeHeight = roof == RoofShape.Gable ? Mathf.Max(0f, ridgeHeight) : 0f;
        }

        public bool HasGable => Roof == RoofShape.Gable && RidgeHeight > 0f;
        public float TopHeight => Height + RidgeHeight;
        public bool RidgeAlongX => Rect.width >= Rect.height;

        public bool SameAs(in BlockShape other) =>
            Rect == other.Rect && Height == other.Height && Roof == other.Roof && RidgeHeight == other.RidgeHeight;
    }
}
