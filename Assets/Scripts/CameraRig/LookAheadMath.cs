using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary>
    /// Where the commander looks: the camera slides toward the cursor by a share of the distance from the screen
    /// centre to it. The cursor is read in screen space, not world space, so moving the camera never drags the
    /// target further out; the offset is bounded by the screen edge and a hard cap.
    /// </summary>
    public static class LookAheadMath
    {
        public static Vector2 Offset(Vector2 pointerScreen, Vector2 screenSize, float orthographicSize,
            float strength, float maxDistance)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
                return Vector2.zero;

            var viewport = new Vector2(Mathf.Clamp01(pointerScreen.x / screenSize.x), Mathf.Clamp01(pointerScreen.y / screenSize.y));
            var fromCentre = (viewport - new Vector2(0.5f, 0.5f)) * 2f;
            var halfExtents = new Vector2(orthographicSize * screenSize.x / screenSize.y, orthographicSize);
            var offset = Vector2.Scale(fromCentre, halfExtents) * strength;
            return Vector2.ClampMagnitude(offset, Mathf.Max(0f, maxDistance));
        }
    }
}
