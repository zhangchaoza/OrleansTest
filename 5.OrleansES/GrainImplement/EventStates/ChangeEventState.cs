namespace GrainImplement.EventStates
{
    using System;
    using EventSourcing.EventStates;
    using GrainInterfaces;


    public class SimpleChangeEventState : SimpleEventState<Change>
    {
        public SimpleChangeEventState()
        {
        }

    }
}