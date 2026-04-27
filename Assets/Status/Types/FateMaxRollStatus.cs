using MessagePack;
using Status.General;
using Units.General;

namespace Status.Types
{
	[MessagePackObject(true)]
	public class FateMaxRollData : StatusData
	{
		public override StatusBase Initialize(Unit owner) => new FateMaxRollStatus(this, owner);
	}

	public class FateMaxRollStatus : StatusBase
	{
		private int _stacks = 1;

		public FateMaxRollStatus(StatusData data, Unit owner) : base(data, owner) { }

		public override bool IsFinished => _stacks <= 0;
		public override int Stacks => _stacks;

		public override void AddStacks(int amount) => _stacks += amount;

		public override void Update() { }

		public override string ToString() => $"{StatusData.Name}";
	}
}
