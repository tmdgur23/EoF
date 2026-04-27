using MessagePack;
using Status.General;
using Units.General;
using UnityEngine;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class NextAttackBonusData : StatusData
	{
		public int Amount;

		public NextAttackBonusData()
		{
			Name = "NextAttackBonus";
			BuffType = Cards.General.BuffType.Buff;
		}

		public override StatusBase Initialize(Unit owner)
		{
			return new NextAttackBonusStatus(this, owner);
		}
	}

	public class NextAttackBonusStatus : StatusBase
	{
		private bool m_isFinished;
		private NextAttackBonusData m_data;
		private int m_currentBonus;

		public override bool IsFinished => m_isFinished;

		public override int Stacks => m_currentBonus;

		public NextAttackBonusStatus(StatusData statusData, Unit unit) : base(statusData, unit)
		{
			m_data = (NextAttackBonusData)statusData;
			m_currentBonus = m_data.Amount;
		}

		public override void Activate()
		{
			AffectedUnit.Might.Current += m_currentBonus;
			AffectedUnit.OnDamageDealt += OnDamageDealt;
		}

		private void OnDamageDealt()
		{
			m_isFinished = true;
			AffectedUnit.StatusContainer.UpdateDuration(); // Force update so it removes itself immediately
		}

		public override void Deactivate()
		{
			AffectedUnit.OnDamageDealt -= OnDamageDealt;
			AffectedUnit.Might.Current -= m_currentBonus;
		}

		public override void AddStacks(int amount)
		{
			AffectedUnit.Might.Current -= m_currentBonus;
			m_currentBonus += amount;
			AffectedUnit.Might.Current += m_currentBonus;
		}

		public override void Update() { }

		public override string ToString() => m_currentBonus.ToString();
	}
}
