using System.Collections;
using System.Collections.Generic;
using Cards.General;
using Units.Player.General;
using UnityEngine;
using UnityEngine.UI;
using Deck;
using Utilities;

namespace MainScene
{
    public class MainSceneRewardUI : MonoBehaviour
    {
        public static MainSceneRewardUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject m_cardPrefab;
        [SerializeField] private Transform m_contentTransform;
        [SerializeField] private GameObject m_background;

        private Player m_player;
        private List<GameObject> m_spawnedCards = new List<GameObject>();
        private float m_openTime;
        private bool m_isProcessing = false; // To prevent multiple selections

        private void Awake()
        {
            Instance = this; // Always overwrite with the latest instance
            m_player = FindObjectOfType<Units.Player.General.Player>();
            AutoLinkReferences();
            
            if (m_background != null) m_background.SetActive(false);
        }

        public void OpenReward(IEnumerable<CardInstance> cards)
        {
            Debug.Log("[MainSceneRewardUI] OpenReward() Call Started!");
            
            // 필수 레퍼런스 체크
            if (m_contentTransform == null) AutoLinkReferences();
            if (cards == null) return;

            // EventSystem 부재 시 자동 생성
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Ensure this object is named "Reward" so PlayerInteraction can find it
            gameObject.name = "Reward";
            m_openTime = Time.unscaledTime;
            m_isProcessing = false; // Reset lock

            // 2. 기존 카드 제거 (레이아웃 그룹에서 즉시 분리하여 6개로 보이는 현상 방지)
            if (m_contentTransform != null)
            {
                List<Transform> children = new List<Transform>();
                foreach (Transform child in m_contentTransform) children.Add(child);
                foreach (Transform child in children)
                {
                    child.SetParent(null); // 레이아웃에서 즉시 제외
                    Destroy(child.gameObject);
                }
            }
            
            foreach (var cardGo in m_spawnedCards)
            {
                if (cardGo != null)
                {
                    cardGo.transform.SetParent(null);
                    Destroy(cardGo);
                }
            }
            m_spawnedCards.Clear();

            // Force sizing every time to prevent layout shrinking bugs
            if (m_contentTransform != null && m_contentTransform is RectTransform contentRect)
            {
                contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                contentRect.pivot = new Vector2(0.5f, 0.5f);
                contentRect.anchoredPosition = new Vector2(0f, -100f);
                contentRect.sizeDelta = new Vector2(1400f, 600f);
            }

            // Setup UI
            gameObject.SetActive(true);
            
            // BULLETPROOF CURSOR UNLOCK: 보상창이 열리는 즉시 커서를 해제합니다.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (m_background != null) m_background.SetActive(true);
            
            var cardReward = transform.Find("CardReward");
            if (cardReward != null)
            {
                cardReward.gameObject.SetActive(true);
                for (int i = 0; i < cardReward.childCount; i++)
                {
                    cardReward.GetChild(i).gameObject.SetActive(true);
                }
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (m_cardPrefab == null)
            {
                Debug.LogError("[MainSceneRewardUI] Card Prefab is NULL! Cannot spawn rewards.");
                return;
            }

            // Spawn cards
            int count = 0;
            foreach (var cardInstance in cards)
            {
                count++;
                GameObject cardGo = Instantiate(m_cardPrefab, m_contentTransform);
                m_spawnedCards.Add(cardGo);

                CardModel model = cardGo.GetComponent<CardModel>();
                if (model != null)
                {
                    model.Setup(cardInstance);
                }

                // 핵심 버그: HoverableCard가 남아있으면 마우스를 뗄 때 카드 크기를 (0,0,0)으로 줄여버려서 카드가 증발합니다! 여기서 파괴해야 합니다.
                var hoverable = cardGo.GetComponent<Cards.General.HoverableCard>();
                if (hoverable != null) Destroy(hoverable);

                // 클릭 이벤트가 버블링되지 않을 경우를 대비하여 이미지가 있는 자식(CardModel)에 직접 트리거를 추가합니다.
                var clickTarget = cardGo.transform.Find("CardModel")?.gameObject ?? cardGo;
                
                UnityEngine.EventSystems.EventTrigger trigger = clickTarget.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null) trigger = clickTarget.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();

                var downEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                downEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                downEntry.callback.AddListener((data) => {
                    Debug.Log($"[MainSceneRewardUI] Card click detected on: {cardInstance.CardData.Name}");
                    OnCardSelected(cardInstance);
                });
                trigger.triggers.Add(downEntry);
            }
        Debug.Log($"[MainSceneRewardUI] Successfully spawned and setup {count} cards.");

        Canvas.ForceUpdateCanvases();
        Debug.Log("[MainSceneRewardUI] OpenReward() Call Finished. Waiting for user click...");
    }

        public void SetCardPrefab(GameObject prefab)
        {
            m_cardPrefab = prefab; // Provided dynamically from RoomAttributeManager
        }

        private void AutoLinkReferences()
        {
            if (m_background == null)
            {
                var bg = transform.Find("Background");
                if (bg != null) m_background = bg.gameObject;
            }

            if (m_contentTransform == null)
            {
                // Try to find the standard path for card selection
                var content = transform.Find("CardReward/CardSelection");
                if (content == null) content = transform.Find("CardSelection");
                if (content != null) m_contentTransform = content;
            }

            if (m_contentTransform != null)
            {
                // Reset layout just in case
                RectTransform contentRect = m_contentTransform.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                    contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                    contentRect.pivot = new Vector2(0.5f, 0.5f);
                    contentRect.anchoredPosition = new Vector2(0f, -100f); // Move down by 150
                    contentRect.sizeDelta = new Vector2(1400f, 600f);
                }

                HorizontalLayoutGroup layout = m_contentTransform.GetComponent<HorizontalLayoutGroup>();
                if (layout == null) layout = m_contentTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
                
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.spacing = 100f; // Gap between cards
            }
        }

        public void OnCardSelected(Cards.General.CardInstance card)
        {
            Debug.Log($"[MainSceneRewardUI] OnCardSelected called! card={card.CardData.Name}, isProcessing={m_isProcessing}, thisIsNull={(this == null)}");
            
            m_isProcessing = true; // LOCK!
            
            try
            {
                if (m_player == null) m_player = FindObjectOfType<Player>();
                
                // Physical Lock: Disable all card raycasts immediately
                if (m_spawnedCards != null)
                {
                    foreach (var spawned in m_spawnedCards)
                    {
                        if (spawned != null)
                        {
                            var cg = spawned.GetComponent<CanvasGroup>();
                            if (cg != null) cg.blocksRaycasts = false;
                            
                            var trigger = spawned.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                            if (trigger != null) trigger.enabled = false;
                        }
                    }
                }
                
                if (m_player != null)
                {
                    if (m_player.CardDeck != null)
                    {
                        m_player.CardDeck.Add(card);
                        Utilities.DeckUtility.SaveDeck(m_player.CardDeck);
                        Debug.Log($"[MainSceneRewardUI] 🎉 '{card.CardData.Name}' 카드 덱 추가 성공! (Player 상태 갱신 완료)");
                    }
                    else
                    {
                        Debug.LogError("[MainSceneRewardUI] Player CardDeck is NULL! Falling back to direct save.");
                        FallbackDirectSave(card);
                    }
                }
                else
                {
                    FallbackDirectSave(card);
                }

                int currentRewards = PlayerPrefs.GetInt("MainScene_Rewards", 0);
                currentRewards++;
                
                if (currentRewards >= 5)
                {
                    PlayerPrefs.SetInt("MainScene_Rewards", 0);
                    Debug.Log("[MainSceneRewardUI] 5th reward reached! Transitioning to Battle...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
                }
                else
                {
                    PlayerPrefs.SetInt("MainScene_Rewards", currentRewards);
                    Debug.Log($"[MainSceneRewardUI] Reward count: {currentRewards}/5");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainSceneRewardUI] Exception in OnCardSelected: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                Debug.Log("[MainSceneRewardUI] Closing Reward UI (Bulletproof Finally Block)...");
                
                // Deactivate all Reward UIs in the scene
                var rewards = FindObjectsOfType<MainSceneRewardUI>();
                if (rewards != null)
                {
                    foreach (var r in rewards)
                    {
                        if (r != null && r.gameObject != null)
                        {
                            r.gameObject.SetActive(false);
                        }
                    }
                }

                // Restore cursor state
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void FallbackDirectSave(Cards.General.CardInstance card)
        {
            var savedData = Utilities.DeckUtility.LoadSavedDeckData();
            if (savedData == null) savedData = Utilities.DeckUtility.LoadStarterDeckData();
            
            if (savedData != null)
            {
                int id = card.CardData.Id;
                var index = savedData.Cards.FindIndex(x => x.CardId == id);
                if (index == -1) savedData.Cards.Add(new Deck.CardSaveData() { CardId = id, Count = 1 });
                else savedData.Cards[index].Count++;
                
                Utilities.PersistentData.Save(savedData, Battle.General.Constants.PlayerDeckIdentifier);
                Debug.Log($"[MainSceneRewardUI] 🎉 '{card.CardData.Name}' 카드 덱 추가 성공! (세이브 데이터에 직접 저장 완료)");
            }
            else
            {
                Debug.LogError("[MainSceneRewardUI] 치명적 오류: 세이브 데이터를 불러오지 못해 덱에 추가되지 않았습니다.");
            }
        }

        public void Close()
        {
            if (this == null) return;

            Debug.Log("[MainSceneRewardUI] Closing Reward UI...");
            
            // Robust close: Deactivate all Reward UIs in the scene
            var rewards = FindObjectsOfType<MainSceneRewardUI>();
            foreach (var r in rewards)
            {
                if (r != null && r.gameObject != null)
                {
                    r.gameObject.SetActive(false);
                }
            }

            m_isProcessing = false;

            // Re-lock Cursor ONLY if in "Main" scene!
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnGUI()
        {
            // 보상 획득 횟수를 화면 좌측 상단에 표시
            int currentRewards = PlayerPrefs.GetInt("MainScene_Rewards", 0);
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.yellow;

            GUI.Label(new Rect(20, 80, 400, 50), $"Rewards Collected: {currentRewards} / 5", style);
            
            if (m_isProcessing)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Processing Choice...", style);
            }
        }
    }
}
