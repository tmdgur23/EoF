using Battle.General;
using MessagePack;
using Units.General;
using UnityEngine;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class MadnessType3Effect : DiceRollEffect
	{
		public MadnessType3Effect()
		{
			MinRoll = 1;
			MaxRoll = 6;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			// 첫 번째 주사위 결과를 저장하고, 두 번째 주사위를 DiceSystem에 Enqueue
			var secondRollEffect = new MadnessType3SecondRollEffect(roll);
			var diceSystem = Object.FindObjectOfType<DiceSystem>();
			if (diceSystem != null)
			{
				diceSystem.EnqueueRoll(secondRollEffect, target, from);
			}
		}
	}

	/// <summary>
	/// Madness Type3의 두 번째 주사위 - 첫 번째 결과와 곱해서 데미지 적용
	/// </summary>
	[MessagePackObject(true)]
	public class MadnessType3SecondRollEffect : DiceRollEffect
	{
		public int FirstRoll;

		public MadnessType3SecondRollEffect()
		{
			MinRoll = 1;
			MaxRoll = 6;
		}

		public MadnessType3SecondRollEffect(int firstRoll)
		{
			MinRoll = 1;
			MaxRoll = 6;
			FirstRoll = firstRoll;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			int damageAmount = FirstRoll * roll;

			if (target != null)
			{
				target.ApplyDamage(damageAmount, from);
			}

			int selfDamage = damageAmount / 3;
			if (selfDamage > 0)
			{
				from.ApplyDamage(selfDamage, from);
			}
		}
	}
}
