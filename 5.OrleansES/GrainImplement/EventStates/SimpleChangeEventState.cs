namespace GrainImplement.EventStates
{
    using EventSourcing.EventModels;
    using GrainInterfaces;


    public class ChangeEventState : CustomEventState<Change, double, double>
    {
        public ChangeEventState()
        {
        }

        protected override double ValueOpertion(double value1, double value2)
        {
            return value1 + value2;
        }
    }
}