using System;

namespace Navigation
{
    public class ActorFinishedMovementEventArgs : EventArgs
    {
        public int GoalIndex { get; private set; }

        public ActorFinishedMovementEventArgs(int goalIndex)
        {
            GoalIndex = goalIndex;
        }
    }
}