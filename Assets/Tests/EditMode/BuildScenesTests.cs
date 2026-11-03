using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolishTanks.Tests.EditMode
{
    public class BuildScenesTests
    {
        private static IEnumerable<string> EnabledBuildScenes() =>
            EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path);

        [Test]
        public void BuildSettings_HaveAtLeastOneEnabledScene()
        {
            Assert.That(EnabledBuildScenes(), Is.Not.Empty);
        }

        [TestCaseSource(nameof(EnabledBuildScenes))]
        public void BuildScene_HasNoMissingScripts(string scenePath)
        {
            // A scene the user already has open must not be closed by the test.
            var alreadyLoaded = SceneManager.GetSceneByPath(scenePath).isLoaded;
            var scene = alreadyLoaded
                ? SceneManager.GetSceneByPath(scenePath)
                : EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var objectsWithMissingScripts = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Where(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                    .Select(t => t.name)
                    .ToList();

                Assert.That(objectsWithMissingScripts, Is.Empty,
                    $"Objects with missing scripts in {scenePath}: {string.Join(", ", objectsWithMissingScripts)}");
            }
            finally
            {
                if (!alreadyLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
