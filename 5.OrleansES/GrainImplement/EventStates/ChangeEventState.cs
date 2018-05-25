namespace GrainImplement.EventStates
{
    using System;
    using EventSourcing.EventModels;
    using GrainInterfaces;


    public class SimpleChangeEventState : SimpleEventState<Change, double, double>
    {
        public SimpleChangeEventState()
        {
        }

        protected override double ValueOpertion(double value1, double value2)
        {
            return value1 + value2;
        }
    }
}