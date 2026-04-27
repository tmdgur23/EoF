using MessagePack;
using Status.General;
using Units.General;
using Units.Player.General;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class EnergizedStatus : CounterStatus
	{
		public EnergizedStatus(StatusData statusData, Unit unit) : base(statusData, unit) { }

		public override void Update()
		{
			if (AffectedUnit is Player player)
			{
				player.Energy.Current += Stacks;
			}
			Duration = 0; // Consume all stacks completely
		}
	}

	[MessagePackObject(true)]
	public class EnergizedData : StatusData
	{
		public EnergizedData()
		{
			Name = "Energized";
		}
		public override StatusBase Initialize(Unit owner) => new EnergizedStatus(this, owner);
	}
}
