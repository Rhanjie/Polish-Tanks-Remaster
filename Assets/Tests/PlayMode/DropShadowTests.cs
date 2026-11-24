using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Rendering;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>A sprite with a <see cref="DropShadow"/> whose sprite is swapped and hidden at run time.</summary>
    public class DropShadowTests
    {
        private readonly List<Object> created = new List<Object>();
        private GameObject go;
        private Texture2D texture;

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(go);
            foreach (var obj in created)
                Object.Destroy(obj);
            created.Clear();
            texture = null;
        }

        private Sprite CreateSprite(int width)
        {
            if (texture == null)
            {
                texture = new Texture2D(8, 8);
                created.Add(texture);
            }
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, 8f), new Vector2(0.5f, 0.5f));
            created.Add(sprite);
            return sprite;
        }

        [UnityTest]
        public IEnumerator Shadow_FollowsSwappedSpriteAndFallsAwayFromTheSun()
        {
            go = new GameObject("Caster");
            go.transform.position = new Vector3(3f, -2f, 0f);
            var source = go.AddComponent<SpriteRenderer>();
            source.sprite = CreateSprite(4);
            var caster = go.AddComponent<DropShadow>();
            var swapped = CreateSprite(8);

            source.sprite = swapped;
            go.transform.rotation = Quaternion.Euler(0f, 0f, 70f);
            yield return null;

            Vector2 offset = caster.Shadow.transform.position - go.transform.position;
            Assert.That(caster.Shadow.sprite, Is.SameAs(swapped));
            Assert.That(Vector2.Angle(offset, SunLight.Direction), Is.LessThan(0.1f));
            Assert.That(caster.Shadow.sortingOrder, Is.LessThan(source.sortingOrder));

            source.enabled = false;
            yield return null;

            Assert.That(caster.Shadow.enabled, Is.False);
        }
    }
}
