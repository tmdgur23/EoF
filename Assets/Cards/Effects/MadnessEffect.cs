using Cards.Effects.General;
using MessagePack;
using Units.General;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class MadnessEffect : Effect
	{
		public override void Execute(Unit target, Unit from)
		{
			// TODO: Implement High Risk/High Reward logic (e.g., self damage for massive output, or random extreme effects)
		}

		public override object Value(Unit from, Unit target)
		{
			return null;
		}
	}
}
