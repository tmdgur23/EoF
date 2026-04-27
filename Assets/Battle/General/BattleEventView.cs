using OptionMenu;
using SCT;
using Units.Enemy.General;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

#pragma warning disable 0649
namespace Battle.General
{
	/// <summary>
	/// Provides drag n drop Events.
	/// </summary>
	[RequireComponent(typeof(BattleTurnProcedure))]
	public class BattleEventView : MonoBehaviour
	{
		public UnityEvent OnPlayerWon;
		public UnityEvent OnPlayerLost;
		public UnityEvent OnGameEnd;
		public UnityEvent OnPlayerTurn;
		public UnityEvent OnEnemyTurn;
		
		[Header("UI Panels")]
		public GameObject winMenu;
		public GameObject loseMenu;

		private BattleTurnProcedure m_turnProcedure;

		private void Start()
		{
			m_turnProcedure = GetComponent<BattleTurnProcedure>();
			AddListener();
		}

		private void AddListener()
		{
			m_turnProcedure.PlayerWon += () =>
			{
				if (IsValidGameEnd())
				{
					if (winMenu == null)
					{
						var prefab = Resources.Load<GameObject>("WinMenu");
						if (prefab != null)
						{
							var canvas = Object.FindObjectOfType<Canvas>();
							if (canvas != null)
							{
								winMenu = Instantiate(prefab, canvas.transform);
							}
							else
							{
								winMenu = Instantiate(prefab);
							}
							winMenu.name = "WinMenu";
							
							// Hook up buttons
							var buttons = winMenu.GetComponentsInChildren<UnityEngine.UI.Button>(true);
							foreach (var btn in buttons)
							{
								string btnName = btn.name.ToLower();
								if (btnName.Contains("continue") || btnName.Contains("next") || btnName.Contains("confirm") || btnName.Contains("menu"))
								{
									btn.onClick.AddListener(OnContinueButtonClicked);
								}
							}
						}
					}

					if (winMenu != null)
					{
						winMenu.SetActive(true);
						OnGameEnd?.Invoke();
					}
					else
					{
						// Fallback if UI is missing
						Options.ResetConfigData();
						PlayerPrefs.SetInt("MainScene_Rewards", 0);
						UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
					}
				}
				else
				{
					// Normal battle! Skip UI and go directly to Main scene!
					UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
				}
			};

			m_turnProcedure.PlayerLost += () => 
            {
                Debug.Log("[BattleEventView] Player lost!");
				if (loseMenu == null)
				{
					var prefab = Resources.Load<GameObject>("LooseScreen");
					if (prefab != null)
					{
						var canvas = Object.FindObjectOfType<Canvas>();
						if (canvas != null)
						{
							loseMenu = Instantiate(prefab, canvas.transform);
						}
						else
						{
							loseMenu = Instantiate(prefab);
						}
						loseMenu.name = "LooseScreen";

						// Hook up buttons on Lose Menu (Specifically targeting Get resurrected / restart)
						var buttons = loseMenu.GetComponentsInChildren<UnityEngine.UI.Button>(true);
						foreach (var btn in buttons)
						{
							string btnName = btn.name.ToLower();
							var tmpro = btn.GetComponentInChildren<TextMeshProUGUI>();
							string btnText = tmpro != null ? tmpro.text.ToLower() : "";

							if (btnName.Contains("resurrect") || btnName.Contains("restart") || 
							    btnText.Contains("resurrect") || btnText.Contains("restart"))
							{
								btn.onClick.AddListener(() => 
								{
									Options.ResetConfigData();
									PlayerPrefs.SetInt("MainScene_Rewards", 0);
									UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
								});
							}
						}
					}
				}

				if (loseMenu != null) loseMenu.SetActive(true);
                OnPlayerLost?.Invoke();
            };

			m_turnProcedure.PlayerTurnStart += () =>
			{
				ScriptableTextDisplay.Instance.InitializeScriptableText(2, Vector3.zero, "Player Turn!");
				OnPlayerTurn?.Invoke();
			};

			m_turnProcedure.OpponentTurnStart += () =>
			{
				ScriptableTextDisplay.Instance.InitializeScriptableText(2, Vector3.zero, "Enemy Turn!");
				OnEnemyTurn?.Invoke();
			};
		}

		public void OnContinueButtonClicked()
		{
			if (IsValidGameEnd())
			{
				Options.ResetConfigData();
				PlayerPrefs.SetInt("MainScene_Rewards", 0);
				UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
			}
			else
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
			}
		}

		public void OnQuitToMenuButtonClicked()
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}

		public bool IsValidGameEnd()
		{
			var config = Options.LoadConfigData();
            // Sequence: Hound (0), Imp (1), BrokenSeal (2). 
            // config.BattleCount was incremented in BattleTurnProcedure.Save() before this call.
            // So after Hound: 1, after Imp: 2, after BrokenSeal: 3.
			return config.BattleCount >= 3;
		}
	}
}
#pragma warning restore 0649