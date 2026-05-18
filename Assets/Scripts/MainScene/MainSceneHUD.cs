using UnityEngine;
using UnityEngine.UI;
using Battle.General;
using OptionMenu;
using TMPro;
using Units.General;

namespace MainScene
{
    public class MainSceneHUD : MonoBehaviour
    {
        public static MainSceneHUD Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("현재 체력 텍스트 (예: 100/100)")]
        public TextMeshProUGUI hpText;
        [Tooltip("체력바 이미지 (Fill Amount 사용)")]
        public Image hpFillImage;
        
        [Tooltip("현재 소울 텍스트")]
        public TextMeshProUGUI soulText;
        [Tooltip("소울 원형 게이지 스크립트")]
        public RadialBar soulRadialBar;

        private GameObject buffsContainer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            BattleConfig config = Options.LoadConfigData();

            // 체력 처리
            if (config.Health != null && config.Health.Max > 0)
            {
                if (hpText != null)
                {
                    hpText.text = $"{config.Health.Min} / {config.Health.Max}";
                }

                if (hpFillImage != null)
                {
                    hpFillImage.fillAmount = (float)config.Health.Min / config.Health.Max;
                }
            }
            else
            {
                // 아직 전투 씬을 한 번도 안 가서 Health가 세팅 안 된 경우 기본 40으로 표시
                if (hpText != null) hpText.text = "40 / 40";
                if (hpFillImage != null) hpFillImage.fillAmount = 1f;
            }

            // 소울 처리
            if (soulText != null)
            {
                soulText.text = config.Soul.ToString(); // 배틀씬처럼 숫자만 표시
            }

            // 소울 게이지 채우기
            if (soulRadialBar != null)
            {
                // 참고: 메인 씬에는 최대 소울 정보가 없으므로 일단 50을 기준으로 잡습니다. (필요시 조절)
                float maxSoul = 50f;
                float minSoul = -50f;

                if (config.Soul >= 0)
                {
                    soulRadialBar.TopFillAmount = config.Soul / maxSoul;
                }
                else
                {
                    soulRadialBar.BottomFillAmount = config.Soul / minSoul;
                }
            }

            // 다음 전투 버프/디버프 아이콘 업데이트
            UpdateNextBattleBuffs();
        }

        private void UpdateNextBattleBuffs()
        {
            int nextDef = PlayerPrefs.GetInt("Next_DEF", 0);
            int nextDraw = PlayerPrefs.GetInt("Next_DRAW", 0);
            int nextVul = PlayerPrefs.GetInt("Next_VUL", 0);

            // 버프가 아무것도 없으면 컨테이너 비활성화 후 리턴
            if (nextDef <= 0 && nextDraw <= 0 && nextVul <= 0)
            {
                if (buffsContainer != null) buffsContainer.SetActive(false);
                return;
            }

            // 컨테이너가 없으면 생성
            if (buffsContainer == null)
            {
                Transform parentTransform = transform;
                if (hpFillImage != null && hpFillImage.transform.parent != null)
                {
                    parentTransform = hpFillImage.transform.parent; // 체력바 자체 또는 체력바의 부모
                }

                GameObject containerGo = new GameObject("NextBattleBuffsContainer");
                containerGo.transform.SetParent(parentTransform, false);
                
                RectTransform containerRect = containerGo.AddComponent<RectTransform>();
                
                // 체력바 바로 아래에 배치
                containerRect.anchorMin = new Vector2(0f, 0f);
                containerRect.anchorMax = new Vector2(1f, 0f);
                containerRect.pivot = new Vector2(0.5f, 1f);
                
                // Y 위치를 체력바 밑으로 -18만큼 조절하고 높이는 32
                containerRect.anchoredPosition = new Vector2(0f, -18f);
                containerRect.sizeDelta = new Vector2(0f, 32f);

                // 부모의 LayoutGroup 영향 차단용
                LayoutElement layoutElement = containerGo.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                HorizontalLayoutGroup layout = containerGo.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 6f; // tighter spacing
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                buffsContainer = containerGo;
            }

            buffsContainer.SetActive(true);

            // 기존 자식 오브젝트들 전부 파괴
            foreach (Transform child in buffsContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // 각 버프 아이콘 생성
            if (nextDef > 0)
            {
                CreateBuffIcon("Status/Icons/IconBullwark", nextDef.ToString(), "다음 전투 추가 방어도", $"다음 전투 시작 시 추가 방어도를 {nextDef}만큼 획득합니다.", Color.cyan);
            }
            if (nextDraw > 0)
            {
                CreateBuffIcon("Status/Icons/IconBountyOfFaith", nextDraw.ToString(), "다음 전투 추가 드로우", $"다음 전투 첫 턴에 카드를 {nextDraw}장 추가로 드로우합니다.", Color.green);
            }
            if (nextVul > 0)
            {
                CreateBuffIcon("Status/Icons/IconVulnerability", nextVul.ToString(), "다음 전투 적 취약", $"다음 전투 시작 시 모든 적에게 취약 디버프 {nextVul}중첩을 부여합니다.", Color.red);
            }
        }

        private void CreateBuffIcon(string spriteResourcePath, string countText, string buffName, string buffDesc, Color fallbackColor)
        {
            GameObject iconGo = new GameObject(buffName);
            iconGo.transform.SetParent(buffsContainer.transform, false);

            RectTransform rect = iconGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30f, 30f);

            // 배경용 반투명 어두운 원판
            Image bgImg = iconGo.AddComponent<Image>();
            bgImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);
            bgImg.raycastTarget = true;

            // 실제 아이콘 이미지 (자식)
            GameObject imgGo = new GameObject("Icon");
            imgGo.transform.SetParent(iconGo.transform, false);
            RectTransform imgRect = imgGo.AddComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.sizeDelta = new Vector2(-4f, -4f); // 살짝 안쪽으로 여백

            Image iconImg = imgGo.AddComponent<Image>();
            iconImg.raycastTarget = false;
            Sprite sprite = Resources.Load<Sprite>(spriteResourcePath);
            if (sprite != null)
            {
                iconImg.sprite = sprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = fallbackColor;
            }

            // 카운터 텍스트 (우하단 배치)
            GameObject txtGo = new GameObject("Counter");
            txtGo.transform.SetParent(iconGo.transform, false);
            RectTransform txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0.5f, 0f);
            txtRect.anchorMax = new Vector2(1f, 0.5f);
            txtRect.pivot = new Vector2(1f, 0f);
            txtRect.anchoredPosition = new Vector2(3f, -3f);
            txtRect.sizeDelta = new Vector2(25f, 15f);

            TextMeshProUGUI txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.text = countText;
            txt.fontSize = 11;
            txt.alignment = TextAlignmentOptions.BottomRight;
            txt.color = Color.yellow;
            txt.fontStyle = FontStyles.Bold;
            txt.raycastTarget = false;

            if (hpText != null)
            {
                txt.font = hpText.font;
            }

            // 툴팁 스크립트 부착
            MainSceneBuffTooltip tooltip = iconGo.AddComponent<MainSceneBuffTooltip>();
            tooltip.buffName = buffName;
            tooltip.buffDesc = buffDesc;
        }
    }
}

