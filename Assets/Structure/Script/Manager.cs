using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    // singleton
    static Manager s_Manager = null;
    public static Manager instance
    {
        get
        {
            return s_Manager;
        }
    }


    // li'l bit of abstraction here
    class SparseIntMatrix<T> where T : MonoBehaviour
    {
        Dictionary<int, Dictionary<int, T>> m_Data = new Dictionary<int, Dictionary<int, T>>();

        public T Lookup(IntVector2 position)
        {
            if (!m_Data.ContainsKey(position.x))
            {
                return null;
            }

            if (!m_Data[position.x].ContainsKey(position.y))
            {
                return null;
            }

            return m_Data[position.x][position.y];
        }

        public void Set(IntVector2 position, T structure)
        {
            Assert.IsNull(Lookup(position));

            if (!m_Data.ContainsKey(position.x))
            {
                m_Data[position.x] = new Dictionary<int, T>();
            }

            m_Data[position.x][position.y] = structure;
        }
    }
    SparseIntMatrix<Structure> m_WorldLookup = new SparseIntMatrix<Structure>();

    List<Structure> m_StructureList = new List<Structure>();

    public virtual void Awake()
    {
        Assert.IsNull(s_Manager);
        s_Manager = this;
    }

    public Vector3 ClampToGrid(Vector3 input)
    {
        return new Vector3(Mathf.Round(input.x / Constants.GridSize) * Constants.GridSize, input.y, Mathf.Round(input.z / Constants.GridSize) * Constants.GridSize);
    }

    public IntVector2 ClampToIndex(Vector3 input)
    {
        return new IntVector2(Mathf.RoundToInt(input.x / Constants.GridSize), Mathf.RoundToInt(input.z / Constants.GridSize));
    }

    public void PlaceAttempt(Structure structure, Vector3 position)
    {
        IntVector2 target = ClampToIndex(position);
        if (m_WorldLookup.Lookup(target))
        {
            // already full, nope
            return;
        }

        Structure newStructure = Instantiate(structure);
        newStructure.transform.position = ClampToGrid(position);

        m_WorldLookup.Set(target, newStructure);
        m_StructureList.Add(newStructure);
    }
}
