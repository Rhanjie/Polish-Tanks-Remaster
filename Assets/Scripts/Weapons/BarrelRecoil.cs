using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>Slides a visual (the turret sprite) back along its parent's -Y on a shot and springs it home.</summary>
    public class BarrelRecoil : MonoBehaviour
    {
        [Tooltip("Transform that kicks back. Its parent's up axis must point down the barrel.")]
        [SerializeField] private Transform target;

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
            if (target == null)
                target = transform;
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
