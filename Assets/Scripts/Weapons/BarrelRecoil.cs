using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>Sits on the turret and slides its <see cref="TurretView"/> back along the barrel on a shot, then springs it home.</summary>
    public class BarrelRecoil : MonoBehaviour
    {
        private Transform target;
        private RecoilSpring spring;
        private Vector3 restPosition;

        public float Offset => spring?.Offset ?? 0f;

        public void Kick(float distance, float returnTime)
        {
            if (spring == null)
                return;
            spring.ReturnTime = returnTime;
            spring.Kick(distance);
        }

        private void Awake()
        {
            var view = GetComponentInChildren<TurretView>();
            target = view != null ? view.transform : transform;
            restPosition = target.localPosition;
            spring = new RecoilSpring(0.3f);
        }

        private void OnDisable()
        {
            if (target != null)
                target.localPosition = restPosition;
        }

        private void LateUpdate()
        {
            spring.Step(Time.deltaTime);
            target.localPosition = restPosition + Vector3.down * spring.Offset;
        }
    }
}
