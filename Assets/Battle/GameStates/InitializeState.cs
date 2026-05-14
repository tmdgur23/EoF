using Battle.General;
using Deck;
using Units.Enemy.General;
using Utilities;
using UnityEngine;

namespace Battle.GameStates
{
	/// <summary>
	/// Setup Player DrawPile and Enemies based on config.
	/// </summary>
	public class InitializeState : GameState
	{
		private readonly BattleConfig m_config;
		private readonly DeckSaveData m_deckSaveData;

		public InitializeState(BattleConfig config)
		{
			m_deckSaveData = DeckUtility.LoadSavedDeckData();
			m_config = config;
		}

		protected override void Start()
		{
			SetUpPlayer();
			CreateDeck();
			CreateEnemies();
		}

		/// <summary>
		/// Creating deck from previous loaded data.
		/// </summary>
		private void CreateDeck()
		{
			var deck = DeckFactory.Build(m_deckSaveData, BattleInfo.Player).Cards;
			deck.Shuffle();
			BattleInfo.Player.CardDeck.Cards = deck;
		}

		/// <summary>
		/// Based on battle count that is stored in the config file, creating an Encounter.
		/// </summary>
		private void CreateEnemies()
		{
			BattleInfo.Encounter.Enemies = EncounterFactory.Build(m_config).Enemies;
		}

		/// <summary>
		/// Set previous loaded save data.
		/// </summary>
		private void SetUpPlayer()
		{
			// 첫 전투(BattleCount == 0)라 하더라도, 메인씬에서 체력 보상을 받아
			// config.Health가 세팅되어 있다면 그 값을 불러와야 합니다.
			if (m_config.Health != null && m_config.Health.Max > 0)
			{
				BattleInfo.Player.Health.Set(m_config.Health.Min,
											 m_config.Health.Max);

				BattleInfo.Player.Soul.Set(BattleInfo.Player.Soul.Min,
										   m_config.Soul,
										   m_config.Health.Max);
			}
			else if (m_config.BattleCount > 0)
			{
				// 만약 Health 데이터가 없는데 배틀 카운트가 진행된 경우의 예외 처리 (기존 안전 장치)
				BattleInfo.Player.Health.Set(m_config.Health.Min,
											 m_config.Health.Max);

				BattleInfo.Player.Soul.Set(BattleInfo.Player.Soul.Min,
										   m_config.Soul,
										   m_config.Health.Max);
			}

			// ================== 다음 전투 버프 적용 ==================
			// 1. 방어도 추가
			int nextDef = PlayerPrefs.GetInt("Next_DEF", 0);
			if (nextDef > 0)
			{
				BattleInfo.Player.Defense.Current = nextDef;
				UnityEngine.Debug.Log($"[InitializeState] 다음 전투 방어도 버프 적용: +{nextDef}");
				PlayerPrefs.SetInt("Next_DEF", 0); // 사용 후 리셋
			}

			// 2. 드로우 추가
			int nextDraw = PlayerPrefs.GetInt("Next_DRAW", 0);
			if (nextDraw > 0)
			{
				BattleInfo.Player.HandSize = 5 + nextDraw;
				UnityEngine.Debug.Log($"[InitializeState] 다음 전투 드로우 버프 적용: +{nextDraw} (총 {BattleInfo.Player.HandSize}장)");
				PlayerPrefs.SetInt("Next_DRAW", 0); // 사용 후 리셋
			}
			else
			{
				BattleInfo.Player.HandSize = 5; // 기본값 보장
			}

			// 3. 주사위(재굴림) 추가 - DiceSystem에서 참조할 수 있도록 PlayerPrefs에 남겨두거나 정적 변수 활용
			// 여기서는 PlayerPrefs를 그대로 두고 DiceSystem이 읽어간 뒤 리셋하도록 함
			UnityEngine.Debug.Log($"[InitializeState] 다음 전투 주사위 버프 예약 확인: {PlayerPrefs.GetInt("Next_DICE", 0)}");
			// =========================================================
		}

#region Ignore

		//can be ignored,not be called
		protected override void Update() { }

		//can be ignored,not be called
		protected override void Finish() { }

#endregion
	}
}