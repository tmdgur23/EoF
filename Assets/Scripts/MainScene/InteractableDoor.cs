using UnityEngine;
using MainScene;

namespace MainScene
{
    public class InteractableDoor : MonoBehaviour
    {
        [SerializeField] private int roomIndex;
        private float m_lastInteractTime = -10f;

        public void SetRoomIndex(int index)
        {
            roomIndex = index;
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
