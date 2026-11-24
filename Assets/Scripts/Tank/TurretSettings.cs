using System;
using UnityEngine;

namespace PolishTanks.Tank
{
    [Serializable]
    public struct TurretSettings
    {
        [Tooltip("Top traverse speed, degrees per second.")]
        [Min(1f)] public float maxTraverseSpeed;

        [Tooltip("How fast the turret spins up and slows down, degrees/s². Lower feels heavier.")]
        [Min(1f)] public float traverseAcceleration;

        [Tooltip("Seconds the turret takes to close most of the remaining angle. Higher lags more behind the cursor.")]
        [Min(0.01f)] public float responseTime;

        public static TurretSettings Default => new TurretSettings
        {
            maxTraverseSpeed = 90f,
            traverseAcceleration = 240f,
            responseTime = 0.25f,
        };
    }
}
