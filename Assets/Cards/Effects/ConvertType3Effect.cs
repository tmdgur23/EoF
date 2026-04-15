using MessagePack;
using Units.General;
using UnityEngine;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class ConvertType3Effect : DiceRollEffect
	{
		public ConvertType3Effect()
		{
			MinRoll = 12;
			MaxRoll = 18;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			int damageAmount = Random.Range(0, roll + 1);
			int defenseAmount = roll - damageAmount;

			if (damageAmount > 0 && target != null)
			{
				target.ApplyDamage(UseLens(from, target, damageAmount), from);
			}

			if (defenseAmount > 0)
			{
				from.ChangeBlock(UseLens(from, null, defenseAmount), false);
			}
		}
	}
}
