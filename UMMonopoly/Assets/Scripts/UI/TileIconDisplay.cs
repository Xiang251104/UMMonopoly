using UnityEngine;
using UMMonopoly.Data;

namespace UMMonopoly.UI
{
    public class TileIconDisplay : MonoBehaviour
    {
        [SerializeField] private TileDataSO tileData;
        [SerializeField] private SpriteRenderer iconRenderer;

        private void Start()
        {
            if (tileData != null && iconRenderer != null && tileData.icon != null)
                iconRenderer.sprite = tileData.icon;
        }
    }
}
