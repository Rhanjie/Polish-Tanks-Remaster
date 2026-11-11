using NUnit.Framework;
using PolishTanks.Tank.Modules;
using UnityEditor;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Guard that machine-gun assets never shove what they hit; only gun shells push.</summary>
    public class MachineGunAssetsTests
    {
        [Test]
        public void MachineGunAssets_ImpactImpulse_IsZero()
        {
            var guids = AssetDatabase.FindAssets("t:CoaxialMachineGunModule", new[] { "Assets/Data/Modules/MachineGuns" });
            Assert.That(guids, Is.Not.Empty);

            foreach (var guid in guids)
            {
                var module = AssetDatabase.LoadAssetAtPath<CoaxialMachineGunModule>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(module.ImpactImpulse, Is.EqualTo(0f), AssetDatabase.GUIDToAssetPath(guid));
            }
        }
    }
}
