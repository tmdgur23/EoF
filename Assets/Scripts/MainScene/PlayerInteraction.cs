using UnityEngine;

namespace MainScene
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactDistance = 100f;
        [SerializeField] private LayerMask interactLayer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        private Camera mainCamera;
        private UnityEngine.UI.Image crosshair;

        private void Start()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null) mainCamera = Camera.main;
            
            CreateCrosshair();
        }

        private void EnsureCamera()
        {
            if (mainCamera == null || mainCamera == (UnityEngine.Object)null)
            {
                mainCamera = Camera.main;
            }
        }

        private void CreateCrosshair()
        {
            GameObject canvasObj = new GameObject("CrosshairCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            
            GameObject imgObj = new GameObject("Crosshair");
            imgObj.transform.SetParent(canvasObj.transform);
            crosshair = imgObj.AddComponent<UnityEngine.UI.Image>();
            
            // Simple dot or small square
            crosshair.rectTransform.sizeDelta = new Vector2(8, 8);
            crosshair.rectTransform.anchoredPosition = Vector2.zero;
            crosshair.color = normalColor;
            
            // Use default sprite to avoid "Failed to find UI/Skin/..." errors
            // If we don't assign a sprite, it defaults to a white square which works as a dot.
        }

        private void Update()
        {
            EnsureCamera();
            if (mainCamera == null) return;

            UpdateCrosshairFeedback();

            if (Input.GetMouseButtonDown(0)) // Left Click
            {
                HandleInteraction();
            }
        }

        private void UpdateCrosshairFeedback()
        {
            if (crosshair == null) return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
            
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
                crosshair.color = (door != null) ? highlightColor : normalColor;
            }
            else
            {
                crosshair.color = normalColor;
            }
        }

        private void HandleInteraction()
        {
            // 1. Singleton을 통한 정확한 UI 활성화 체크
            if (MainSceneRewardUI.Instance != null && MainSceneRewardUI.Instance.gameObject.activeInHierarchy) return;
            
            // 2. 마우스가 UI(카드, 리셋 버튼 등) 위에 있는지 체크
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of screen
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                Debug.Log($"[PlayerInteraction] Raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                
                InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
                if (door != null)
                {
                    door.Interact();
                }
                else
                {
                    Debug.Log("Hit object is not a door or has no InteractableDoor component.");
                }
            }
            else
            {
                Debug.Log("Raycast didn't hit anything.");
            }
        }

        private bool rewardUIActive()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            return false;
        }
    }
}
