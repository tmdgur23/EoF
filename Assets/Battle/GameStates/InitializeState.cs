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

			// ================== 다음 전투 적 디버프 적용 ==================
			int nextVul = PlayerPrefs.GetInt("Next_VUL", 0);
			if (nextVul > 0)
			{
				foreach (var enemy in BattleInfo.Encounter.Enemies)
				{
					if (enemy == null) continue;
					enemy.StatusContainer.Setup(enemy); // Ensure StatusContainer is setup before applying status
					var data = new Status.Types.VulnerabilityData { Percentage = 50, BuffType = Cards.General.BuffType.Debuff };
					var status = new Status.Types.Vulnerability(data, enemy);
					status.AddStacks(nextVul - 1); // 기본 1스택에 추가
					enemy.StatusContainer.Apply(status);
				}
				PlayerPrefs.SetInt("Next_VUL", 0);
				UnityEngine.Debug.Log($"[InitializeState] 다음 전투 적 취약 버프 적용: {nextVul} 스택");
			}
			// =========================================================
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
			if (BattleInfo.Player != null)
			{
				BattleInfo.Player.StatusContainer.Setup(BattleInfo.Player);
			}

			// 1. 방어도 추가
			int nextDef = PlayerPrefs.GetInt("Next_DEF", 0);
			if (nextDef > 0)
			{
				BattleInfo.Player.Defense.Current = nextDef;
				UnityEngine.Debug.Log($"[InitializeState] 다음 전투 방어도 버프 적용: +{nextDef}");
				
				// 배틀씬 플레이어 디스플레이용 상태 아이콘 추가
				var data = new Status.Types.RuntimeStatusData(
					"추가 방어도", 
					"Status/Icons/IconBullwark", 
					$"다음 전투 시작 시 추가 방어도를 {nextDef}만큼 획득했습니다.", 
					Cards.General.BuffType.Buff
				);
				var status = new Status.Types.RuntimeStatus(data, BattleInfo.Player, nextDef);
				BattleInfo.Player.StatusContainer.Apply(status);

				PlayerPrefs.SetInt("Next_DEF", 0); // 사용 후 리셋
			}

			// 2. 드로우 추가
			int nextDraw = PlayerPrefs.GetInt("Next_DRAW", 0);
			if (nextDraw > 0)
			{
				BattleInfo.Player.HandSize = 5 + nextDraw;
				UnityEngine.Debug.Log($"[InitializeState] 다음 전투 드로우 버프 적용: +{nextDraw} (총 {BattleInfo.Player.HandSize}장)");

				// 배틀씬 플레이어 디스플레이용 상태 아이콘 추가
				var data = new Status.Types.RuntimeStatusData(
					"추가 드로우", 
					"Status/Icons/IconBountyOfFaith", 
					$"다음 전투 첫 턴에 카드를 {nextDraw}장 추가로 드로우했습니다.", 
					Cards.General.BuffType.Buff
				);
				var status = new Status.Types.RuntimeStatus(data, BattleInfo.Player, nextDraw);
				BattleInfo.Player.StatusContainer.Apply(status);

				PlayerPrefs.SetInt("Next_DRAW", 0); // 사용 후 리셋
			}
			else
			{
				BattleInfo.Player.HandSize = 5; // 기본값 보장
			}
			// =========================================================

			// ================== 영구 스탯 보너스 적용 ==================
			BattleInfo.Player.BonusStrength = PlayerPrefs.GetInt("Bonus_STR", 0);
			BattleInfo.Player.BonusMental = PlayerPrefs.GetInt("Bonus_MEN", 0);
			UnityEngine.Debug.Log($"[InitializeState] 영구 스탯 적용: 근력(STR) {BattleInfo.Player.BonusStrength}, 정신력(MEN) {BattleInfo.Player.BonusMental}");
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