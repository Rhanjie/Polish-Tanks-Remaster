using System;
using PolishTanks.AI;
using PolishTanks.AI.Orders;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>A fake driver, empty orders, an unaware target and a doctrine asset wired into an <see cref="AiContext"/>. Dispose destroys the doctrine.</summary>
    internal sealed class AiTestRig : IDisposable
    {
        public AiTestRig()
        {
            Driver = new FakeTankDriver();
            Doctrine = ScriptableObject.CreateInstance<CombatDoctrine>();
            Target = new TargetMemory();
            Orders = new OrderList();
            Context = new AiContext(Driver, Orders, Target, Doctrine);
        }

        public FakeTankDriver Driver { get; }
        public CombatDoctrine Doctrine { get; }
        public TargetMemory Target { get; }
        public OrderList Orders { get; }
        public AiContext Context { get; }

        public AiTestRig WithDoctrine(string field, float value)
        {
            var so = new SerializedObject(Doctrine);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return this;
        }

        public AiTestRig WithDoctrine(string field, bool value)
        {
            var so = new SerializedObject(Doctrine);
            so.FindProperty(field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return this;
        }

        public void Dispose() => Object.DestroyImmediate(Doctrine);
    }
}
