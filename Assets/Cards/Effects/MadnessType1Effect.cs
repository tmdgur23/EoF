using Battle.General;
using MessagePack;
using Units.General;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class MadnessType1Effect : DiceRollEffect
	{
		public MadnessType1Effect()
		{
			MinRoll = 1;
			MaxRoll = 6;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			if (target != null)
			{
				target.ApplyDamage(UseLens(from, target, roll), from);
			}

			if (roll <= 2)
			{
				from.ApplyDamage(3, from);
			}
		}
	}
}
