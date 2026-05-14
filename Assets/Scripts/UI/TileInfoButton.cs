using UMMonopoly.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UMMonopoly.UI
{
    public class TileInfoButton : MonoBehaviour, IPointerClickHandler
    {
        public int tilePosition;
        public PropertyPopup popup;

        public void OnPointerClick(PointerEventData eventData)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Board == null || popup == null) return;
            if (tilePosition < 0 || tilePosition >= gm.Board.Tiles.Count) return;

            popup.Show(gm.Board.GetTile(tilePosition));
        }
    }
}
