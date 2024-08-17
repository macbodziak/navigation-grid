using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Navigation
{
    public class Path
    {
        int m_cost;
        List<PathElement> m_elements;


        public List<PathElement> elements { get => m_elements; private set => m_elements = value; }
        public int cost { get => m_cost; private set => m_cost = value; }
        public int Count { get => m_elements.Count; }


        public Path(List<PathElement> elements, int cost)
        {
            m_cost = cost;
            m_elements = elements;
        }
    }
}
