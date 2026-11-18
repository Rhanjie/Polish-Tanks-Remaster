using System.Collections.Generic;
using System.Globalization;
using PolishTanks.CameraRig;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Development-build view of the scene's navigation, toggled in play (F2 steps Off, Grid, Clearance).
    /// Grid paints blocked cells red, crates and barrels yellow, cells too tight for a tank orange, and the cells
    /// the last A* search expanded blue, so you can watch how far a search spread before it found its way.
    /// Clearance paints every open cell from red (against a wall) to green (wide open). Every tank driving with a
    /// <see cref="TankPathFollower"/> shows its remaining path in cyan. Middle mouse plans a path from the player's
    /// tank to the cursor without moving; Shift + middle mouse drives the player there (the controls come back on
    /// arrival or when the view is turned off). The overlay sits in front of the z = 0 plane so tall buildings, which
    /// write depth, can't hide it.
    /// </summary>
    [RequireComponent(typeof(NavigationGridSource))]
    public class NavigationDebugOverlay : MonoBehaviour
    {
        private const float PathWidth = 0.3f;
        private const float RefreshInterval = 0.25f;
        private const float HeatmapRange = 8f;
        private const float OverlayDepth = -5f;

        private static readonly Color BlockedColor = new Color(0.9f, 0.12f, 0.1f, 0.55f);
        private static readonly Color SoftColor = new Color(1f, 0.8f, 0.1f, 0.55f);
        private static readonly Color TightColor = new Color(1f, 0.5f, 0.1f, 0.2f);
        private static readonly Color ExpandedColor = new Color(0.25f, 0.5f, 1f, 0.4f);
        private static readonly Color FollowerPathColor = new Color(0.2f, 0.95f, 1f, 0.95f);
        private static readonly Color TestPathColor = new Color(1f, 0.3f, 0.9f, 0.95f);

        [SerializeField] private Key toggleKey = Key.F2;
        [Tooltip("Radius used to mark cells too tight for a tank. Matches a medium hull plus the follower's margin.")]
        [SerializeField, Min(0.1f)] private float tankRadius = 1.6f;
        [SerializeField] private int sortingOrder = 32760;

        [Inject] private PlayerTank playerTank;
        [Inject] private IGameCamera gameCamera;

        private readonly List<LineRenderer> followerLines = new List<LineRenderer>();
        private NavigationGridSource source;
        private GridNavigation navigation;
        private NavigationDebugView view;
        private Texture2D texture;
        private Color32[] pixels;
        private SpriteRenderer gridRenderer;
        private LineRenderer testLine;
        private Material lineMaterial;
        private TankPathFollower[] followers = new TankPathFollower[0];
        private TankPathFollower playerFollower;
        private TankInput playerInput;
        private int paintedVersion = -1;
        private NavigationDebugView paintedView;
        private float nextRefresh;
        private GUIStyle labelStyle;

        public NavigationDebugView View => view;

        public void SetView(NavigationDebugView newView)
        {
            view = newView;
            var visible = view != NavigationDebugView.Off;
            if (navigation == null && visible)
                navigation = source.Navigation as GridNavigation;
            if (navigation != null)
                navigation.RecordSearches = visible;
            if (!visible)
                StopDrivingPlayer();
            if (gridRenderer != null)
                gridRenderer.enabled = visible;
            if (testLine != null)
                testLine.enabled = visible;
            foreach (var line in followerLines)
                line.enabled = visible;
            if (visible)
                Repaint();
        }

        private void Awake()
        {
            source = GetComponent<NavigationGridSource>();
            if (!Debug.isDebugBuild)
                enabled = false;
        }

        private void OnDestroy()
        {
            if (gridRenderer != null && gridRenderer.sprite != null)
                Destroy(gridRenderer.sprite);
            if (texture != null)
                Destroy(texture);
            if (lineMaterial != null)
                Destroy(lineMaterial);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
                SetView((NavigationDebugView)(((int)view + 1) % 3));
            if (view == NavigationDebugView.Off || navigation == null)
                return;

            HandleMouse(keyboard);
            var trace = navigation.LastSearch;
            if ((trace != null && trace.Version != paintedVersion) || paintedView != view)
                Repaint();
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + RefreshInterval;
                followers = FindObjectsByType<TankPathFollower>(FindObjectsSortMode.None);
            }
            DrawFollowerPaths();
        }

        private void HandleMouse(Keyboard keyboard)
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.middleButton.wasPressedThisFrame || gameCamera == null || playerTank == null)
                return;
            var target = gameCamera.ScreenToWorld(mouse.position.ReadValue());
            var drive = keyboard != null && keyboard.shiftKey.isPressed;
            if (drive)
                DrivePlayer(target);
            else
                PlanFromPlayer(target);
        }

        private void PlanFromPlayer(Vector2 target)
        {
            var radius = PlayerFollower().AgentRadius;
            var result = navigation.FindPath(new PathRequest(playerTank.Transform.position, target, radius, 1f, PlayerFollower().PivotRadius));
            DrawPath(TestLine(), playerTank.Transform.position, result.Points);
        }

        private void DrivePlayer(Vector2 target)
        {
            var follower = PlayerFollower();
            if (playerTank.Controller.IsDestroyed)
                return;
            if (playerInput != null)
                playerInput.enabled = false;
            if (!follower.MoveTo(target))
                RestorePlayerInput(follower);
        }

        private TankPathFollower PlayerFollower()
        {
            if (playerFollower != null)
                return playerFollower;
            var tank = playerTank.Controller.gameObject;
            playerInput = tank.GetComponent<TankInput>();
            playerFollower = tank.GetComponent<TankPathFollower>();
            if (playerFollower == null)
                playerFollower = tank.AddComponent<TankPathFollower>();
            playerFollower.Navigation = navigation;
            playerFollower.Arrived += RestorePlayerInput;
            playerFollower.PathFailed += RestorePlayerInput;
            return playerFollower;
        }

        private void StopDrivingPlayer()
        {
            if (playerFollower == null || !playerFollower.IsMoving)
                return;
            playerFollower.Stop();
            RestorePlayerInput(playerFollower);
        }

        private void RestorePlayerInput(TankPathFollower _)
        {
            if (playerInput != null && !playerTank.Controller.IsDestroyed)
                playerInput.enabled = true;
        }

        private void DrawFollowerPaths()
        {
            while (followerLines.Count < followers.Length)
                followerLines.Add(CreateLine($"Follower Path {followerLines.Count}", FollowerPathColor));
            for (var i = 0; i < followerLines.Count; i++)
            {
                var follower = i < followers.Length ? followers[i] : null;
                var line = followerLines[i];
                if (follower == null || !follower.IsMoving)
                {
                    line.positionCount = 0;
                    continue;
                }
                DrawPath(line, follower.transform.position, follower.Path);
            }
        }

        private static void DrawPath(LineRenderer line, Vector2 from, IReadOnlyList<Vector2> points)
        {
            line.positionCount = points.Count + 1;
            line.SetPosition(0, new Vector3(from.x, from.y, OverlayDepth));
            for (var i = 0; i < points.Count; i++)
                line.SetPosition(i + 1, new Vector3(points[i].x, points[i].y, OverlayDepth));
        }

        private void Repaint()
        {
            var grid = navigation != null ? navigation.Grid : null;
            if (grid == null)
                return;
            EnsureTexture(grid);

            for (var index = 0; index < grid.CellCount; index++)
                pixels[index] = view == NavigationDebugView.Clearance ? ClearanceColor(grid, index) : GridColor(grid, index);

            var trace = navigation.LastSearch;
            if (view == NavigationDebugView.Grid && trace != null)
                foreach (var index in trace.ExpandedCells)
                    pixels[index] = ExpandedColor;

            texture.SetPixels32(pixels);
            texture.Apply(false);
            paintedVersion = trace != null ? trace.Version : -1;
            paintedView = view;
        }

        private Color32 GridColor(NavGrid grid, int index)
        {
            var cell = grid.CellAt(index);
            if (grid.IsBlocked(cell))
                return BlockedColor;
            if (grid.SoftCost(cell) > 0f)
                return SoftColor;
            return grid.Clearance(cell) < tankRadius ? TightColor : Color.clear;
        }

        private static Color32 ClearanceColor(NavGrid grid, int index)
        {
            var cell = grid.CellAt(index);
            if (grid.IsBlocked(cell))
                return new Color(0.25f, 0f, 0f, 0.7f);
            var t = Mathf.Clamp01(grid.Clearance(cell) / HeatmapRange);
            var color = t < 0.5f ? Color.Lerp(Color.red, Color.yellow, t * 2f) : Color.Lerp(Color.yellow, Color.green, (t - 0.5f) * 2f);
            color.a = 0.45f;
            return color;
        }

        private void EnsureTexture(NavGrid grid)
        {
            if (texture != null && texture.width == grid.Width && texture.height == grid.Height)
                return;
            if (gridRenderer != null && gridRenderer.sprite != null)
                Destroy(gridRenderer.sprite);
            if (texture != null)
                Destroy(texture);

            texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "Navigation Debug",
            };
            pixels = new Color32[grid.CellCount];

            if (gridRenderer == null)
            {
                var holder = new GameObject("Navigation Debug Grid");
                holder.transform.SetParent(transform, false);
                gridRenderer = holder.AddComponent<SpriteRenderer>();
                gridRenderer.sortingOrder = sortingOrder;
                gridRenderer.material = LineMaterial();
            }
            gridRenderer.transform.position = new Vector3(grid.Origin.x, grid.Origin.y, OverlayDepth);
            gridRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, grid.Width, grid.Height), Vector2.zero, 1f / grid.CellSize);
        }

        private LineRenderer TestLine()
        {
            if (testLine == null)
                testLine = CreateLine("Navigation Test Path", TestPathColor);
            return testLine;
        }

        private LineRenderer CreateLine(string lineName, Color color)
        {
            var holder = new GameObject(lineName);
            holder.transform.SetParent(transform, false);
            var line = holder.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = PathWidth;
            line.startColor = color;
            line.endColor = color;
            line.material = LineMaterial();
            line.sortingOrder = sortingOrder + 1;
            line.positionCount = 0;
            return line;
        }

        private Material LineMaterial()
        {
            if (lineMaterial == null)
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
            return lineMaterial;
        }

        private void OnGUI()
        {
            if (view == NavigationDebugView.Off)
                return;
            var trace = navigation != null ? navigation.LastSearch : null;
            var text = string.Format(NavigationDebugText.HeaderFormat, NavigationDebugText.ViewName(view)) + "\n" + NavigationDebugText.Help;
            if (trace != null)
                text += "\n" + string.Format(CultureInfo.InvariantCulture, NavigationDebugText.LastSearchFormat,
                    NavigationDebugText.StatusName(trace.Result.Status), trace.ExpandedCells.Count, trace.Milliseconds, trace.Result.Length(trace.From));
            labelStyle ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 13, padding = new RectOffset(8, 8, 6, 6) };
            GUI.Box(new Rect(12f, 12f, 460f, trace != null ? 86f : 66f), text, labelStyle);
        }
    }
}
