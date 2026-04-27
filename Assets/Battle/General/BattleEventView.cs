using OptionMenu;
using SCT;
using Units.Enemy.General;
using UnityEngine;
using UnityEngine.Events;

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
				if (winMenu != null)
				{
					winMenu.SetActive(true);
					// The UI buttons should handle the scene loading now.
					// We can also trigger events if needed.
					if (IsValidGameEnd()) OnGameEnd?.Invoke();
					else OnPlayerWon?.Invoke();
				}
				else
				{
					// Fallback if UI is missing
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
			};

			m_turnProcedure.PlayerLost += () => 
            {
                Debug.Log("[BattleEventView] Player lost!");
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