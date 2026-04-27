using Cards.Effects.General;
using MessagePack;
using Status.Types;
using Units.General;
using UnityEngine;

namespace Cards.Effects
{
	[MessagePackObject(true)]
	public class BleedingEffect : Effect
	{
		public int Amount; // Amount of bleeding to apply

		public override void Execute(Unit target, Unit from)
		{
			// Bleeding 상태이상을 생성하고 스택을 적용합니다.
			var status = new BleedingStatusData().Initialize(target);
			int finalAmount = UseLens(from, target, Amount);
			status.AddStacks(Mathf.Max(finalAmount - 1, 0));
			
			target.StatusContainer.Apply(status);
		}

		public override object Value(Unit from, Unit target)
		{
			return UseLens(from, null, Amount);
		}
	}
}
