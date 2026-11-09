using System;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Simplified main gun: reload timer, fire event and muzzle flash. No projectiles yet.
    /// </summary>
    public class TankGun : MonoBehaviour
    {
        [SerializeField] private GunSettings settings = GunSettings.Default;
        [SerializeField] private Transform muzzle;
        [SerializeField] private SpriteRenderer muzzleFlash;

        private ReloadTimer reload;
        private float flashTimeLeft;
        private bool triggerHeld;

        /// <summary>Raised on every shot with the gun that fired.</summary>
        public event Action<TankGun> Fired;

        public Transform Muzzle => muzzle;
        public bool IsReloaded => reload == null || reload.IsReady;
        public float ReloadProgress => reload?.Progress ?? 1f;

        public GunSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                if (reload != null)
                    reload.ReloadTime = value.reloadTime;
            }
        }

        /// <summary>While held, the gun fires every time it finishes reloading.</summary>
        public void SetTrigger(bool held) => triggerHeld = held;

        public bool TryFire()
        {
            if (!isActiveAndEnabled || !reload.TryConsume())
                return false;

            ShowMuzzleFlash();
            Fired?.Invoke(this);
            return true;
        }

        private void Awake()
        {
            reload = new ReloadTimer(settings.reloadTime);
            if (muzzleFlash != null)
                muzzleFlash.enabled = false;
        }

        private void OnDisable()
        {
            triggerHeld = false;
            flashTimeLeft = 0f;
            if (muzzleFlash != null)
                muzzleFlash.enabled = false;
        }

        private void OnValidate()
        {
            if (reload != null)
                reload.ReloadTime = settings.reloadTime;
        }

        private void Update()
        {
            reload.Tick(Time.deltaTime);
            if (triggerHeld)
                TryFire();
            UpdateMuzzleFlash(Time.deltaTime);
        }

        private void ShowMuzzleFlash()
        {
            if (muzzleFlash == null || settings.muzzleFlashDuration <= 0f)
                return;
            flashTimeLeft = settings.muzzleFlashDuration;
            muzzleFlash.enabled = true;
        }

        private void UpdateMuzzleFlash(float deltaTime)
        {
            if (flashTimeLeft <= 0f)
                return;
            flashTimeLeft -= deltaTime;
            if (flashTimeLeft <= 0f && muzzleFlash != null)
                muzzleFlash.enabled = false;
        }
    }
}
