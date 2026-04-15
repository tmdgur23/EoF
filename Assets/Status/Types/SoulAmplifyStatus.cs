using MessagePack;
using Status.General;
using Units.General;
using UnityEngine;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class SoulAmplifyData : StatusData
	{
		public SoulAmplifyData()
		{
			Name = "SoulAmplify";
			BuffType = Cards.General.BuffType.Debuff;
		}

		public override StatusBase Initialize(Unit owner)
		{
			return new SoulAmplifyStatus(this, owner);
		}
	}

	public class SoulAmplifyStatus : StatusBase
	{
		private bool m_isFinished;
		public override bool IsFinished => m_isFinished;

		public override int Stacks => 1;

		public SoulAmplifyStatus(StatusData statusData, Unit unit) : base(statusData, unit) { }

		public override void Activate()
		{
			AffectedUnit.SoulMultiplier += 1f;
			AffectedUnit.OnSoulChanged += OnSoulChanged;
		}

		private void OnSoulChanged()
		{
			m_isFinished = true;
		}

		public override void Deactivate()
		{
			AffectedUnit.OnSoulChanged -= OnSoulChanged;
			AffectedUnit.SoulMultiplier -= 1f;
		}

		public override void AddStacks(int amount) { }

		public override void Update() { }

		public override string ToString() => "x2";
	}
}
