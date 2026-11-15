using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>
    /// Wall cutting, roof surface heights, ridge joins and drawing ranks of building compounds.
    /// </summary>
    public class CompoundLayoutTests
    {
        private readonly List<WallSegment> walls = new List<WallSegment>();
        private readonly List<GableEnd> ends = new List<GableEnd>();

        private static BlockShape Flat(float x, float y, float width, float depth, float height) =>
            new BlockShape(new Rect(x, y, width, depth), height);

        private static BlockShape Gable(float x, float y, float width, float depth, float height, float ridge) =>
            new BlockShape(new Rect(x, y, width, depth), height, RoofShape.Gable, ridge);

        [Test]
        public void WallSegments_AdjacentFlatBlocksOfEqualHeight_HideSharedEdge()
        {
            var blocks = new[] { Flat(0f, 0f, 4f, 4f, 3f), Flat(4f, 0f, 4f, 4f, 3f) };

            CompoundLayout.WallSegments(blocks, 0, walls);
            var leftShowsEdge = walls.Any(w => w.Normal == Vector2.right);
            CompoundLayout.WallSegments(blocks, 1, walls);
            var rightShowsEdge = walls.Any(w => w.Normal == Vector2.left);

            Assert.That(leftShowsEdge || rightShowsEdge, Is.False);
        }

        [Test]
        public void WallSegments_TallerBlockNextToLowerOne_StartsAtLowerBlockHeight()
        {
            var blocks = new[] { Flat(0f, 0f, 4f, 4f, 2f), Flat(4f, 0f, 4f, 4f, 5f) };

            CompoundLayout.WallSegments(blocks, 1, walls);

            var shared = walls.Single(w => w.Normal == Vector2.left);
            Assert.That(new[] { shared.BottomA, shared.BottomB, shared.Top }, Is.EqualTo(new[] { 2f, 2f, 5f }));
        }

        [Test]
        public void WallSegments_LowerBlockNextToTallerOne_HasNoWallOnSharedEdge()
        {
            var blocks = new[] { Flat(0f, 0f, 4f, 4f, 2f), Flat(4f, 0f, 4f, 4f, 5f) };

            CompoundLayout.WallSegments(blocks, 0, walls);

            Assert.That(walls.Any(w => w.Normal == Vector2.right), Is.False);
        }

        [Test]
        public void WallSegments_ChimneyOnFlatRoof_StartsAtRoofHeight()
        {
            var blocks = new[] { Flat(0f, 0f, 10f, 10f, 4f), Flat(4f, 4f, 2f, 2f, 7f) };

            CompoundLayout.WallSegments(blocks, 1, walls);

            Assert.That(walls.SelectMany(w => new[] { w.BottomA, w.BottomB }), Is.EqualTo(Enumerable.Repeat(4f, 8)));
        }

        [Test]
        public void WallSegments_BlockOnGableRoof_BottomFollowsRoofSlope()
        {
            var blocks = new[] { Gable(0f, 0f, 10f, 4f, 3f, 2f), Flat(4f, 0.5f, 1f, 1f, 8f) };

            CompoundLayout.WallSegments(blocks, 1, walls);

            var side = walls.Single(w => w.Normal == Vector2.right);
            Assert.That(side.BottomA, Is.LessThan(side.BottomB));
        }

        [Test]
        public void SurfaceHeight_GableMidwayBetweenEaveAndRidge_IsHalfwayUpTheSlope()
        {
            var block = Gable(0f, 0f, 10f, 4f, 3f, 2f);

            Assert.That(CompoundLayout.SurfaceHeight(block, new Vector2(5f, 1f)), Is.EqualTo(4f).Within(1e-4f));
        }

        [Test]
        public void RidgeExtensions_WingMeetsMainRoof_ReachesMainRidge()
        {
            var blocks = new[] { Gable(0f, 0f, 10f, 4f, 3f, 2f), Gable(3f, 4f, 4f, 6f, 3f, 2f) };

            var extensions = CompoundLayout.RidgeExtensions(blocks, 1);

            Assert.That(extensions.x, Is.EqualTo(2f).Within(0.05f));
        }

        [Test]
        public void GableEnds_WingMeetsMainRoof_KeepsOnlyFreeEnd()
        {
            var blocks = new[] { Gable(0f, 0f, 10f, 4f, 3f, 2f), Gable(3f, 4f, 4f, 6f, 3f, 2f) };

            CompoundLayout.GableEnds(blocks, 1, ends);

            Assert.That(ends.Select(e => e.Normal), Is.EqualTo(new[] { Vector2.up }));
        }

        [Test]
        public void Ranks_LowerBlock_GetsLowerRank()
        {
            var blocks = new[] { Flat(0f, 0f, 4f, 4f, 5f), Flat(4f, 0f, 4f, 4f, 2f) };
            var ranks = new int[2];

            CompoundLayout.Ranks(blocks, ranks, 7);

            Assert.That(ranks[1], Is.LessThan(ranks[0]));
        }

        [Test]
        public void Ranks_MoreBlocksThanMaxRank_ClampsToMaxRank()
        {
            var blocks = new[] { Flat(0f, 0f, 1f, 1f, 1f), Flat(1f, 0f, 1f, 1f, 2f), Flat(2f, 0f, 1f, 1f, 3f) };
            var ranks = new int[3];

            CompoundLayout.Ranks(blocks, ranks, 1);

            Assert.That(ranks, Is.EqualTo(new[] { 0, 1, 1 }));
        }
    }
}
