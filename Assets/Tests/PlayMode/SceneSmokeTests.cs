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

        private static IEnumerable<int> BuildSceneIndices() =>
            Enumerable.Range(0, SceneManager.sceneCountInBuildSettings);

        // Any error or exception logged while the scene runs fails the test (Unity Test Framework default).
        [UnityTest]
        public IEnumerator BuildScene_LoadsAndRunsWithoutErrors([ValueSource(nameof(BuildSceneIndices))] int buildIndex)
        {
            var load = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            while (!load.isDone)
                yield return null;

            for (var i = 0; i < FramesToRun; i++)
                yield return null;

            Assert.That(SceneManager.GetActiveScene().buildIndex, Is.EqualTo(buildIndex));
        }
    }
}
