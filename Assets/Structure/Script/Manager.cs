using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    // li'l bit of abstraction here
    class WorldLookup
    {
        Dictionary<int, Dictionary<int, Structure>> m_Structures = new Dictionary<int, Dictionary<int, Structure>>();

        public Structure Lookup(int x, int y)
        {
            if (!m_Structures.ContainsKey(x))
            {
                return null;
            }
            
            if (!m_Structures[x].ContainsKey(y))
            {
                return null;
            }

            return m_Structures[x][y];
        }

        public void Set(int x, int y, Structure structure)
        {
            Assert.IsNull(Lookup(x, y));

            if (!m_Structures.ContainsKey(x))
            {
                m_Structures[x] = new Dictionary<int, Structure>();
            }

            m_Structures[x][y] = structure;
        }
    }
    WorldLookup m_WorldLookup = new WorldLookup();

    List<Structure> m_StructureList = new List<Structure>();
}
