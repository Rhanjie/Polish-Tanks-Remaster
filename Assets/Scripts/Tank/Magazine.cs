using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>Rounds in a belt or magazine and the pause to refill it. Size 0 means fed continuously.</summary>
    public sealed class Magazine
    {
        private float reloadLeft;

        public int Size { get; private set; }
        public float ReloadTime { get; private set; }
        public int Rounds { get; private set; }

        public bool IsUnlimited => Size <= 0;
        public bool IsReloading => reloadLeft > 0f;
        public bool CanFire => IsUnlimited || (!IsReloading && Rounds > 0);

        public float ReloadProgress => IsReloading && ReloadTime > 0f ? 1f - reloadLeft / ReloadTime : 1f;

        public Magazine(int size, float reloadTime)
        {
            Configure(size, reloadTime);
        }

        public void Configure(int size, float reloadTime)
        {
            Size = Mathf.Max(0, size);
            ReloadTime = Mathf.Max(0f, reloadTime);
            Rounds = Size;
            reloadLeft = 0f;
        }

        public void Consume()
        {
            if (IsUnlimited || Rounds <= 0)
                return;
            Rounds--;
            if (Rounds == 0)
                StartReload();
        }

        public void Tick(float deltaTime)
        {
            if (!IsReloading)
                return;
            reloadLeft -= deltaTime;
            if (reloadLeft <= 0f)
            {
                reloadLeft = 0f;
                Rounds = Size;
            }
        }

        private void StartReload()
        {
            if (ReloadTime <= 0f)
                Rounds = Size;
            else
                reloadLeft = ReloadTime;
        }
    }
}
