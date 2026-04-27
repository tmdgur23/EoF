using MessagePack;
using Status.General;
using Units.General;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class TemporaryMightData : StatusData
	{
		public int Amount;

		public TemporaryMightData()
		{
			Name = "TemporaryMight";
			BuffType = Cards.General.BuffType.Debuff;
		}

		public override StatusBase Initialize(Unit owner)
		{
			return new TemporaryMightStatus(this, owner);
		}
	}

	public class TemporaryMightStatus : CounterStatus
	{
		private TemporaryMightData m_mightData;

		public TemporaryMightStatus(StatusData statusData, Unit unit) : base(statusData, unit)
		{
			m_mightData = (TemporaryMightData)statusData;
		}

		public override void Activate()
		{
			AffectedUnit.Might.Current += m_mightData.Amount;
		}

		public override void Deactivate()
		{
			AffectedUnit.Might.Current -= m_mightData.Amount;
		}
	}
}
