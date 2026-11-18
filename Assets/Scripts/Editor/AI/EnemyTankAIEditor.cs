using PolishTanks.AI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.Editor.AI
{
    /// <summary>
    /// Default inspector for <see cref="EnemyTankAI"/> plus Scene view handles: every point in its orders (drive-to
    /// targets, patrol points, watch points) can be dragged in place and is labelled with its order number.
    /// Points that follow a marker object are moved by moving the marker instead.
    /// </summary>
    [CustomEditor(typeof(EnemyTankAI))]
    public class EnemyTankAIEditor : UnityEditor.Editor
    {
        private const float HandleSize = 0.35f;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();
            var orders = serializedObject.FindProperty("orders.orders");
            if (orders == null)
                return;

            Handles.color = new Color(1f, 0.85f, 0.2f, 1f);
            for (var i = 0; i < orders.arraySize; i++)
            {
                var element = orders.GetArrayElementAtIndex(i);
                var end = element.GetEndProperty();
                var child = element.Copy();
                if (!child.Next(true))
                    continue;
                do
                {
                    if (SerializedProperty.EqualContents(child, end))
                        break;
                    if (child.propertyType == SerializedPropertyType.Vector2)
                        DrawPoint(child, (i + 1).ToString());
                    else if (child.isArray && child.arrayElementType == "Vector2")
                        for (var p = 0; p < child.arraySize; p++)
                            DrawPoint(child.GetArrayElementAtIndex(p), $"{i + 1}.{p + 1}");
                }
                while (child.Next(false));
            }
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawPoint(SerializedProperty point, string label)
        {
            var position = (Vector3)point.vector2Value;
            var size = HandleUtility.GetHandleSize(position) * HandleSize;
            var moved = Handles.FreeMoveHandle(position, size, Vector3.zero, Handles.SphereHandleCap);
            Handles.Label(position + Vector3.up * size * 1.5f, label);
            if (moved != position)
                point.vector2Value = moved;
        }
    }
}
