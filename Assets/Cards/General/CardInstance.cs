using System.Collections.Generic;
using System.Linq;
using Units.Enemy.General;
using Units.General;
using Units.Player.General;
using VFX;

namespace Cards.General
{
	[System.Serializable]
	public class CardInstance
	{
		public CardData CardData;
		public int LastPlayEffect { get; private set; }
		public Unit Owner { get; private set; }
		public Unit Target { get; set; }
		public int CostReduction { get; set; } = 0;
		public int EnergyCost => UnityEngine.Mathf.Max(0, CardData.Energy - CostReduction);

		public CardInstance(CardData cardData)
		{
			CardData = cardData;
		}

		public CardInstance(CardData cardData, Unit owner)
		{
			CardData = cardData;
			Owner = owner;
		}

		public bool CanPlay(Player player)
		{
			var hasEnergy = player.Energy.Current >= EnergyCost;

			if (CardData.PlayCondition.Any(conditionEffect => !conditionEffect.HasReached(player)))
			{
				return false;
			}

			return hasEnergy;
		}

		public System.Collections.IEnumerator PlayCoroutine(Enemy enemy)
		{
			var diceSystem = UnityEngine.Object.FindObjectOfType<DiceSystem>();

			for (LastPlayEffect = 0; LastPlayEffect < CardData.PlayEffect.Count; LastPlayEffect++)
			{
				var effect = CardData.PlayEffect[LastPlayEffect];
				effect.Use(enemy, Owner, CardData.TargetType);

				if (diceSystem != null)
				{
					bool interactedWithDice = false;
					while (diceSystem.IsBusy)
					{
						interactedWithDice = true;
						yield return null;
					}

					// 주사위를 굴린 직후라면, 다음 효과(혹은 두 번째 주사위)로 넘어가기 전에 UI가 확실히 사라졌음을 보여주기 위해 짧은 대기시간을 가집니다.
					if (interactedWithDice)
					{
						yield return new UnityEngine.WaitForSeconds(0.4f);
					}
				}
			}

			CardVFXHandler.Instance.Play(CardData.VFXIndex, CardData.TargetType);
		}

		public bool CanDiscard(Player player)
		{
			if (CardData.DiscardCondition.Count <= 0) return true;

			foreach (var condition in CardData.DiscardCondition)
			{
				if (!condition.HasReached(player))
					return false;
			}

			return true;
		}

		private string ParseValues(string description)
		{
			return DescriptionParser.Parse(description,
										   Owner,
										   Target,
										   new List<IDescriptionValue>(CardData.EarlyEffects),
										   new List<IDescriptionValue>(CardData.PlayCondition),
										   new List<IDescriptionValue>(CardData.PlayEffect),
										   new List<IDescriptionValue>(CardData.DiscardCondition)
										  );
		}

		public string Description(string description)
		{
			return ParseValues(description);
		}
	}
}