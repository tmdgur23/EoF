using MessagePack;
using Units.General;
using UnityEngine;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class ConvertType2Effect : DiceRollEffect
	{
		public ConvertType2Effect()
		{
			MinRoll = 1;
			MaxRoll = 6;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			int amount = roll * 2;
			int choice = Random.Range(0, 2); // 0: Damage, 1: Soul
			if (choice == 0)
			{
				if (target != null)
					target.ApplyDamage(UseLens(from, target, amount), from);
			}
			else if (choice == 1)
			{
				if (target != null)
					target.ChangeSoul(UseLens(target, null, amount), false);
			}
		}
	}
}
