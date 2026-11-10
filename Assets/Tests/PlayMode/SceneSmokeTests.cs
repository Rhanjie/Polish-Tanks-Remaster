using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class SceneSmokeTests
    {
        private const int FramesToRun = 60;

        private static IEnumerable<string> BuildScenePaths() =>
            Enumerable.Range(0, SceneManager.sceneCountInBuildSettings)
                .Select(SceneUtility.GetScenePathByBuildIndex)
                .Where(path => !string.IsNullOrEmpty(path) && !path.Contains("InitTestScene"));

        [UnityTest]
        public IEnumerator BuildScene_LoadsAndRunsWithoutErrors([ValueSource(nameof(BuildScenePaths))] string scenePath)
        {
            var load = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, $"Scene {scenePath} could not be loaded");
            while (!load.isDone)
                yield return null;

            for (var i = 0; i < FramesToRun; i++)
                yield return null;

            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(scenePath));
        }

        [UnityTearDown]
        public IEnumerator UnloadLoadedScene()
        {
            var loaded = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("SceneSmokeTests_Empty_" + System.Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (loaded.IsValid() && loaded.isLoaded && loaded != empty)
            {
                var unload = SceneManager.UnloadSceneAsync(loaded);
                while (unload != null && !unload.isDone)
                    yield return null;
            }
        }
    }
}
