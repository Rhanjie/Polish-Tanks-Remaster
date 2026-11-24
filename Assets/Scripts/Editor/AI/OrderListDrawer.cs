using System;
using System.Linq;
using PolishTanks.AI.Orders;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.Editor.AI
{
    /// <summary>
    /// Inspector for an <see cref="OrderList"/>: each order as a numbered foldout with its own fields and buttons to move
    /// or remove it, and an Add order menu listing every <see cref="Order"/> subclass in the project, so a new order
    /// type shows up without touching this drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(OrderList))]
    public class OrderListDrawer : PropertyDrawer
    {
        private const string OrdersField = "orders";
        private const string LoopField = "loop";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var orders = property.FindPropertyRelative(OrdersField);
            var foldout = new Foldout { text = property.displayName, value = true };
            var items = new VisualElement();
            foldout.Add(items);

            var add = new Button { text = "Add order ▾" };
            add.clicked += () => ShowAddMenu(orders, items);
            foldout.Add(add);
            foldout.Add(new PropertyField(property.FindPropertyRelative(LoopField)));

            Rebuild(items, orders);
            foldout.TrackPropertyValue(orders.FindPropertyRelative("Array.size"), _ => Rebuild(items, orders));
            void OnUndo() => Rebuild(items, orders);
            foldout.RegisterCallback<AttachToPanelEvent>(_ => Undo.undoRedoPerformed += OnUndo);
            foldout.RegisterCallback<DetachFromPanelEvent>(_ => Undo.undoRedoPerformed -= OnUndo);
            return foldout;
        }

        private static void Rebuild(VisualElement items, SerializedProperty orders)
        {
            items.Clear();
            if (orders.serializedObject.targetObject == null)
                return;
            orders.serializedObject.Update();
            for (var i = 0; i < orders.arraySize; i++)
                items.Add(OrderView(orders, i, items));
            items.Bind(orders.serializedObject);
        }

        private static VisualElement OrderView(SerializedProperty orders, int index, VisualElement items)
        {
            var element = orders.GetArrayElementAtIndex(index);
            var value = element.managedReferenceValue;
            var title = $"{index + 1}. {(value != null ? DisplayName(value.GetType()) : "(empty)")}";

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var foldout = new Foldout { text = title, value = true, style = { flexGrow = 1 } };
            row.Add(foldout);
            row.Add(SmallButton("↑", index > 0, () => Move(orders, index, index - 1, items)));
            row.Add(SmallButton("↓", index < orders.arraySize - 1, () => Move(orders, index, index + 1, items)));
            row.Add(SmallButton("✕", true, () => Remove(orders, index, items)));

            var end = element.GetEndProperty();
            var child = element.Copy();
            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end))
                        break;
                    foldout.Add(new PropertyField(child.Copy()));
                }
                while (child.NextVisible(false));
            }
            return row;
        }

        private static Button SmallButton(string text, bool enabled, Action action)
        {
            var button = new Button(action) { text = text, style = { width = 22, alignSelf = Align.FlexStart } };
            button.SetEnabled(enabled);
            return button;
        }

        private static void ShowAddMenu(SerializedProperty orders, VisualElement items)
        {
            var menu = new GenericMenu();
            var types = TypeCache.GetTypesDerivedFrom<Order>()
                .Where(type => !type.IsAbstract && !type.IsGenericType && type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => DisplayName(type));
            foreach (var type in types)
                menu.AddItem(new GUIContent(DisplayName(type)), false, () => Add(orders, type, items));
            menu.ShowAsContext();
        }

        private static void Add(SerializedProperty orders, Type type, VisualElement items)
        {
            orders.serializedObject.Update();
            orders.arraySize++;
            orders.GetArrayElementAtIndex(orders.arraySize - 1).managedReferenceValue = Activator.CreateInstance(type);
            orders.serializedObject.ApplyModifiedProperties();
            Rebuild(items, orders);
        }

        private static void Move(SerializedProperty orders, int from, int to, VisualElement items)
        {
            orders.serializedObject.Update();
            orders.MoveArrayElement(from, to);
            orders.serializedObject.ApplyModifiedProperties();
            Rebuild(items, orders);
        }

        private static void Remove(SerializedProperty orders, int index, VisualElement items)
        {
            orders.serializedObject.Update();
            orders.DeleteArrayElementAtIndex(index);
            orders.serializedObject.ApplyModifiedProperties();
            Rebuild(items, orders);
        }

        public static string DisplayName(Type type)
        {
            var name = type.Name.EndsWith("Order") ? type.Name.Substring(0, type.Name.Length - "Order".Length) : type.Name;
            return ObjectNames.NicifyVariableName(name);
        }
    }
}
