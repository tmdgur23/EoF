using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Misc.PopUp;
using MainScene;

namespace Battle
{
    /// <summary>
    /// 배틀씬에서 메인씬과 동일한 스탯 HUD(구슬 3개)를 런타임으로 생성합니다.
    /// Battle 프리팹의 PlayerTurn 또는 Battle 루트 오브젝트에 붙여두세요.
    /// </summary>
    public class BattleStatHUD : MonoBehaviour
    {
        private void Start()
        {
            CreateStatHUD();
            StartCoroutine(DiagnosticCoroutine());
        }

        private void CreateStatHUD()
        {
            // 씬에 이미 있는 Canvas(RootCanvas)를 찾아서 우선적으로 사용합니다.
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            Canvas existingCanvas = null;

            // 1. "RootCanvas" 매칭
            foreach (var c in allCanvases)
            {
                if (c.name == "RootCanvas")
                {
                    existingCanvas = c;
                    break;
                }
            }

            // 2. "HUD" 매칭
            if (existingCanvas == null)
            {
                foreach (var c in allCanvases)
                {
                    if (c.name == "HUD")
                    {
                        existingCanvas = c;
                        break;
                    }
                }
            }

            // 3. 기타 ScreenSpaceOverlay 중 활성화된 캔버스 매칭
            if (existingCanvas == null)
            {
                foreach (var c in allCanvases)
                {
                    if (c.gameObject.activeInHierarchy && c.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        existingCanvas = c;
                        break;
                    }
                }
            }

            Transform canvasTransform;
            if (existingCanvas != null)
            {
                canvasTransform = existingCanvas.transform;
                Debug.Log($"[BattleStatHUD] Found existing Canvas for StatHUD: {existingCanvas.name}");
            }
            else
            {
                // 없으면 새로 생성
                GameObject canvasGo = new GameObject("StatHUD_Canvas");
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
                canvasTransform = canvasGo.transform;
                Debug.Log("[BattleStatHUD] Created new Canvas for StatHUD.");
            }

            // EventSystem이 없으면 추가
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // 1. Try loading the prefab from Resources
            GameObject prefab = Resources.Load<GameObject>("Prefabs/StatHUD");
            if (prefab != null)
            {
                GameObject instantiatedHUD = Instantiate(prefab, canvasTransform, false);
                instantiatedHUD.name = "StatHUD";
                
                // UI 스케일 보장 및 크기 1.35배 키움
                instantiatedHUD.transform.localScale = new Vector3(1.35f, 1.35f, 1.35f);
                
                // MainRect(배틀씬 배경 판때기)보다 앞에 오도록 가장 최근 자식 순서로 배치
                instantiatedHUD.transform.SetAsLastSibling();

                // 독립적인 캔버스 및 레이캐스터 추가하여 항상 최상단 렌더링 및 마우스 레이캐스트 보장
                Canvas hudCanvas = instantiatedHUD.GetComponent<Canvas>();
                if (hudCanvas == null)
                {
                    hudCanvas = instantiatedHUD.AddComponent<Canvas>();
                }
                hudCanvas.overrideSorting = true;
                hudCanvas.sortingOrder = 4; // HUD 캔버스와 동일 레벨로 설정하여 툴팁 팝업(5)보다 아래에 위치

                if (instantiatedHUD.GetComponent<GraphicRaycaster>() == null)
                {
                    instantiatedHUD.AddComponent<GraphicRaycaster>();
                }

                RectTransform rect = instantiatedHUD.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // 좌상단(Top-Left) 앵커 및 위치 설정 (유저 요청: 더 아래로 Y이동, 더 오른쪽으로 X이동)
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(115f, -250f);
                }

                // 툴팁 활성화를 위해 MainSceneStatUI 컴포넌트 동적 부착 및 레이캐스트 활성화
                AttachStatUI(instantiatedHUD.transform, "STR_Stat", global::MainScene.MainSceneStatUI.StatType.Strength);
                AttachStatUI(instantiatedHUD.transform, "INT_Stat", global::MainScene.MainSceneStatUI.StatType.Intelligence);
                AttachStatUI(instantiatedHUD.transform, "MEN_Stat", global::MainScene.MainSceneStatUI.StatType.Mental);

                Debug.Log($"[BattleStatHUD] Successfully instantiated StatHUD prefab in battle scene under canvas: {canvasTransform.name}");
                return;
            }

            Debug.LogWarning("[BattleStatHUD] Prefab 'Prefabs/StatHUD' not found. Falling back to procedural StatHUD.");

            // 2. HUD 컨테이너 (좌상단 소울바 아래, 메인씬과 동일 위치) - Fallback
            GameObject container = new GameObject("StatHUDContainer");
            container.transform.SetParent(canvasTransform, false);
            // MainRect보다 앞에 오도록 가장 최근 자식 순서로 배치
            container.transform.SetAsLastSibling();

            // 독립적인 캔버스 및 레이캐스터 추가 (동일하게 레이캐스트 보장)
            Canvas fallbackCanvas = container.AddComponent<Canvas>();
            fallbackCanvas.overrideSorting = true;
            fallbackCanvas.sortingOrder = 4;
            container.AddComponent<GraphicRaycaster>();

            RectTransform containerRect = container.AddComponent<RectTransform>();
            // 좌상단(Top-Left) 기준 앵커
            containerRect.anchorMin = new Vector2(0f, 1f);
            containerRect.anchorMax = new Vector2(0f, 1f);
            containerRect.pivot = new Vector2(0.5f, 1f);
            containerRect.anchoredPosition = new Vector2(115f, -250f);
            containerRect.sizeDelta = new Vector2(250f, 60f);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // 3. 각 구슬 생성
            CreateOrb(container.transform, "STR_Stat", "MarbleRed", global::MainScene.MainSceneStatUI.StatType.Strength);
            CreateOrb(container.transform, "INT_Stat", "MarbleBlue", global::MainScene.MainSceneStatUI.StatType.Intelligence);
            CreateOrb(container.transform, "MEN_Stat", "MarblePurple", global::MainScene.MainSceneStatUI.StatType.Mental);
        }

        private void CreateOrb(Transform parent, string objName, string spriteName, global::MainScene.MainSceneStatUI.StatType statType)
        {
            // 마스크 컨테이너 (원형 클리핑)
            GameObject orbGo = new GameObject(objName);
            orbGo.transform.SetParent(parent, false);

            RectTransform orbRect = orbGo.AddComponent<RectTransform>();
            orbRect.sizeDelta = new Vector2(38f, 38f);

            // 원형 마스크용 Knob 스프라이트
            Image maskImg = orbGo.AddComponent<Image>();
            Sprite knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            maskImg.sprite = knob;
            maskImg.color = Color.white;
            maskImg.raycastTarget = true; // 레이캐스트 허용

            Mask mask = orbGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // 구슬 이미지 (자식)
            GameObject marbleGo = new GameObject("MarbleImage");
            marbleGo.transform.SetParent(orbGo.transform, false);

            RectTransform marbleRect = marbleGo.AddComponent<RectTransform>();
            marbleRect.anchorMin = new Vector2(0.5f, 0.5f);
            marbleRect.anchorMax = new Vector2(0.5f, 0.5f);
            marbleRect.pivot = new Vector2(0.5f, 0.5f);
            marbleRect.anchoredPosition = Vector2.zero;
            marbleRect.sizeDelta = new Vector2(65f, 65f);

            Image marbleImg = marbleGo.AddComponent<Image>();
            marbleImg.raycastTarget = true; // 레이캐스트 허용

            // Resources 폴더에서 스프라이트 로드 시도, 실패하면 색으로 대체
            Sprite marble = Resources.Load<Sprite>("UI/" + spriteName);
            if (marble == null)
            {
                // Assets/MainScene/UI/ 에서 직접 로드 시도 (Resources 아닌 경우)
                // 런타임에서는 Resources 폴더만 접근 가능하므로 색상으로 대체
                marbleImg.color = GetFallbackColor(statType);
                Debug.LogWarning($"[BattleStatHUD] {spriteName} 스프라이트를 찾을 수 없어 색상으로 대체합니다. Resources/UI/ 폴더에 스프라이트를 배치하세요.");
            }
            else
            {
                marbleImg.sprite = marble;
                marbleImg.color = Color.white;
            }

            // StatUI 컴포넌트 부착 (툴팁용)
            global::MainScene.MainSceneStatUI statUI = orbGo.AddComponent<global::MainScene.MainSceneStatUI>();
            statUI.type = statType;
        }

        private void AttachStatUI(Transform parent, string childName, global::MainScene.MainSceneStatUI.StatType statType)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                var statUI = child.GetComponent<global::MainScene.MainSceneStatUI>();
                if (statUI == null)
                {
                    statUI = child.gameObject.AddComponent<global::MainScene.MainSceneStatUI>();
                }
                statUI.type = statType;
                
                // 마스크 및 이미지 컴포넌트의 Raycast Target 활성화
                var img = child.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                }
                
                Transform marble = child.Find("MarbleImage");
                if (marble != null)
                {
                    var marbleImg = marble.GetComponent<Image>();
                    if (marbleImg != null)
                    {
                        marbleImg.raycastTarget = true;
                    }
                }
            }
        }

        private Color GetFallbackColor(global::MainScene.MainSceneStatUI.StatType statType)
        {
            switch (statType)
            {
                case global::MainScene.MainSceneStatUI.StatType.Strength:    return new Color(0.85f, 0.2f, 0.2f);
                case global::MainScene.MainSceneStatUI.StatType.Intelligence: return new Color(0.2f, 0.4f, 0.85f);
                case global::MainScene.MainSceneStatUI.StatType.Mental:       return new Color(0.6f, 0.2f, 0.8f);
                default: return Color.white;
            }
        }

        private IEnumerator DiagnosticCoroutine()
        {
            yield return new WaitForSeconds(1.0f);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== BattleStatHUD Runtime Diagnostics ===");
            
            // Find all Canvases
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            sb.AppendLine($"Total Canvases in scene: {canvases.Length}");
            foreach (var c in canvases)
            {
                sb.AppendLine($"- Canvas Name: {c.name}, activeInHierarchy: {c.gameObject.activeInHierarchy}, enabled: {c.enabled}, renderMode: {c.renderMode}, sortingOrder: {c.sortingOrder}");
            }

            GameObject hud = GameObject.Find("StatHUD");
            if (hud == null)
            {
                sb.AppendLine("ERROR: Could not find 'StatHUD' GameObject in the scene!");
            }
            else
            {
                sb.AppendLine($"StatHUD name: {hud.name}, activeSelf: {hud.activeSelf}, activeInHierarchy: {hud.activeInHierarchy}");
                Transform parent = hud.transform.parent;
                
                string parentName = (parent != null) ? parent.name : "null";
                sb.AppendLine($"Parent: {parentName}");
                
                if (parent != null)
                {
                    Canvas parentCanvas = parent.GetComponentInParent<Canvas>();
                    string parentCanvasName = (parentCanvas != null) ? parentCanvas.name : "null";
                    string parentCanvasEnabled = (parentCanvas != null) ? parentCanvas.enabled.ToString() : "N/A";
                    string parentCanvasRender = (parentCanvas != null) ? parentCanvas.renderMode.ToString() : "N/A";
                    string parentCanvasSort = (parentCanvas != null) ? parentCanvas.sortingOrder.ToString() : "N/A";
                    
                    sb.AppendLine($"Parent Canvas: {parentCanvasName}, enabled: {parentCanvasEnabled}, renderMode: {parentCanvasRender}, sortingOrder: {parentCanvasSort}");
                    
                    CanvasGroup parentGroup = hud.GetComponentInParent<CanvasGroup>();
                    if (parentGroup != null)
                    {
                        sb.AppendLine($"Found CanvasGroup in parent hierarchy: alpha={parentGroup.alpha}, interactable={parentGroup.interactable}, blocksRaycasts={parentGroup.blocksRaycasts}");
                    }
                }

                RectTransform rect = hud.GetComponent<RectTransform>();
                if (rect != null)
                {
                    sb.AppendLine($"RectTransform - anchorMin: {rect.anchorMin}, anchorMax: {rect.anchorMax}, pivot: {rect.pivot}");
                    sb.AppendLine($"RectTransform - anchoredPosition: {rect.anchoredPosition}, sizeDelta: {rect.sizeDelta}");
                    sb.AppendLine($"Transform - localScale: {hud.transform.localScale}, position: {hud.transform.position}");
                }

                // Inspect children
                for (int i = 0; i < hud.transform.childCount; i++)
                {
                    Transform child = hud.transform.GetChild(i);
                    sb.AppendLine($"Child [{i}]: {child.name}, activeSelf: {child.gameObject.activeSelf}, activeInHierarchy: {child.gameObject.activeInHierarchy}, localScale: {child.localScale}, position: {child.position}");
                    Image img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        string spriteName = (img.sprite != null) ? img.sprite.name : "null";
                        sb.AppendLine($"Child [{i}] Image - enabled: {img.enabled}, color: {img.color}, sprite: {spriteName}");
                    }
                }
            }

            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "battle_hud_debug.txt"), sb.ToString());
                Debug.Log("[BattleStatHUD Diagnostic] Successfully wrote diagnostic report to Assets/battle_hud_debug.txt");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[BattleStatHUD Diagnostic] Failed to write report: " + ex.Message);
            }
        }
    }
}
