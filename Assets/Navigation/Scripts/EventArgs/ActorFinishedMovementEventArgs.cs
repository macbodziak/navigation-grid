using System;

namespace Navigation
{
    public class ActorFinishedMovementEventArgs : EventArgs
    {
        public Actor Actor { get; private set; }
        public NavGrid Grid { get; private set; }
        public int GoalIndex { get; private set; }

        public ActorFinishedMovementEventArgs(Actor actor, NavGrid grid, int goalIndex)
        {
            Actor = actor;
            Grid = grid;
            GoalIndex = goalIndex;
        }
    }
}