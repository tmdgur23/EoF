using MessagePack;
using Status.General;
using Units.General;
using UnityEngine;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class WeaknessData : StatusData
	{
		public int Percentage = 25;

		public WeaknessData()
		{
			Name = "Weakness";
			BuffType = Cards.General.BuffType.Debuff;
		}

		public override StatusBase Initialize(Unit owner)
		{
			return new WeaknessStatus(this, owner);
		}
	}

	public class WeaknessStatus : CounterStatus
	{
		private WeaknessData m_weaknessData;

		public WeaknessStatus(StatusData statusData, Unit unit) : base(statusData, unit)
		{
			m_weaknessData = (WeaknessData)statusData;
		}

		public override void Activate()
		{
			AffectedUnit.Fatigue += -m_weaknessData.Percentage / 100f;
		}

		public override void Deactivate()
		{
			AffectedUnit.Fatigue -= -m_weaknessData.Percentage / 100f;
		}
	}
}
