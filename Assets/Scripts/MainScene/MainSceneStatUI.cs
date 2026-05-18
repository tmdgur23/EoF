using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Misc.PopUp;
using Cards.General;

namespace MainScene
{
    public class MainSceneStatUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum StatType { Strength, Intelligence, Mental }
        public StatType type;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (PopUpHandler.Instance == null) return;

            string header = "";
            string key = "";
            string desc = "";
            switch(type)
            {
                case StatType.Strength: 
                    header = "근력"; 
                    key = "Bonus_STR"; 
                    desc = "공격 시 물리 공격의 최종 피해량이 근력 수치만큼 추가로 증가합니다."; 
                    break;
                case StatType.Intelligence: 
                    header = "지식"; 
                    key = "Bonus_INT"; 
                    desc = "전투 시작 시 주사위 리롤 기회가 지식 수치만큼 추가로 증가합니다."; 
                    break;
                case StatType.Mental: 
                    header = "정신력"; 
                    key = "Bonus_MEN"; 
                    desc = "영혼 상호작용 시 수치가 정신력 수치만큼 증가합니다."; 
                    break;
            }
            int val = PlayerPrefs.GetInt(key, 0);
            
            // 팝업 표시
            var myRect = GetComponent<RectTransform>();
            PopUpHandler.Instance.OpenTextPopUp($"{header} {val}", desc, myRect, RectAnchor.Bottom);

            // 팝업 X 위치를 아이콘 중심으로 고정 (올바른 m_rect 사용)
            var popupRect = PopUpHandler.Instance.PopupRect;
            if (popupRect != null)
            {
                var corners = new Vector3[4];
                myRect.GetWorldCorners(corners);
                float iconCenterX = (corners[0].x + corners[3].x) * 0.5f;
                var pos = popupRect.position;
                popupRect.position = new Vector3(iconCenterX, pos.y, pos.z);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (PopUpHandler.Instance != null)
            {
                PopUpHandler.Instance.CloseAll();
            }
        }
    }
}
