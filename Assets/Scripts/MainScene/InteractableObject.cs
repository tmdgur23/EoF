using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MainScene
{
    public class InteractableObject : MonoBehaviour
    {
        [Header("Object Settings")]
        [SerializeField] private string interactHintText = "알 수 없는 기운이 느껴진다...";
        [SerializeField] private string statName = "MaxHP"; // MaxHP, BasicAttack, BasicSoul, DiceRoll
        [SerializeField] private int statValue = 5;
        
        private bool isInteracted = false;

        // PlayerInteraction에서 호출할 메서드
        public string GetInteractHint()
        {
            return interactHintText;
        }

        public void Interact()
        {
            if (isInteracted) return;

            if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomInteractions >= 5)
            {
                Debug.Log("[InteractableObject] 이미 이 방에서 5회 상호작용을 마쳤습니다.");
                return;
            }

            isInteracted = true;
            Debug.Log($"[InteractableObject] '{gameObject.name}' 상호작용! {statName} +{statValue}");

            // 시각적 피드백 (예: 색상 변경 또는 오브젝트 비활성화)
            Renderer r = GetComponent<Renderer>();
            if (r != null) r.material.color = Color.gray;

            if (RoomExplorationManager.Instance != null)
            {
                RoomExplorationManager.Instance.OnObjectInteracted(statName, statValue);
            }
        }
    }
}
