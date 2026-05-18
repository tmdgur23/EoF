using Battle.General;
using Cards.General;
using Misc;
using Misc.Events;
using Status;
using UnityEngine;

namespace Battle.GameStates
{
	public class PlayerTurnState : GameState
	{
		private bool m_isFirstTurn = true;

		protected override void Start()
		{
			RestockPhase();
			StatusPhase();

			EventLog.Add(new TurnStart(BattleInfo.Player.name));
			m_isFirstTurn = false;
		}

		/// <summary>
		/// Refill energy and draw players hand draws cards.
		/// </summary>
		private void RestockPhase()
		{
			BattleInfo.TurnPhase = TurnPhase.Restock;
			BattleInfo.Player.Energy.Refill();

			if (m_isFirstTurn)
			{
				BattleInfo.Player.CreateDrawPile();
			}
			else
			{
				// 첫 턴 이후부터는 드로우 보너스를 제거하고 기본 5장으로 복구합니다.
				BattleInfo.Player.HandSize = 5;
				BattleInfo.Player.ReCreateDrawPile();
			}

			TriggerEarlyEffects();
			BattleInfo.Player.CreateHand();
		}

		/// <summary>
		/// Early card effect from cards that are in the draw pile, will be triggered.
		/// </summary>
		private void TriggerEarlyEffects()
		{
			var drawPile = BattleInfo.Player.DrawPile;

			foreach (var card in drawPile.Cards)
			{
				var earlyEffects = card.CardData.EarlyEffects;

				foreach (var earlyEffect in earlyEffects)
				{
					earlyEffect.Use(null, BattleInfo.Player, card.CardData.TargetType);
				}
			}
		}

		private void StatusPhase()
		{
			BattleInfo.TurnPhase = TurnPhase.StatusPhase;
			var player = BattleInfo.Player;
			
			// 첫 턴이 아닐 때만 방어도를 초기화합니다. (보너스 방어도 유지용)
			if (!m_isFirstTurn)
			{
				player.Defense.Current = 0;
			}

			player.StatusContainer.UpdateDuration();
		}

		protected override void Update()
		{
			PlayPhase();
		}

		/// <summary>
		/// Player plays his cards, ends when player ends his turn.
		/// </summary>
		private void PlayPhase()
		{
			BattleInfo.TurnPhase = TurnPhase.PlayPhase;
		}

		/// <summary>
		/// To Finish this turn.
		/// </summary>
		public void EndState()
		{
			if (BattleInfo.Player.IsDone)
			{
				IsCompleted = true;
			}
		}

		protected override void Finish()
		{
			EndPhase();
		}

		/// <summary>
		/// Status effects, are triggered hand cards are discarded.
		/// </summary>
		private void EndPhase()
		{
			BattleInfo.TurnPhase = TurnPhase.EndPhase;
			EventLog.Add(new TurnEnd(BattleInfo.Player.name));

			var playerHand = BattleInfo.Player.Hand;
			playerHand.DiscardAll();
			CardTurnLog.Clear();
		}
	}
}