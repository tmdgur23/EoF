using MessagePack;
using Units.General;
using UnityEngine;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class ConvertType1Effect : DiceRollEffect
	{
		public ConvertType1Effect()
		{
			MinRoll = 1;
			MaxRoll = 6;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			int choice = Random.Range(0, 3); // 0: Damage, 1: Defense, 2: Soul
			if (choice == 0)
			{
				if (target != null)
					target.ApplyDamage(UseLens(from, target, roll), from);
			}
			else if (choice == 1)
			{
				from.ChangeBlock(UseLens(from, null, roll), false);
			}
			else if (choice == 2)
			{
				if (target != null)
					target.ChangeSoul(UseLens(target, null, roll), false);
			}
		}
	}
}
