using MessagePack;
using Status.General;
using Units.General;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class BleedingStatusData : StatusData
	{
		public BleedingStatusData()
		{
			Name = "Bleeding";
		}

		public override StatusBase Initialize(Unit owner)
		{
			return new BleedingStatus(this, owner);
		}
	}

	public class BleedingStatus : CounterStatus
	{
		public BleedingStatus(StatusData statusData, Unit unit) : base(statusData, unit)
		{
		}

		public override void Update()
		{
			// 현재 스택(출혈량)만큼 데미지를 줍니다.
			if (Duration > 0)
			{
				AffectedUnit.ApplyDamage(Duration, null);
			}

			// base.Update()가 Duration(스택)을 1 감소시킵니다.
			base.Update();
		}
	}
}
