using Status.General;
using Cards.Effects.General;
using MessagePack;
using Units.General;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class ExecuteIfStatus : Effect
	{
		public StatusData TargetStatus;
		public Effect EffectToExecute;

		public override void Execute(Unit target, Unit from)
		{
			if (target != null && TargetStatus != null && target.StatusContainer.Contains(TargetStatus.GetType(), out _))
			{
				EffectToExecute?.Execute(target, from);
			}
		}

		public override object Value(Unit from, Unit target) => EffectToExecute?.Value(from, target) ?? "";
	}
}
