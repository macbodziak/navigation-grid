using System;

namespace Navigation
{
    public class ActorStartedMovementEventArgs : EventArgs
    {
        public Actor Actor { get; private set; }
        public NavGrid Grid { get; private set; }
        public int StartIndex { get; private set; }

        public ActorStartedMovementEventArgs(Actor actor, NavGrid grid, int startIndex)
        {
            Actor = actor;
            Grid = grid;
            StartIndex = startIndex;
        }
    }
}