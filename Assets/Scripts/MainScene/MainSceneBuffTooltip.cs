using UnityEngine;
using UnityEngine.EventSystems;
using Misc.PopUp;
using Cards.General;

namespace MainScene
{
    public class MainSceneBuffTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string buffName;
        public string buffDesc;

        public void OnPointerEnter(PointerEventData eventData)
        {
            var rect = GetComponent<RectTransform>();
            PopUpHandler.Instance.OpenTextPopUp(buffName, buffDesc, rect, RectAnchor.Bottom);
            
            // Adjust popup position like MainSceneStatUI
            var popupRect = PopUpHandler.Instance.PopupRect;
            if (popupRect != null && rect != null)
            {
                Vector3 targetPos = popupRect.position;
                targetPos.x = rect.position.x;
                popupRect.position = targetPos;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PopUpHandler.Instance.CloseAll();
        }

        private void OnDisable()
        {
            if (PopUpHandler.Instance != null)
            {
                PopUpHandler.Instance.CloseAll();
            }
        }
    }
}
