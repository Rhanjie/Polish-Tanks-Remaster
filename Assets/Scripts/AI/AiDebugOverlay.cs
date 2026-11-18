using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.AI
{
    /// <summary>
    /// Development-build view of every <see cref="EnemyTankAI"/>, toggled in play with F3. Each enemy shows its sight
    /// range as a circle (red while it has the player spotted), a line to where it thinks the player is, and a label with
    /// what it is doing, its current order, what it knows about the player and how its shells have done per plate.
    /// Created on its own in the editor and development builds, so scenes need no setup.
    /// </summary>
    public class AiDebugOverlay : MonoBehaviour
    {
        private const Key ToggleKey = Key.F3;
        private const int CircleSegments = 48;
        private const float LineWidth = 0.15f;
        private const float OverlayDepth = -5f;
        private const int SortingOrder = 32761;
        private const float LabelWidth = 320f;
        private const float LabelHeight = 70f;

        private static readonly Color CalmColor = new Color(1f, 0.85f, 0.2f, 0.8f);
        private static readonly Color AlertColor = new Color(1f, 0.25f, 0.15f, 0.9f);
        private static readonly Color LeadColor = new Color(1f, 0.5f, 0.9f, 0.9f);

        private readonly Dictionary<EnemyTankAI, (LineRenderer sight, LineRenderer lead)> lines =
            new Dictionary<EnemyTankAI, (LineRenderer, LineRenderer)>();
        private readonly List<EnemyTankAI> stale = new List<EnemyTankAI>();
        private readonly StringBuilder text = new StringBuilder();
        private Material lineMaterial;
        private GUIStyle labelStyle;
        private bool visible;

        public bool Visible => visible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (!Debug.isDebugBuild)
                return;
            var host = new GameObject("[AI Debug]");
            DontDestroyOnLoad(host);
            host.AddComponent<AiDebugOverlay>();
        }

        public void SetVisible(bool show)
        {
            visible = show;
            if (!visible)
                ClearLines();
        }

        private void OnDestroy()
        {
            ClearLines();
            if (lineMaterial != null)
                Destroy(lineMaterial);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[ToggleKey].wasPressedThisFrame)
                SetVisible(!visible);
            if (!visible)
                return;

            foreach (var ai in EnemyTankAI.All)
                DrawLines(ai);
            RemoveStaleLines();
        }

        private void DrawLines(EnemyTankAI ai)
        {
            if (!lines.TryGetValue(ai, out var pair))
            {
                pair = (NewLine(ai.name + " sight", true), NewLine(ai.name + " lead", false));
                lines[ai] = pair;
            }

            var centre = (Vector2)ai.transform.position;
            var color = ai.IsTargetSpotted ? AlertColor : CalmColor;
            pair.sight.startColor = pair.sight.endColor = color;
            pair.sight.positionCount = CircleSegments;
            var radius = ai.SightRange;
            for (var i = 0; i < CircleSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / CircleSegments;
                pair.sight.SetPosition(i, new Vector3(centre.x + Mathf.Cos(angle) * radius, centre.y + Mathf.Sin(angle) * radius, OverlayDepth));
            }

            var target = ai.Target;
            pair.lead.enabled = target.IsKnown;
            if (target.IsKnown)
            {
                pair.lead.SetPosition(0, new Vector3(centre.x, centre.y, OverlayDepth));
                pair.lead.SetPosition(1, new Vector3(target.LastKnownPosition.x, target.LastKnownPosition.y, OverlayDepth));
            }
        }

        private void RemoveStaleLines()
        {
            stale.Clear();
            foreach (var ai in lines.Keys)
                if (ai == null || !ai.isActiveAndEnabled)
                    stale.Add(ai);
            foreach (var ai in stale)
            {
                DestroyLines(lines[ai]);
                lines.Remove(ai);
            }
        }

        private LineRenderer NewLine(string lineName, bool loop)
        {
            if (lineMaterial == null)
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
            var line = new GameObject(lineName).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.sharedMaterial = lineMaterial;
            line.widthMultiplier = LineWidth;
            line.loop = loop;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.sortingOrder = SortingOrder;
            line.startColor = line.endColor = LeadColor;
            return line;
        }

        private void ClearLines()
        {
            foreach (var pair in lines.Values)
                DestroyLines(pair);
            lines.Clear();
        }

        private static void DestroyLines((LineRenderer sight, LineRenderer lead) pair)
        {
            if (pair.sight != null)
                Destroy(pair.sight.gameObject);
            if (pair.lead != null)
                Destroy(pair.lead.gameObject);
        }

        private void OnGUI()
        {
            if (!visible)
                return;
            var view = Camera.main;
            if (view == null)
                return;
            labelStyle ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12, wordWrap = false };

            GUI.Label(new Rect(10f, 10f, 80f, 22f), AiDebugText.Header, labelStyle);
            foreach (var ai in EnemyTankAI.All)
            {
                var screen = view.WorldToScreenPoint(ai.transform.position);
                if (screen.z < 0f)
                    continue;
                var rect = new Rect(screen.x - LabelWidth * 0.5f, Screen.height - screen.y - LabelHeight - 40f, LabelWidth, LabelHeight);
                GUI.Label(rect, Describe(ai), labelStyle);
            }
        }

        private string Describe(EnemyTankAI ai)
        {
            text.Clear();
            var order = ai.Orders.Current;
            var orderText = ai.Orders.Orders.Count == 0 ? AiDebugText.NoOrders
                : ai.Orders.IsFinished ? AiDebugText.OrdersDone
                : AiDebugText.OrderName(order != null ? order : ai.Orders.Orders[ai.Orders.Index]);
            var behaviourFormat = ai.ScriptedCombat ? AiDebugText.ScriptedBehaviourFormat : AiDebugText.BehaviourFormat;
            text.AppendFormat(behaviourFormat, AiDebugText.BehaviourName(ai.CurrentBehaviour), orderText);
            text.AppendLine();

            var target = ai.Target;
            var awareness = AiDebugText.AwarenessName(target.Awareness);
            if (target.Awareness == TargetAwareness.Remembered)
                text.AppendFormat(AiDebugText.TargetAgeFormat, awareness, target.SecondsSinceContact(Time.time));
            else
                text.AppendFormat(AiDebugText.TargetFormat, awareness);
            text.AppendLine();
            text.Append(AiDebugText.SideCounts(ai.Penetrations));
            return text.ToString();
        }
    }
}
