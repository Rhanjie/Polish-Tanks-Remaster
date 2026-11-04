using System;
using System.Collections;
using NUnit.Framework;
using PolishTanks.Bouncing;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PolishTanks.Tests.PlayMode
{
    public class BounceBallTests
    {
        private const float Timeout = 5f;
        private GameObject _floor;
        private GameObject _ball;

        [TearDown]
        public void TearDown()
        {
            if (_floor != null) Object.Destroy(_floor);
            if (_ball != null) Object.Destroy(_ball);
        }

        private void CreateScene()
        {
            _floor = new GameObject("TestFloor");
            _floor.transform.position = new Vector3(0f, -1f, 0f);
            _floor.AddComponent<BoxCollider2D>().size = new Vector2(20f, 1f);

            // Inactive while configuring so Awake/OnEnable run with everything attached.
            _ball = new GameObject("TestBall");
            _ball.SetActive(false);
            _ball.transform.position = new Vector3(0f, 2f, 0f);
            _ball.AddComponent<Rigidbody2D>().gravityScale = 1f;
            _ball.AddComponent<CircleCollider2D>().radius = 0.25f;
            _ball.AddComponent<SpriteRenderer>().color = Color.white;
            _ball.AddComponent<BounceCounter>();
            _ball.AddComponent<BounceColorChanger>();
            _ball.SetActive(true);
        }

        private static IEnumerator WaitUntil(Func<bool> condition)
        {
            var start = Time.realtimeSinceStartup;
            while (!condition() && Time.realtimeSinceStartup - start < Timeout)
                yield return null;
        }

        [UnityTest]
        public IEnumerator Ball_FallingOnFloor_IncrementsBounceCount()
        {
            CreateScene();
            var counter = _ball.GetComponent<BounceCounter>();

            yield return WaitUntil(() => counter.Tracker.Count > 0);

            Assert.GreaterOrEqual(counter.Tracker.Count, 1);
        }

        [UnityTest]
        public IEnumerator Ball_AfterBounce_ChangesSpriteColor()
        {
            CreateScene();
            var counter = _ball.GetComponent<BounceCounter>();
            var changer = _ball.GetComponent<BounceColorChanger>();
            var spriteRenderer = _ball.GetComponent<SpriteRenderer>();

            yield return WaitUntil(() => counter.Tracker.Count > 0);
            // The colour blends over 0.12 s, so wait until the renderer reaches the target.
            yield return WaitUntil(() => spriteRenderer.color == changer.TargetColor);

            Assert.AreNotEqual(Color.white, spriteRenderer.color);
        }

        [UnityTest]
        public IEnumerator Ball_EveryCountedBounce_RaisesColorChangedOnce()
        {
            CreateScene();
            var counter = _ball.GetComponent<BounceCounter>();
            var changer = _ball.GetComponent<BounceColorChanger>();
            var changes = 0;
            changer.ColorChanged += _ => changes++;

            yield return WaitUntil(() => counter.Tracker.Count >= 2);

            Assert.AreEqual(counter.Tracker.Count, changes);
        }
    }
}
