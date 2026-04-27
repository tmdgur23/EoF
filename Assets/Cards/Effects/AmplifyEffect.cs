using Cards.Effects.General;
using MessagePack;
using Units.General;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class AmplifyEffect : Effect
	{
		public int Amount; // Amount to enhance the next card or stats

		public override void Execute(Unit target, Unit from)
		{
			// TODO: Implement Amplification logic (buffing card effects, dealing extra damage, etc.)
		}

		public override object Value(Unit from, Unit target)
		{
			return Amount;
		}
	}
}
