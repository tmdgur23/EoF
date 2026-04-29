using Battle.General;
using Deck;
using Units.Enemy.General;
using Utilities;

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
		}

#region Ignore

		//can be ignored,not be called
		protected override void Update() { }

		//can be ignored,not be called
		protected override void Finish() { }

#endregion
	}
}