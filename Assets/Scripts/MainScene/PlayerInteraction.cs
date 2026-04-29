using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Battle.General;
using OptionMenu;

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
        private float lastUIActiveTime = 0f; // UI가 닫힌 후 프레임 겹침 방지용 쿨타임
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

        private InteractableDoor currentDoor;

        private GameObject enterBtnGo, cancelBtnGo;
        private GameObject cardRewardBtnGo, cardRemoveBtnGo, soulBtnGo;
        
        private HashSet<int> interactedBookIDs = new HashSet<int>();
        private int currentBookID = -1;
        private int lastTrackedRoomIndex = -1;

        private void CreateHintUI()
        {
            GameObject canvasGo = new GameObject("HintCanvas");
            hintCanvas = canvasGo.AddComponent<Canvas>();
            hintCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // 배경 패널 추가
            GameObject panelGo = new GameObject("HintPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.7f);
            RectTransform panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(1000, 300); // 3버튼을 위해 높이를 키움
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0, 100);

            // Text 생성
            GameObject textGo = new GameObject("HintText");
            textGo.transform.SetParent(panelGo.transform, false);
            hintText = textGo.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintText.fontSize = 35;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.color = Color.white;

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(1000, 100);
            textRect.anchoredPosition = new Vector2(0, 60);

            // ================== 문 상호작용 버튼 ==================
            enterBtnGo = CreateButton(panelGo.transform, "EnterButton", "들어간다", new Vector2(-150, -40), new Color(0.2f, 0.6f, 0.2f, 1f));
            enterBtnGo.GetComponent<Button>().onClick.AddListener(OnEnterClicked);

            cancelBtnGo = CreateButton(panelGo.transform, "CancelButton", "돌아간다", new Vector2(150, -40), new Color(0.6f, 0.2f, 0.2f, 1f));
            cancelBtnGo.GetComponent<Button>().onClick.AddListener(OnCancelClicked);

            // ================== 책 상호작용 3선택지 버튼 ==================
            cardRewardBtnGo = CreateButton(panelGo.transform, "CardRewardBtn", "카드 보상", new Vector2(-300, -40), new Color(0.2f, 0.4f, 0.8f, 1f));
            cardRemoveBtnGo = CreateButton(panelGo.transform, "CardRemoveBtn", "카드 제거", new Vector2(0, -40), new Color(0.8f, 0.3f, 0.3f, 1f));
            soulBtnGo = CreateButton(panelGo.transform, "SoulBtn", "Soul +2", new Vector2(300, -40), new Color(0.6f, 0.2f, 0.8f, 1f));
            
            cardRewardBtnGo.GetComponent<Button>().onClick.AddListener(() => OnBookChoiceClicked(0));
            cardRemoveBtnGo.GetComponent<Button>().onClick.AddListener(() => OnBookChoiceClicked(1));
            soulBtnGo.GetComponent<Button>().onClick.AddListener(() => OnBookChoiceClicked(2));

            hintCanvas.gameObject.SetActive(false);
        }

        private GameObject CreateButton(Transform parent, string name, string textStr, Vector2 pos, Color color)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            Button btn = btnGo.AddComponent<Button>();
            Image img = btnGo.AddComponent<Image>();
            img.color = color;
            RectTransform rect = btnGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 60);
            rect.anchoredPosition = pos;

            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            Text txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = textStr;
            txt.fontSize = 28;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txtGo.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 60);

            return btnGo;
        }

        private void Update()
        {
            EnsureCamera();
            if (mainCamera == null) return;

            // 방 밖으로 나갔거나 다른 방으로 이동했다면 읽은 책 기록 초기화
            if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomIndex != lastTrackedRoomIndex)
            {
                lastTrackedRoomIndex = RoomExplorationManager.Instance.currentRoomIndex;
                interactedBookIDs.Clear();
            }

            // 이미 힌트 창이나 보상 창(카드 등)이 켜져있어서 마우스가 보이는 상태라면 상호작용 검사를 하지 않음
            if (Cursor.lockState == CursorLockMode.None || Cursor.visible)
            {
                lastUIActiveTime = Time.time;
                return;
            }

            if (hintCanvas != null && hintCanvas.gameObject.activeSelf)
            {
                lastUIActiveTime = Time.time;
                return;
            }

            // UI가 닫힌 직후 0.5초 동안은 동일한 마우스 클릭이 배경 오브젝트(책 등)에 적용되지 않도록 무시합니다.
            if (Time.time - lastUIActiveTime < 0.5f)
                return;

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
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
                InteractableObject obj = hit.collider.GetComponentInParent<InteractableObject>();
                
                crosshair.color = (door != null || obj != null) ? highlightColor : normalColor;
            }
            else
            {
                crosshair.color = normalColor;
            }
        }

        private void HandleInteraction()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
                if (door != null)
                {
                    // 방 안에 있을 때는 문을 클릭해도 입장 UI가 뜨지 않도록 무시
                    if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomIndex >= 0)
                    {
                        return;
                    }
                    ShowHint(door);
                    return;
                }

                // 태그가 "Book"인 오브젝트를 클릭했을 때 (부모 오브젝트에 태그가 있어도 인식하도록)
                Transform currentTransform = hit.collider.transform;
                bool isBook = false;
                while (currentTransform != null)
                {
                    if (currentTransform.CompareTag("Book"))
                    {
                        isBook = true;
                        break;
                    }
                    currentTransform = currentTransform.parent;
                }

                if (isBook)
                {
                    int bookID = currentTransform.gameObject.GetInstanceID();
                    if (interactedBookIDs.Contains(bookID))
                    {
                        Debug.Log("[PlayerInteraction] 이 책은 이미 읽어서 보상을 획득했습니다.");
                        return; // 이미 읽은 책 무시
                    }

                    currentBookID = bookID;
                    ShowBookRewardChoice();
                }
            }
        }

        private List<RewardOption> currentChoices;

        private void ShowBookRewardChoice()
        {
            if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomInteractions >= 5)
            {
                Debug.Log("[PlayerInteraction] 이미 이 방에서 5번의 보상을 획득했습니다.");
                return;
            }

            if (hintText != null)
            {
                hintText.text = "책을 탐색했습니다. 보상을 선택하세요.";
                hintCanvas.gameObject.SetActive(true);
                
                // 문 버튼 숨기기
                if (enterBtnGo != null) enterBtnGo.SetActive(false);
                if (cancelBtnGo != null) cancelBtnGo.SetActive(false);
                
                // 라운드에 맞는 랜덤 보상 3개 뽑기
                int loopCount = RoomExplorationManager.Instance != null ? RoomExplorationManager.Instance.currentLoopCount : 0;
                
                if (RewardPoolManager.Instance != null)
                {
                    currentChoices = RewardPoolManager.Instance.GetRandomRewards(loopCount, 3);

                    if (cardRewardBtnGo != null && currentChoices.Count > 0)
                    {
                        cardRewardBtnGo.SetActive(true);
                        cardRewardBtnGo.GetComponentInChildren<Text>().text = currentChoices[0].RewardName;
                    }
                    if (cardRemoveBtnGo != null && currentChoices.Count > 1)
                    {
                        cardRemoveBtnGo.SetActive(true);
                        cardRemoveBtnGo.GetComponentInChildren<Text>().text = currentChoices[1].RewardName;
                    }
                    if (soulBtnGo != null && currentChoices.Count > 2)
                    {
                        soulBtnGo.SetActive(true);
                        soulBtnGo.GetComponentInChildren<Text>().text = currentChoices[2].RewardName;
                    }
                }
                else
                {
                    Debug.LogError("[PlayerInteraction] RewardPoolManager.Instance가 없습니다! 씬에 추가해주세요.");
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnBookChoiceClicked(int index)
        {
            CloseHintUI();

            if (currentChoices == null || index < 0 || index >= currentChoices.Count) return;
            
            RewardOption selectedReward = currentChoices[index];
            string choiceId = selectedReward.RewardId;

            // 보상 획득 시 이 책의 ID를 저장하여 중복 획득 방지
            if (currentBookID != -1)
            {
                interactedBookIDs.Add(currentBookID);
                currentBookID = -1;
            }

            if (RoomExplorationManager.Instance != null)
            {
                RoomExplorationManager.Instance.currentRoomInteractions++;
                Debug.Log($"[PlayerInteraction] 선택한 보상: {selectedReward.RewardName}. (현재 보상 획득 횟수: {RoomExplorationManager.Instance.currentRoomInteractions}/5)");
            }

            bool uiOpened = false;
            string[] parts = choiceId.Split('_');

            // 1. 상태 변화 (체력, 소울 등) 모두 일괄 적용
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "HP" && i + 1 < parts.Length)
                {
                    var config = OptionMenu.Options.LoadConfigData();
                    if (config.Health == null || config.Health.Max == 0) config.Health = new Utilities.Range(100, 100);

                    if (parts[i+1] == "HALF")
                    {
                        config.Health.Min /= 2;
                    }
                    else if (int.TryParse(parts[i+1], out int amount))
                    {
                        config.Health.Min += amount;
                    }

                    if (config.Health.Min > config.Health.Max) config.Health.Min = config.Health.Max;
                    if (config.Health.Min < 1) config.Health.Min = 1; // 메인씬에서 즉사 방지
                    
                    OptionMenu.Options.SaveConfigData(config);
                    Debug.Log($"[PlayerInteraction] 체력 보상 적용 완료. 현재 체력: {config.Health.Min}");
                    if (MainSceneHUD.Instance != null) MainSceneHUD.Instance.UpdateUI();
                }
                else if (parts[i] == "SOUL" && i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i+1], out int amount))
                    {
                        var config = OptionMenu.Options.LoadConfigData();
                        config.Soul += amount;
                        if (config.Soul < 0) config.Soul = 0;
                        OptionMenu.Options.SaveConfigData(config);
                        Debug.Log($"[PlayerInteraction] 소울 보상 적용 완료. 현재 소울: {config.Soul}");
                        if (MainSceneHUD.Instance != null) MainSceneHUD.Instance.UpdateUI();
                    }
                }
                else if (parts[i] == "MAXHP" && i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i+1], out int amount))
                    {
                        var config = OptionMenu.Options.LoadConfigData();
                        if (config.Health == null || config.Health.Max == 0) config.Health = new Utilities.Range(100, 100);
                        
                        config.Health.Max += amount;
                        config.Health.Min += amount; // 최대 체력이 늘어나면 현재 체력도 함께 회복시켜주는 것이 일반적입니다.
                        
                        OptionMenu.Options.SaveConfigData(config);
                        Debug.Log($"[PlayerInteraction] 최대 체력 증가 완료. 현재 최대 체력: {config.Health.Max}");
                        if (MainSceneHUD.Instance != null) MainSceneHUD.Instance.UpdateUI();
                    }
                }
            }

            // 2. UI 호출 (동시에 두 개의 UI가 뜨는 것을 방지하기 위해 우선순위 처리)
            if (choiceId.Contains("REMOVE"))
            {
                if (MainSceneDeckViewer.Instance != null)
                {
                    int count = choiceId.Contains("REMOVE_2") ? 2 : 1;
                    MainSceneDeckViewer.Instance.OpenForRemoval(count);
                    uiOpened = true;
                }
            }
            else if (choiceId.Contains("UPGRADE"))
            {
                if (MainSceneDeckViewer.Instance != null)
                {
                    int count = choiceId.Contains("UPGRADE_2") ? 2 : 1;
                    MainSceneDeckViewer.Instance.OpenForUpgrade(count);
                    uiOpened = true;
                }
            }
            else if (choiceId.Contains("CARD")) // CARD_1, CARD_2, CARD_CHOOSE_2 등 모든 카드 보상 포괄
            {
                if (RoomAttributeManager.Instance != null && RoomExplorationManager.Instance != null)
                {
                    int count = choiceId.Contains("CARD_2") ? 2 : 1;
                    RoomAttributeManager.Instance.TriggerReward(RoomExplorationManager.Instance.currentRoomIndex, count);
                    uiOpened = true;
                }
            }

            // UI가 뜨지 않는 단순 스탯 보상이거나 꽝(NOTHING)일 경우 즉시 상호작용 체크
            if (!uiOpened)
            {
                CheckInteractionLimit();
            }
        }

        private void CheckInteractionLimit()
        {
            // 카드 보상을 고르지 않고 다른 보상을 고른 경우에도 5회가 넘었는지 체크하여 루프를 진행합니다.
            if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomInteractions >= 5)
            {
                Debug.Log("[PlayerInteraction] 방 안에서 보상 5회 획득 달성. 방 탐색을 종료합니다.");
                RoomExplorationManager.Instance.ExitRoomOrBattle();
            }
        }

        private void ShowHint(InteractableDoor door)
        {
            if (hintText != null && door != null)
            {
                currentDoor = door;
                hintText.text = door.GetHintMessage();
                hintCanvas.gameObject.SetActive(true);
                
                if (enterBtnGo != null) enterBtnGo.SetActive(true);
                if (cancelBtnGo != null) cancelBtnGo.SetActive(true);
                
                if (cardRewardBtnGo != null) cardRewardBtnGo.SetActive(false);
                if (cardRemoveBtnGo != null) cardRemoveBtnGo.SetActive(false);
                if (soulBtnGo != null) soulBtnGo.SetActive(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnEnterClicked()
        {
            if (currentDoor != null)
            {
                // 방 안으로 이동 (RoomExplorationManager 호출)
                if (RoomExplorationManager.Instance != null)
                {
                    RoomExplorationManager.Instance.EnterRoom(currentDoor.RoomIndex);
                }
                else
                {
                    Debug.LogError("[PlayerInteraction] RoomExplorationManager.Instance is null!");
                }
            }

            CloseHintUI();
        }

        private void OnCancelClicked()
        {
            CloseHintUI();
        }

        private void CloseHintUI()
        {
            if (hintCanvas != null) hintCanvas.gameObject.SetActive(false);
            currentDoor = null;

            // 다시 카메라 회전 등을 위해 커서를 잠급니다.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
