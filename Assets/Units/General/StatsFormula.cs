using Stats.Types;
using UnityEngine;

namespace Units.General
{
	public static class StatsFormula
	{
		public static int CalculateDamage(float damage, Unit owner, float vulnerabilityMultiplier)
		{
			float baseDamage = CalculateDamage(damage, owner) * (1 + vulnerabilityMultiplier);
			return Mathf.FloorToInt(baseDamage + owner.BonusStrength);
		}

		/// <summary>
		/// Damage based on Strength.
		/// </summary>
		public static float CalculateDamage(float damage, Unit owner)
		{
			return (damage + owner.Might.Current) * (1 + owner.Fatigue);
		}

		public static int CalculateDefense(int amount, Unit unit)
		{
			return Mathf.RoundToInt((amount + unit.Perseverance.Current) * (1 + unit.Decrepitude));
		}

		public static int CalculateSoul(int amount, Unit unit)
		{
			int baseAmount = Mathf.RoundToInt((amount * (1 + unit.SoulMultiplier)));
			if (amount == 0) return 0;

			// 정신력 보너스: 얻을 때 +1, 잃을 때 1 덜 잃음 (baseAmount + BonusMental)
			int result = baseAmount + unit.BonusMental;

			// 잃을 때(amount < 0) 이득이 커서 얻는 것으로 변하면 안됨 (0으로 고정)
			if (amount < 0) return Mathf.Min(0, result);
			return result;
		}

		public static int PurityStacks(this Soul soul, int soulStackThreshold)
		{
			var stacks = RelativeSoulStacks(soul, soulStackThreshold);
			return soul.Current > 0 ? stacks : 0;
		}

		public static int CorruptionStacks(this Soul soul, int soulStackThreshold)
		{
			var stacks = RelativeSoulStacks(soul, soulStackThreshold);
			return soul.Current < 0 ? stacks : 0;
		}

		public static void AddPurityStacks(this Soul soul,
										   int amount,
										   int soulStackThreshold)
		{
			if (soul.Current > 0)
			{
				soul.Current += (soulStackThreshold * amount);
			}
		}

		public static void RemovePurityStacks(this Soul soul,
											  int amount,
											  int soulStackThreshold)
		{
			if (PurityStacks(soul, soulStackThreshold) >= amount)
			{
				soul.Current -= (soulStackThreshold * amount);
			}
		}

		public static void AddCorruptionStacks(this Soul soul,
											   int amount,
											   int soulStackThreshold)
		{
			if (soul.Current < 0)
			{
				soul.Current -= (soulStackThreshold * amount);
			}
		}

		public static void RemoveCorruptionStacks(this Soul soul,
												  int amount,
												  int soulStackThreshold)
		{
			if (CorruptionStacks(soul, soulStackThreshold) >= amount)
			{
				soul.Current += (soulStackThreshold * amount);
			}
		}

		private static int RelativeSoulStacks(Soul soul, int soulStackThreshold)
		{
			var value = Mathf.Abs(soul.Current);
			var stacks = Mathf.FloorToInt(value / soulStackThreshold);
			return stacks;
		}
	}
}