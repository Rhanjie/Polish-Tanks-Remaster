using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Small bar above a <see cref="Damageable"/> parent, shown only once it has taken damage.
    /// Builds its own sprites, keeps a fixed world orientation and hides when the owner dies.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        private const int SortingOrder = 40;

        [SerializeField] private Vector2 offset = new Vector2(0f, 2.8f);
        [SerializeField] private Vector2 size = new Vector2(2f, 0.22f);
        [SerializeField] private Color fullColor = new Color(0.45f, 0.85f, 0.35f);
        [SerializeField] private Color lowColor = new Color(0.9f, 0.25f, 0.15f);

        private static Sprite pixel;

        private Damageable target;
        private SpriteRenderer background;
        private SpriteRenderer fill;

        private void Awake()
        {
            target = GetComponentInParent<Damageable>();
            background = CreateBar("Background", new Color(0f, 0f, 0f, 0.6f), SortingOrder);
            fill = CreateBar("Fill", fullColor, SortingOrder + 1);
        }

        private void LateUpdate()
        {
            var visible = target != null && !target.IsDead && target.HealthFraction < 1f;
            background.enabled = visible;
            fill.enabled = visible;
            if (!visible)
                return;

            transform.SetPositionAndRotation((Vector2)target.transform.position + offset, Quaternion.identity);
            var fraction = target.HealthFraction;
            background.transform.localScale = new Vector3(size.x, size.y, 1f);
            fill.transform.localScale = new Vector3(size.x * fraction, size.y * 0.7f, 1f);
            fill.transform.localPosition = new Vector3(-size.x * (1f - fraction) * 0.5f, 0f, 0f);
            fill.color = Color.Lerp(lowColor, fullColor, fraction);
        }

        private SpriteRenderer CreateBar(string barName, Color color, int order)
        {
            var go = new GameObject(barName);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Pixel;
            renderer.color = color;
            renderer.sortingOrder = order;
            renderer.enabled = false;
            return renderer;
        }

        private static Sprite Pixel
        {
            get
            {
                if (pixel == null)
                {
                    var texture = new Texture2D(1, 1) { name = "HealthBarPixel" };
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    pixel = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                }
                return pixel;
            }
        }
    }
}
