using NUnit.Framework;
using PolishTanks.Tank.Modules;
using UnityEditor;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Sanity checks on the shipped example loadouts, so a bad edit to the data assets gets caught.</summary>
    public class ExampleLoadoutsTests
    {
        private const string Folder = "Assets/Data/Loadouts/";

        private static TankLoadout Load(string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TankLoadout>(Folder + "Loadout_" + name + ".asset");
            Assert.That(asset, Is.Not.Null, "Loadout_" + name + ".asset is missing");
            return asset;
        }

        private static float Speed(string name) => TankStatsCalculator.Calculate(Load(name)).Drive.maxForwardSpeed;

        [TestCase("Light")]
        [TestCase("Medium")]
        [TestCase("Heavy")]
        public void Validate_ExampleLoadout_HasNoErrors(string name)
        {
            Assert.That(TankStatsCalculator.Validate(Load(name)), Is.Empty);
        }

        [TestCase("Light")]
        [TestCase("Medium")]
        [TestCase("Heavy")]
        public void Calculate_ExampleLoadout_GivesPositiveSpeed(string name)
        {
            Assert.That(Speed(name), Is.GreaterThan(0f));
        }

        [Test]
        public void Calculate_ExampleLoadouts_HeavyIsSlowerThanMedium()
        {
            Assert.That(Speed("Heavy"), Is.LessThan(Speed("Medium")));
        }

        [Test]
        public void Calculate_ExampleLoadouts_MediumIsSlowerThanLight()
        {
            Assert.That(Speed("Medium"), Is.LessThan(Speed("Light")));
        }
    }
}
