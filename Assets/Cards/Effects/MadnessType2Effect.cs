using Battle.General;
using MessagePack;
using Units.General;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class MadnessType2Effect : DiceRollEffect
	{
		public MadnessType2Effect()
		{
			MinRoll = 1;
			MaxRoll = 6;
		}

		public override void ApplyResult(Unit target, Unit from, int roll)
		{
			if (target != null)
			{
				target.ChangeSoul(UseLens(target, null, roll), false);
			}

			if (roll <= 3)
			{
				from.ChangeSoul(-3, false);
			}
		}
	}
}
