using Cards.Effects.General;
using Cards.General;
using MessagePack;
using Units.General;
using UnityEngine;
using Battle;
using Utilities;
using Status.General;

namespace Cards.Effects
{
	public enum DiceRollEffectType
	{
		None,
		Damage,
		Defense,
		DamageAndDefense,
		DamageAndStatus,
		Soul,
		DoubleSoul,
		Conditional,
		Status
	}

	[MessagePackObject(true)]
	public class DiceRollEffect : Effect
	{
		public int MinRoll;
		public int MaxRoll;
		public int BaseAmount; // Optional base amount to add to the roll
		public int Multiplier = 100; // Multiplies the total roll
		public int Repetitions = 1; // Number of times to apply the rolled result
		public DiceRollEffectType RollType;
		public CustomTarget CustomTarget;
		public StatusData TargetStatus;
		
		// Conditional/Complex logic
		public int SuccessThreshold;
		public Effect SuccessEffect;
		public bool UseResultAsValue;

		public override void Execute(Unit target, Unit from)
		{
			var diceSystem = Object.FindObjectOfType<DiceSystem>();
			if (diceSystem != null)
			{
				diceSystem.EnqueueRoll(this, target, from);
			}
		}

		public virtual void ApplyResult(Unit target, Unit from, int roll)
		{
			int mul = Multiplier == 0 ? 100 : Multiplier;
			int finalAmount = Mathf.FloorToInt((roll + BaseAmount) * (mul / 100f));
			int reps = Repetitions <= 0 ? 1 : Repetitions;
			
			for (int i = 0; i < reps; i++)
			{
				if (RollType == DiceRollEffectType.Damage || RollType == DiceRollEffectType.DamageAndDefense)
				{
					if (CustomTarget == CustomTarget.SpecifiedTarget)
					{
						target.ApplyDamage(UseLens(from, target, finalAmount), from);
					}
					else
					{
						from.ApplyDamage(UseLens(from, from, finalAmount), from);
					}
				}
				
				if (RollType == DiceRollEffectType.Defense || RollType == DiceRollEffectType.DamageAndDefense)
				{
					if (CustomTarget == CustomTarget.SpecifiedTarget && RollType == DiceRollEffectType.Defense)
					{
						target.ChangeBlock(UseLens(target, null, finalAmount), false);
					}
					else
					{
						from.ChangeBlock(UseLens(from, null, finalAmount), false);
					}
				}

				if (RollType == DiceRollEffectType.DamageAndStatus && TargetStatus != null)
				{
					if (CustomTarget == CustomTarget.SpecifiedTarget)
					{
						target.ApplyDamage(UseLens(from, target, finalAmount), from);
						var status = TargetStatus.Initialize(target);
						status.AddStacks(Mathf.Max(finalAmount - 1, 0));
						target.StatusContainer.Apply(status);
					}
					else
					{
						from.ApplyDamage(UseLens(from, from, finalAmount), from);
						var status = TargetStatus.Initialize(from);
						status.AddStacks(Mathf.Max(finalAmount - 1, 0));
						from.StatusContainer.Apply(status);
					}
				}

				if (RollType == DiceRollEffectType.Soul)
				{
					if (CustomTarget == CustomTarget.SpecifiedTarget)
					{
						target.ChangeSoul(UseLens(target, null, finalAmount), false);
					}
					else
					{
						from.ChangeSoul(UseLens(from, null, finalAmount), false);
					}
				}

				if (RollType == DiceRollEffectType.Status && TargetStatus != null)
				{
					if (CustomTarget == CustomTarget.SpecifiedTarget)
					{
						var status = TargetStatus.Initialize(target);
						status.AddStacks(Mathf.Max(finalAmount - 1, 0));
						target.StatusContainer.Apply(status);
					}
					else
					{
						var status = TargetStatus.Initialize(from);
						status.AddStacks(Mathf.Max(finalAmount - 1, 0));
						from.StatusContainer.Apply(status);
					}
				}

				if (RollType == DiceRollEffectType.DoubleSoul)
				{
					from.ChangeSoul(UseLens(from, null, finalAmount), false);
					target.ChangeSoul(UseLens(target, null, finalAmount), false);
				}

				if (RollType == DiceRollEffectType.Conditional)
				{
					if (roll >= SuccessThreshold && SuccessEffect != null)
					{
						SuccessEffect.Execute(target, from);
					}
				}
			}
		}

		public override object Value(Unit from, Unit target)
		{
			if (BaseAmount > 0) return $"{BaseAmount} + {MinRoll}~{MaxRoll}";
			return $"{MinRoll}~{MaxRoll}";
		}
	}
}
