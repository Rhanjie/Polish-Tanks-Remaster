using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Checks that the project-wide input actions bind the Player/Screenshot action to F4.</summary>
    public class ScreenshotInputActionTests
    {
        private const string AssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        [Serializable]
        private class ActionsFile
        {
            public MapData[] maps;
        }

        [Serializable]
        private class MapData
        {
            public string name;
            public BindingData[] bindings;
        }

        [Serializable]
        private class BindingData
        {
            public string path;
            public string action;
        }

        [Test]
        public void PlayerScreenshotAction_ProjectActions_IsBoundToF4()
        {
            var file = JsonUtility.FromJson<ActionsFile>(File.ReadAllText(AssetPath));
            var player = file.maps.Single(m => m.name == "Player");

            var paths = player.bindings.Where(b => b.action == "Screenshot").Select(b => b.path);

            CollectionAssert.Contains(paths.ToArray(), "<Keyboard>/f4");
        }
    }
}
