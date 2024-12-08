
using System;

namespace Navigation
{
    public class ActorExitedNodeEventArgs : EventArgs
    {
        public Actor Actor { get; private set; }
        public NavGrid Grid { get; private set; }
        public int FromIndex { get; private set; }
        public int ToIndex { get; private set; }

        public ActorExitedNodeEventArgs(Actor actor, NavGrid grid, int fromIndex, int toIndex)
        {
            Actor = actor;
            Grid = grid;
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }
    }
}