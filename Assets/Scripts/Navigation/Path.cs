using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Navigation
{
    public class Path : IEnumerable
    {
        int m_cost;
        List<PathElement> m_elements;


        public IReadOnlyList<PathElement> elements { get => m_elements; }
        public int cost { get => m_cost; private set => m_cost = value; }
        public int Count { get => m_elements.Count; }


        public Path(List<PathElement> elements, int cost)
        {
            m_cost = cost;
            m_elements = elements;
        }


        public PathElement this[int index]
        {
            get
            {
                return m_elements[index];
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public PathEnumerator GetEnumerator()
        {
            return new PathEnumerator(m_elements);
        }
    }

    public class PathEnumerator : IEnumerator
    {
        List<PathElement> m_elements;
        int position = -1;

        public PathEnumerator(List<PathElement> elements)
        {
            m_elements = elements;
        }


        public bool MoveNext()
        {
            position++;
            return (position < m_elements.Count);
        }

        public void Reset()
        {
            position = -1;
        }

        public PathElement Current
        {
            get
            {
                return m_elements[position];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }
    }
}
