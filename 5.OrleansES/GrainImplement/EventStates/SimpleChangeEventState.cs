namespace GrainImplement.EventStates
{
    using EventSourcing.EventStates;
    using GrainInterfaces;


    public class ChangeEventState : CustomEventState<Change>
    {
        public ChangeEventState()
        {
        }
    }
}