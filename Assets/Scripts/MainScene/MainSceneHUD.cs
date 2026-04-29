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
        }
    }
}
