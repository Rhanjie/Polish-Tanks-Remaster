using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Pure top-down view of a building: only the roof, drawn exactly on the footprint.
    /// Height is shown by the shadow alone.
    /// </summary>
    public class FlatBuildingView : MonoBehaviour
    {
        [SerializeField] private Building building;
        [SerializeField] private SpriteRenderer roof;

        private void Start()
        {
            if (building == null)
                building = GetComponentInParent<Building>();
            if (roof == null || building == null)
                return;
            roof.transform.position = building.transform.position;
            roof.size = building.Size;
        }
    }
}
