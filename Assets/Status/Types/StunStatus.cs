using MessagePack;
using Status.General;
using Units.General;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class StunData : StatusData
	{
		public StunData()
		{
			Name = "Stun";
			BuffType = Cards.General.BuffType.Debuff;
		}

		public override StatusBase Initialize(Unit owner)
		{
			return new StunStatus(this, owner);
		}
	}

	public class StunStatus : CounterStatus
	{
		public StunStatus(StatusData statusData, Unit unit) : base(statusData, unit) { }
	}
}
