using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PolishTanks.Ground;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class GroundLayerSetTests
    {
        private GroundLayerSet set;
        private List<GroundLayer> layers;

        [SetUp]
        public void SetUp()
        {
            set = ScriptableObject.CreateInstance<GroundLayerSet>();
            layers = new List<GroundLayer> { new GroundLayer("Grass", 1.28f, false), new GroundLayer("Dirt", 4f, false), new GroundLayer("Water", 8f, true) };
            typeof(GroundLayerSet).GetField("layers", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(set, layers);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(set);

        [Test]
        public void Slices_BakedLayers_UseTheirOwnRange()
        {
            layers[0].AssignSlices(0, 4);
            layers[1].AssignSlices(4, 3);
            layers[2].AssignSlices(7, 1);

            Assert.That(set.FirstSlice(1), Is.EqualTo(4));
            Assert.That(set.VariantCount(1), Is.EqualTo(3));
            Assert.That(set.FirstSlice(2), Is.EqualTo(7));
        }

        [Test]
        public void Slices_LayerLeftEmptyInABakedSet_ReadsFirstSliceNotAnotherLayers()
        {
            layers[0].AssignSlices(0, 4);
            layers[1].AssignSlices(4, 3);

            Assert.That(set.FirstSlice(2), Is.EqualTo(0));
            Assert.That(set.VariantCount(2), Is.EqualTo(1));
        }

        [Test]
        public void Slices_UnbakedOrUnknownLayer_FallBackToOneSlicePerLayer()
        {
            Assert.That(set.FirstSlice(2), Is.EqualTo(2));
            Assert.That(set.VariantCount(2), Is.EqualTo(1));
            Assert.That(set.VariantCount(12), Is.EqualTo(1));
        }
    }
}
