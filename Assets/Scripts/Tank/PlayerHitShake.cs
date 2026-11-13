using PolishTanks.CameraRig;
using PolishTanks.Combat;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Shakes the camera when a gun shell hits the player's tank, harder for bigger calibres (see <see cref="HitShake"/>).
    /// The kick follows the shell's travel, into the plate it struck.
    /// </summary>
    public class PlayerHitShake : MonoBehaviour
    {
        [Tooltip("Smallest calibre in mm that shakes the camera. Machine-gun bullets below it don't.")]
        [SerializeField, Min(0f)] private float minCaliber = 20f;

        [Tooltip("Calibre in mm that shakes with the force below.")]
        [SerializeField, Min(1f)] private float referenceCaliber = 75f;

        [Tooltip("Shake force of a non-penetrating hit at the reference calibre.")]
        [SerializeField, Min(0f)] private float forceAtReference = 0.8f;

        [Inject] private PlayerTank playerTank;
        [Inject] private ICameraShake cameraShake;

        private Damageable damageable;

        private void Start()
        {
            if (playerTank == null || playerTank.Controller == null)
                return;
            damageable = playerTank.Controller.GetComponent<Damageable>();
            if (damageable != null)
                damageable.Hit += OnHit;
        }

        private void OnDestroy()
        {
            if (damageable != null)
                damageable.Hit -= OnHit;
            damageable = null;
        }

        private void OnHit(PenetrationResult result, HitOutcome outcome, float dealt)
        {
            var force = HitShake.Force(damageable.LastCaliber, outcome, minCaliber, referenceCaliber, forceAtReference);
            if (force > 0f)
                cameraShake?.Shake(damageable.transform.position, -damageable.LastFace, force);
        }
    }
}
