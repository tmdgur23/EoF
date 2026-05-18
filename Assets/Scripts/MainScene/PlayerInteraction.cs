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
        
        // ================== 보상 큐 시스템 ==================
        private struct RewardAction
        {
            public string type; // "CARD", "REMOVE", "UPGRADE"
            public int count;
        }
        private Queue<RewardAction> rewardQueue = new Queue<RewardAction>();
        // ====================================================

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
            // Prevent duplicates
            var oldCanvas = GameObject.Find("CrosshairCanvas");
            if (oldCanvas != null) Destroy(oldCanvas);

            GameObject canvasObj = new GameObject("CrosshairCanvas");
            canvasObj.name = "CrosshairCanvas";
            
            // BULLETPROOF: Ensure the crosshair stays in the same scene as the player, 
            // even if the active scene hasn't switched yet during loading.
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasObj, gameObject.scene);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // BULLETPROOF: Ensure crosshair is always on top
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            
            GameObject imgObj = new GameObject("Crosshair");
            imgObj.transform.SetParent(canvasObj.transform);
            crosshair = imgObj.AddComponent<UnityEngine.UI.Image>();
            crosshair.raycastTarget = false; // Prevent crosshair from blocking clicks
            
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
            // Prevent duplicates
            var oldHint = GameObject.Find("HintCanvas");
            if (oldHint != null) Destroy(oldHint);

            GameObject canvasGo = new GameObject("HintCanvas");
            canvasGo.name = "HintCanvas";
            
            // BULLETPROOF: Ensure the hint UI stays in the same scene as the player
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGo, gameObject.scene);

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
            hintText.font = Resources.Load<Font>("BlackAndWhitePicture-Regular");
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
            txt.font = Resources.Load<Font>("BlackAndWhitePicture-Regular");
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

            // BULLETPROOF: Keep crosshair visible in Main Scene
            if (crosshair != null && crosshair.canvas != null)
            {
                if (!crosshair.canvas.gameObject.activeSelf)
                    crosshair.canvas.gameObject.SetActive(true);
            }

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
                var config = OptionMenu.Options.LoadConfigData();
                int battleCount = config != null ? config.BattleCount : 0;
                int rewardRound = 0;
                if (battleCount == 1) rewardRound = 3;
                else if (battleCount >= 2) rewardRound = 6;
                
                if (RewardPoolManager.Instance != null)
                {
                    // 이번 방에서 이미 스탯 보상을 받았으면 스탯 선택지를 제외하고 뽑음
                    bool excludeStats = RoomExplorationManager.Instance != null &&
                                       RoomExplorationManager.Instance.statRewardGivenThisRoom;
                    currentChoices = RewardPoolManager.Instance.GetRandomRewards(rewardRound, 3, excludeStats);

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
            if (currentChoices == null || index < 0 || index >= currentChoices.Count) return;

            // 보상 선택 시 즉시 힌트창 닫기 (커서는 보상창이 관리하게 둠)
            if (hintCanvas != null) hintCanvas.gameObject.SetActive(false);
            
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
                
                // 스탯 보상 수령 여부 플래그 - STR/INT/MEN/ALL_STAT 포함이면 표시
                string choiceIdForStatCheck = selectedReward.RewardId;
                if (choiceIdForStatCheck.StartsWith("STR_") || choiceIdForStatCheck.StartsWith("INT_") ||
                    choiceIdForStatCheck.StartsWith("MEN_") || choiceIdForStatCheck.StartsWith("ALL_STAT"))
                {
                    RoomExplorationManager.Instance.statRewardGivenThisRoom = true;
                }
                
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
                        
                        // 배틀씬의 StatsFormula.CalculateSoul()과 동일한 정신력 보너스 적용
                        int bonusMental = PlayerPrefs.GetInt("Bonus_MEN", 0);
                        int actualAmount = amount;
                        if (amount != 0)
                        {
                            actualAmount = amount + bonusMental;
                            // 잃을 때(amount < 0) 이득이 생겨서 양수로 바뀌는 것은 방지 (0으로 고정)
                            if (amount < 0) actualAmount = Mathf.Min(0, actualAmount);
                        }
                        
                        config.Soul += actualAmount;
                        Debug.Log($"[PlayerInteraction] 소울 변동: 원래={amount}, 정신력보너스={bonusMental}, 실제적용={actualAmount}");
                        OptionMenu.Options.SaveConfigData(config);
                        Debug.Log($"[PlayerInteraction] 소울 보상 적용 완료. 현재 소울: {config.Soul}");
                        
                        // 게임 오버 체크 로직
                        if (config.Soul <= -50)
                        {
                            TriggerGameOver();
                        }

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
                        config.Health.Min += amount; 
                        
                        OptionMenu.Options.SaveConfigData(config);
                        Debug.Log($"[PlayerInteraction] 최대 체력 증가 완료. 현재 최대 체력: {config.Health.Max}");
                        if (MainSceneHUD.Instance != null) MainSceneHUD.Instance.UpdateUI();
                    }
                }
                else if ((parts[i] == "STR" || parts[i] == "INT" || parts[i] == "MEN") && i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i+1], out int amount))
                    {
                        int currentStat = PlayerPrefs.GetInt("Bonus_" + parts[i], 0);
                        PlayerPrefs.SetInt("Bonus_" + parts[i], currentStat + amount);
                        Debug.Log($"[PlayerInteraction] 영구 스탯 보너스 적용: {parts[i]} +{amount}");
                    }
                }
                else if (parts[i] == "ALL" && i + 2 < parts.Length && parts[i+1] == "STAT")
                {
                    if (int.TryParse(parts[i+2], out int amount))
                    {
                        string[] statNames = { "STR", "INT", "MEN" };
                        foreach (var stat in statNames)
                        {
                            int currentStat = PlayerPrefs.GetInt("Bonus_" + stat, 0);
                            PlayerPrefs.SetInt("Bonus_" + stat, currentStat + amount);
                            Debug.Log($"[PlayerInteraction] 영구 스탯 보너스 적용: {stat} +{amount}");
                        }
                    }
                }
                else if (parts[i] == "NEXT" && i + 2 < parts.Length)
                {
                    string buffType = parts[i+1];
                    if (int.TryParse(parts[i+2], out int amount))
                    {
                        int currentBuff = PlayerPrefs.GetInt("Next_" + buffType, 0);
                        PlayerPrefs.SetInt("Next_" + buffType, currentBuff + amount);
                        Debug.Log($"[PlayerInteraction] 다음 전투 예약 버프: {buffType} +{amount}");
                    }
                }
            }

            // 보상 적용 직후 메인씬 HUD 즉시 갱신 (버프 상태이콘 즉시 업데이트 등)
            if (MainSceneHUD.Instance != null) MainSceneHUD.Instance.UpdateUI();

            // 2. UI 보상을 큐에 삽입
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "REMOVE")
                {
                    int count = 1;
                    if (i + 1 < parts.Length && int.TryParse(parts[i+1], out int rCount))
                    {
                        count = rCount;
                    }
                    rewardQueue.Enqueue(new RewardAction { type = "REMOVE", count = count });
                }
                else if (parts[i] == "UPGRADE")
                {
                    int count = 1;
                    if (i + 1 < parts.Length && int.TryParse(parts[i+1], out int uCount))
                    {
                        count = uCount;
                    }
                    rewardQueue.Enqueue(new RewardAction { type = "UPGRADE", count = count });
                }
                else if (parts[i] == "CARD")
                {
                    // CARD_REMOVE, CARD_REMOVE_2, CARD_UPGRADE, CARD_UPGRADE_2 등에서 CARD는 접두사일 뿐 카드 보상이 아닙니다.
                    // 따라서 다음 부분이 REMOVE나 UPGRADE인 경우는 카드 보상으로 처리하지 않습니다.
                    if (i + 1 < parts.Length && (parts[i+1] == "REMOVE" || parts[i+1] == "UPGRADE"))
                    {
                        continue;
                    }

                    int count = 1;
                    if (i + 1 < parts.Length && int.TryParse(parts[i+1], out int cCount))
                    {
                        count = cCount;
                    }
                    rewardQueue.Enqueue(new RewardAction { type = "CARD", count = count });
                }
            }

            // 첫 번째 보상 프로세스 시작
            ProcessNextReward();
        }

        private void ProcessNextReward()
        {
            if (rewardQueue.Count == 0)
            {
                CheckInteractionLimit();
                return;
            }

            var next = rewardQueue.Dequeue();
            Debug.Log($"[PlayerInteraction] 다음 보상 처리 시작: {next.type} (개수: {next.count})");

            switch (next.type)
            {
                case "CARD":
                    if (RoomAttributeManager.Instance != null && RoomExplorationManager.Instance != null)
                    {
                        var rewardUI = Object.FindObjectOfType<MainSceneRewardUI>(true);
                        if (rewardUI != null) rewardUI.OnCloseCallback = ProcessNextReward;
                        RoomAttributeManager.Instance.TriggerReward(RoomExplorationManager.Instance.currentRoomIndex, next.count);
                    }
                    else ProcessNextReward();
                    break;

                case "REMOVE":
                    var removeViewer = Object.FindObjectOfType<MainSceneDeckViewer>(true);
                    if (removeViewer != null)
                    {
                        removeViewer.OnCloseCallback = ProcessNextReward;
                        removeViewer.OpenForRemoval(next.count);
                    }
                    else ProcessNextReward();
                    break;

                case "UPGRADE":
                    var upgradeViewer = Object.FindObjectOfType<MainSceneDeckViewer>(true);
                    if (upgradeViewer != null)
                    {
                        upgradeViewer.OnCloseCallback = ProcessNextReward;
                        upgradeViewer.OpenForUpgrade(next.count);
                    }
                    else ProcessNextReward();
                    break;

                default:
                    ProcessNextReward();
                    break;
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

            // 보상 큐가 비어있을 때만 커서를 다시 잠급니다.
            if (rewardQueue.Count == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        private void TriggerGameOver()
        {
            Debug.Log("[PlayerInteraction] 소울이 -50 이하가 되어 게임 오버됩니다.");

            // Resources 폴더의 LooseScreen 프리팹 소환
            GameObject losePrefab = Resources.Load<GameObject>("LooseScreen");
            if (losePrefab != null)
            {
                Instantiate(losePrefab);
                
                // 게임 오버 시 마우스 커서 해제
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // 타겟점 숨기기
                if (crosshair != null && crosshair.canvas != null)
                {
                    crosshair.canvas.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("[PlayerInteraction] LooseScreen 프리팹을 찾을 수 없습니다.");
            }
        }
    }
}
