using System;

namespace Navigation
{
    public class ActorStartedMovementEventArgs : EventArgs
    {
        public int StartIndex { get; private set; }

        public ActorStartedMovementEventArgs(int startIndex)
        {
            StartIndex = startIndex;
        }
    }
}