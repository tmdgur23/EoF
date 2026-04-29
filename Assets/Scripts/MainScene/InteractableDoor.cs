using UnityEngine;
using MainScene;

namespace MainScene
{
    public class InteractableDoor : MonoBehaviour
    {
        [SerializeField] private int roomIndex;
        public int RoomIndex => roomIndex;

        private float m_lastInteractTime = -10f;
        private string cachedHintMessage = "";

        public void SetRoomIndex(int index)
        {
            roomIndex = index;
        }

        public string GetHintMessage()
        {
            if (string.IsNullOrEmpty(cachedHintMessage))
            {
                GenerateRandomHint();
            }
            return cachedHintMessage;
        }

        public void ResetHint()
        {
            cachedHintMessage = "";
        }

        private void GenerateRandomHint()
        {
            if (RoomAttributeManager.Instance == null) return;
            RoomConceptType concept = RoomAttributeManager.Instance.GetRoomConcept(roomIndex);

            switch (concept)
            {
                case RoomConceptType.Strength_Base:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이 방에서는 강한 육체의 힘을 얻을 수 있을 것 같다." : "피 냄새가 진동한다. 이곳은 폭력의 흔적으로 가득하다.";
                    break;
                case RoomConceptType.Strength_Bleed:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "바닥에 말라붙은 피가 보인다… 이 방은 고통을 기억하고 있다." : "누군가 이곳에서 오래도록 고통받았던 것 같다.";
                    break;
                case RoomConceptType.Strength_Combo:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "빠른 움직임의 흔적이 곳곳에 남아 있다." : "이곳에서는 끊임없는 공격의 흐름을 배울 수 있을지도 모른다.";
                    break;
                case RoomConceptType.Intelligence_Analyze:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이 방에서는 무언가를 꿰뚫어볼 수 있을 것 같다." : "눈에 보이지 않는 진실이 이곳에 숨겨져 있다.";
                    break;
                case RoomConceptType.Intelligence_Amplify:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이 방은 힘을 증폭시키는 무언가를 품고 있다." : "잠재된 힘을 끌어올릴 수 있을 것 같은 기운이 느껴진다.";
                    break;
                case RoomConceptType.Intelligence_Debuff:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이곳에서는 적을 약화시킬 방법을 찾을 수 있을 것 같다." : "기묘한 기운이 상대를 무너뜨릴 것만 같다.";
                    break;
                case RoomConceptType.Willpower_Fate:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이 방에서는 운명을 뒤틀 수 있을 것 같다." : "이곳의 기운은 결과를 바꿀 수 있을지도 모른다.";
                    break;
                case RoomConceptType.Willpower_Madness:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이 방은 위험하다… 하지만 강한 힘을 줄 것 같다." : "정신이 흔들리는 느낌이다. 대신 강력한 힘이 느껴진다.";
                    break;
                case RoomConceptType.Willpower_Convert:
                    cachedHintMessage = UnityEngine.Random.value > 0.5f ? "이 방에서는 모든 가능성을 다른 형태로 바꿀 수 있을 것 같다." : "결과를 원하는 방향으로 바꿀 수 있을지도 모른다.";
                    break;
                default:
                    cachedHintMessage = "알 수 없는 기운이 느껴진다.";
                    break;
            }
        }

        public void Interact()
        {
            // Per-door cooldown of 5 seconds to prevent rapid reopening
            if (Time.unscaledTime - m_lastInteractTime < 5.0f) return;
            m_lastInteractTime = Time.unscaledTime;

            Debug.Log($"[InteractableDoor] Interact() called on Room {roomIndex}. Attempting to trigger reward...");
            
            if (RoomAttributeManager.Instance != null)
            {
                RoomAttributeManager.Instance.TriggerReward(roomIndex);
            }
            else
            {
                Debug.LogError("[InteractableDoor] RoomAttributeManager.Instance is NULL! Is it present in the scene?");
            }
        }
    }
}
