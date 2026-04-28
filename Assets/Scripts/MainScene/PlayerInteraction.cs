using UnityEngine;
using UnityEngine.UI;

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

        // 힌트 UI 관련 변수
        private Canvas hintCanvas;
        private Text hintText;
        private float hideTime = 0f;
        private enum CardType 
        { 
            Strength_Base, Strength_Bleed, Strength_Combo,
            Knowledge_Analyze, Knowledge_Amplify, Knowledge_Debuff,
            Mind_Fate, Mind_Madness, Mind_Convert
        }

        private void Start()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null) mainCamera = Camera.main;
            
            CreateCrosshair();
            CreateHintUI();
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
            
            crosshair.rectTransform.sizeDelta = new Vector2(8, 8);
            crosshair.rectTransform.anchoredPosition = Vector2.zero;
            crosshair.color = normalColor;
        }

        private void CreateHintUI()
        {
            GameObject canvasGo = new GameObject("HintCanvas");
            hintCanvas = canvasGo.AddComponent<Canvas>();
            hintCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // 배경 패널 추가 (더 잘 보이게)
            GameObject panelGo = new GameObject("HintPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.7f);
            RectTransform panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(1000, 120);
            panelRect.anchoredPosition = new Vector2(0, -300); // 화면 하단

            // Text 생성
            GameObject textGo = new GameObject("HintText");
            textGo.transform.SetParent(panelGo.transform, false);
            hintText = textGo.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintText.fontSize = 35;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.color = Color.white;

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(1000, 120);
            textRect.anchoredPosition = Vector2.zero;

            hintCanvas.gameObject.SetActive(false);
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

            if (Time.time > hideTime && hintCanvas != null && hintCanvas.gameObject.activeSelf)
            {
                hintCanvas.gameObject.SetActive(false);
            }
        }

        private void UpdateCrosshairFeedback()
        {
            if (crosshair == null) return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
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
            if (MainSceneRewardUI.Instance != null && MainSceneRewardUI.Instance.gameObject.activeInHierarchy) return;
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                Debug.Log($"[PlayerInteraction] Raycast hit: {hit.collider.gameObject.name}");
                
                InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
                if (door != null)
                {
                    // 기존 보상창 로직(door.Interact()) 비활성화
                    // door.Interact(); 

                    // 신규 힌트 로직 실행
                    ShowHint();
                }
            }
        }

        private void ShowHint()
        {
            CardType randomType = (CardType)UnityEngine.Random.Range(0, 9);
            string message = "";

            switch (randomType)
            {
                // --- 근력 ---
                case CardType.Strength_Base:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이 방에서는 강한 육체의 힘을 얻을 수 있을 것 같다." 
                        : "피 냄새가 진동한다. 이곳은 폭력의 흔적으로 가득하다.";
                    break;
                case CardType.Strength_Bleed:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "바닥에 말라붙은 피가 보인다… 이 방은 고통을 기억하고 있다." 
                        : "누군가 이곳에서 오래도록 고통받았던 것 같다.";
                    break;
                case CardType.Strength_Combo:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "빠른 움직임의 흔적이 곳곳에 남아 있다." 
                        : "이곳에서는 끊임없는 공격의 흐름을 배울 수 있을지도 모른다.";
                    break;

                // --- 지식 ---
                case CardType.Knowledge_Analyze:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이 방에서는 무언가를 꿰뚫어볼 수 있을 것 같다." 
                        : "눈에 보이지 않는 진실이 이곳에 숨겨져 있다.";
                    break;
                case CardType.Knowledge_Amplify:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이 방은 힘을 증폭시키는 무언가를 품고 있다." 
                        : "잠재된 힘을 끌어올릴 수 있을 것 같은 기운이 느껴진다.";
                    break;
                case CardType.Knowledge_Debuff:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이곳에서는 적을 약화시킬 방법을 찾을 수 있을 것 같다." 
                        : "기묘한 기운이 상대를 무너뜨릴 것만 같다.";
                    break;

                // --- 정신력 ---
                case CardType.Mind_Fate:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이 방에서는 운명을 뒤틀 수 있을 것 같다." 
                        : "이곳의 기운은 결과를 바꿀 수 있을지도 모른다.";
                    break;
                case CardType.Mind_Madness:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이 방은 위험하다… 하지만 강한 힘을 줄 것 같다." 
                        : "정신이 흔들리는 느낌이다. 대신 강력한 힘이 느껴진다.";
                    break;
                case CardType.Mind_Convert:
                    message = UnityEngine.Random.value > 0.5f 
                        ? "이 방에서는 모든 가능성을 다른 형태로 바꿀 수 있을 것 같다." 
                        : "결과를 원하는 방향으로 바꿀 수 있을지도 모른다.";
                    break;
            }

            if (hintText != null)
            {
                hintText.text = message;
                hintCanvas.gameObject.SetActive(true);
                hideTime = Time.time + 4f; // 4초 동안 표시
            }
        }
    }
}
