using System.Collections.Generic;

namespace Navigation
{

    public struct OpenListElement
    {
        public int index;
        public int score;

        public OpenListElement(int index, int score)
        {
            this.index = index;
            this.score = score;
        }
    }


    public struct OpenListComparer : IComparer<OpenListElement>
    {
        public int Compare(OpenListElement x, OpenListElement y)
        {
            if (x.score < y.score)
            {
                return -1;
            }
            if (x.score > y.score)
            {
                return 1;
            }
            return 0;
        }
    }
}