using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Sits on the player's tank and tells <see cref="AiAgents"/> when any of its guns fires and where each of its shots
    /// flies, so enemies see further after a shot and notice shells passing close by. Turns itself off on any tank that
    /// is not the scene's player tank (enemy variants built from the player prefab).
    /// </summary>
    public class PlayerFireReporter : MonoBehaviour
    {
        [Inject] private PlayerTank player;

        private TankGun[] guns;
        private Rigidbody2D body;
        private bool listening;

        private void Start()
        {
            if (player == null || player.Controller == null || player.Controller.gameObject != gameObject)
            {
                enabled = false;
                return;
            }
            guns = GetComponentsInChildren<TankGun>(true);
            body = GetComponent<Rigidbody2D>();
            Listen(true);
        }

        private void OnEnable()
        {
            if (guns != null)
                Listen(true);
        }

        private void OnDisable() => Listen(false);

        private void Listen(bool on)
        {
            if (on == listening)
                return;
            listening = on;
            foreach (var gun in guns)
            {
                if (on)
                    gun.Fired += OnGunFired;
                else
                    gun.Fired -= OnGunFired;
            }
            if (on)
                Projectile.Swept += OnShotSwept;
            else
                Projectile.Swept -= OnShotSwept;
        }

        private void OnGunFired(TankGun gun) => AiAgents.PlayerFired(gun.Mount, Time.time);

        private void OnShotSwept(Rigidbody2D shooter, Vector2 from, Vector2 to)
        {
            if (body != null && shooter == body)
                AiAgents.PlayerShotPassed(from, to, body.position, Time.time);
        }
    }
}
