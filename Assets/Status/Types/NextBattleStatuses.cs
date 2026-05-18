using UnityEngine;
using Status.General;
using Units.General;
using Cards.General;

namespace Status.Types
{
    public class RuntimeStatusData : StatusData
    {
        public RuntimeStatusData(string name, string spritePath, string description, BuffType buffType = BuffType.Buff)
        {
            Name = name;
            Icon = Resources.Load<Sprite>(spritePath);
            Description = description;
            BuffType = buffType;
        }

        public override StatusBase Initialize(Unit owner)
        {
            return new RuntimeStatus(this, owner, 1);
        }
    }

    public class RuntimeStatus : CounterStatus
    {
        public RuntimeStatus(StatusData statusData, Unit unit, int initialStacks) : base(statusData, unit)
        {
            Duration = initialStacks;
        }

        public override void Activate()
        {
        }

        public override void Deactivate()
        {
        }
    }
}
