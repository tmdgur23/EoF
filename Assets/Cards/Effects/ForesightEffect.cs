using Cards.Effects.General;
using MessagePack;
using Units.General;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class ForesightEffect : Effect
	{
		public override void Execute(Unit target, Unit from)
		{
			// TODO: Reveal enemy's intention, next action, or predict their stats
		}

		public override object Value(Unit from, Unit target)
		{
			return null;
		}
	}
}
