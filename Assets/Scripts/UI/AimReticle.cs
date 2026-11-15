using PolishTanks.CameraRig;
using PolishTanks.Controls;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.UI
{
    /// <summary>
    /// The player's aiming cursor, drawn in the world in place of the system cursor, which comes back whenever the reticle has nothing to show. A ring around the pointer shows
    /// how far the main gun's shots can stray at that distance right now: it widens when the tank drives, turns or
    /// fires and closes back as the gun settles. A fainter ring does the same for the coaxial machine gun.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class AimReticle : MonoBehaviour
    {
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color gunColor = new Color(1f, 0.93f, 0.78f, 0.9f);
        [SerializeField] private Color machineGunColor = new Color(1f, 0.93f, 0.78f, 0.45f);
        [Tooltip("Dark rim under the gun ring so it reads on light ground.")]
        [SerializeField] private Color outlineColor = new Color(0.08f, 0.07f, 0.06f, 0.6f);
        [Tooltip("Ring line width, screen pixels.")]
        [SerializeField, Min(0.5f)] private float lineWidthPixels = 2f;
        [Tooltip("Smallest ring radius, screen pixels, so a settled gun still shows a ring.")]
        [SerializeField, Min(1f)] private float minRadiusPixels = 7f;
        [SerializeField, Min(8)] private int segments = 48;
        [SerializeField] private int sortingOrder = 30000;

        [Inject] private PlayerTank playerTank;
        [Inject] private IPlayerInput input;
        [Inject] private IGameCamera gameCamera;

        private LineRenderer gunOutline;
        private LineRenderer gunRing;
        private LineRenderer machineGunRing;
        private LineRenderer centreDot;
        private TankGun mainGun;
        private TankGun machineGun;
        private TankController tank;

        public static float Radius(float distance, float spreadDegrees) =>
            Mathf.Max(0f, distance) * Mathf.Tan(Mathf.Clamp(spreadDegrees, 0f, 89f) * Mathf.Deg2Rad);

        private void Awake()
        {
            gunOutline = CreateRing("Gun Ring Outline", outlineColor, segments, -1);
            gunRing = CreateRing("Gun Ring", gunColor, segments);
            machineGunRing = CreateRing("Machine Gun Ring", machineGunColor, segments);
            centreDot = CreateRing("Centre Dot", gunColor, 8);
        }

        private bool shown;

        private void OnDisable()
        {
            shown = false;
            Cursor.visible = true;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (focused && isActiveAndEnabled)
                Cursor.visible = !shown;
        }

        private void LateUpdate()
        {
            if (!FindGuns() || input == null || gameCamera == null)
            {
                Show(false);
                return;
            }

            var pointer = input.PointerScreenPosition;
            var centre = gameCamera.ScreenToWorld(pointer);
            var pixel = (gameCamera.ScreenToWorld(pointer + Vector2.right) - centre).magnitude;
            var width = lineWidthPixels * pixel;

            Show(true);
            var gunRadius = RingRadius(mainGun, centre, pixel);
            Draw(gunOutline, centre, gunRadius, width * 2.5f);
            Draw(gunRing, centre, gunRadius, width);
            var machineGunReady = machineGun != null && machineGun.isActiveAndEnabled;
            machineGunRing.enabled = machineGunReady;
            if (machineGunReady)
                Draw(machineGunRing, centre, RingRadius(machineGun, centre, pixel), width * 0.75f);
            Draw(centreDot, centre, pixel, width);
        }

        private float RingRadius(TankGun gun, Vector2 centre, float pixel)
        {
            var distance = Vector2.Distance(gun.Muzzle.position, centre);
            return Mathf.Max(minRadiusPixels * pixel, Radius(distance, gun.Dispersion));
        }

        private bool FindGuns()
        {
            if (playerTank == null || playerTank.Controller == null || playerTank.Controller.IsDestroyed)
                return false;
            if (tank != playerTank.Controller)
            {
                tank = playerTank.Controller;
                mainGun = TankGun.Find(tank.gameObject, GunMount.Main);
                machineGun = TankGun.Find(tank.gameObject, GunMount.Coaxial);
            }
            return mainGun != null;
        }

        private void Show(bool visible)
        {
            shown = visible;
            Cursor.visible = !visible;
            gunOutline.enabled = visible;
            gunRing.enabled = visible;
            centreDot.enabled = visible;
            if (!visible)
                machineGunRing.enabled = false;
        }

        private void Draw(LineRenderer ring, Vector2 centre, float radius, float width)
        {
            ring.widthMultiplier = width;
            var count = ring.positionCount;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                ring.SetPosition(i, new Vector3(centre.x + Mathf.Cos(angle) * radius, centre.y + Mathf.Sin(angle) * radius, 0f));
            }
        }

        private LineRenderer CreateRing(string ringName, Color color, int points, int orderOffset = 0)
        {
            var ring = new GameObject(ringName).AddComponent<LineRenderer>();
            ring.transform.SetParent(transform, false);
            ring.useWorldSpace = true;
            ring.loop = true;
            ring.positionCount = points;
            ring.sharedMaterial = lineMaterial;
            ring.startColor = color;
            ring.endColor = color;
            ring.numCapVertices = 0;
            ring.numCornerVertices = 2;
            ring.sortingOrder = sortingOrder + orderOffset;
            ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ring.receiveShadows = false;
            ring.enabled = false;
            return ring;
        }
    }
}
