﻿using UnityEngine;
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

    [SerializeField] Transform m_Pillar;
    [SerializeField] Transform m_Wall;
    [SerializeField] Transform m_Door;

    // li'l bit of abstraction here
    class SparseIntMatrix<T> where T : Object
    {
        Dictionary<int, Dictionary<int, T>> m_Data = new Dictionary<int, Dictionary<int, T>>();

        public T Lookup(int x, int y)
        {
            if (!m_Data.ContainsKey(x))
            {
                return null;
            }

            if (!m_Data[x].ContainsKey(y))
            {
                return null;
            }

            return m_Data[x][y];
        }

        public T Lookup(IntVector2 position)
        {
            return Lookup(position.x, position.y);
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

        public void Set(int x, int z, T structure)
        {
            Set(new IntVector2(x, z), structure);
        }

        public void Clear(IntVector2 position)
        {
            if (Lookup(position))
            {
                m_Data[position.x][position.y] = null;
            }
            else
            {
                Assert.IsNotNull(Lookup(position));
            }
        }
    }
    SparseIntMatrix<Structure> m_WorldLookup = new SparseIntMatrix<Structure>();

    List<Structure> m_StructureList = new List<Structure>();

    
    // The various dynamically-built structural elements
    // Each of them is positioned at the top-left of the index, when appropriate; pillars at the top-left corner, walls and doors at either the top edge or the left edge
    // This means if we add a structure at (x,z), with size (width,length), we can rescan all relevant structural elements by going through [(x,z),(x+width+1,z+length+1))
    // Walls and Doors are stored with two different matrices, one for each horizontal and vertical
    enum Alignment {
        Horizontal,
        Vertical,
    };
    SparseIntMatrix<GameObject> m_Pillars = new SparseIntMatrix<GameObject>();
    SparseIntMatrix<GameObject>[] m_Walls = new SparseIntMatrix<GameObject>[2] { new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>() };
    SparseIntMatrix<GameObject>[] m_Doors = new SparseIntMatrix<GameObject>[2] { new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>() };

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

    public Vector3 IndexToGrid(IntVector2 input)
    {
        return new Vector3(input.x * Constants.GridSize, 0, input.y * Constants.GridSize);
    }

    public void PlaceAttempt(Structure structure, Vector3 position)
    {
        IntVector2 target = ClampToIndex(position);

        for (int x = 0; x < structure.GetWidth(); ++x)
        {
            for (int z = 0; z < structure.GetLength(); ++z)
            {
                if (m_WorldLookup.Lookup(target.x + x, target.y + z))
                {
                    // already full, nope
                    return;
                }
            }
        }

        Structure newStructure = Instantiate(structure);
        newStructure.transform.position = ClampToGrid(position);

        for (int x = 0; x < structure.GetWidth(); ++x)
        {
            for (int z = 0; z < structure.GetLength(); ++z)
            {
                m_WorldLookup.Set(target.x + x, target.y + z, newStructure);
            }
        }

        m_StructureList.Add(newStructure);

        // reprocess our structural elements
        for (int x = target.x; x < target.x + newStructure.GetWidth() + 1; ++x)
        {
            for (int z = target.y; z < target.y + newStructure.GetLength() + 1; ++z)
            {
                // there is just no clean way to do this
                RecalculatePillar(x, z);
                RecalculateWall(x, z, m_Walls[(int)Alignment.Horizontal], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z - 1), Quaternion.identity);
                RecalculateWall(x, z, m_Walls[(int)Alignment.Vertical], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x - 1, z), Quaternion.AngleAxis(90, Vector3.up));
                RecalculateDoor(x, z, m_Doors[(int)Alignment.Horizontal], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z - 1), Quaternion.identity);
                RecalculateDoor(x, z, m_Doors[(int)Alignment.Vertical], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x - 1, z), Quaternion.AngleAxis(90, Vector3.up));
            }
        }
    }
    
    void RecalculatePillar(int x, int z)
    {
        int[] dx = {-1, -1, 0, 0};
        int[] dz = {-1, 0, -1, 0};

        // There is a pillar if there is at least one surrounding structure with the Walls flag and not all surrounding structures are identical
        bool hasWalls = false;
        bool heterogenous = false;
        Structure reference = m_WorldLookup.Lookup(x, z);
        for (int i = 0; i < 4; ++i)
        {
            Structure piece = m_WorldLookup.Lookup(x + dx[i], z + dz[i]);
            hasWalls |= piece && piece.GetWalled();
            heterogenous |= (reference != piece);
        }

        SetStructure(m_Pillars, x, z, m_Pillar, hasWalls && heterogenous, Quaternion.identity);
    }

    void RecalculateWall(int x, int z, SparseIntMatrix<GameObject> storage, Structure lhs, Structure rhs, Quaternion rotation)
    {
        // A wall exists iff at least one of the structures has the Walled flag, at least one of the structures doesn't have the DoorCreator flag, and the two structures differ
        bool hasWalls = false;
        hasWalls |= lhs && lhs.GetWalled();
        hasWalls |= rhs && rhs.GetWalled();

        bool hasNoDoorCreator = false;
        hasNoDoorCreator |= !lhs || !lhs.GetDoorCreator();
        hasNoDoorCreator |= !rhs || !rhs.GetDoorCreator();

        SetStructure(storage, x, z, m_Wall, hasWalls && hasNoDoorCreator && lhs != rhs, rotation);
    }

    void RecalculateDoor(int x, int z, SparseIntMatrix<GameObject> storage, Structure lhs, Structure rhs, Quaternion rotation)
    {
        // A door exists iff at least one of the structures has the Walled flag, both structures have the DoorCreator flag, and the two structures differ
        // This is identical to the Wall test except with the noDoorCreator boolean flipped
        bool hasWalls = false;
        hasWalls |= lhs && lhs.GetWalled();
        hasWalls |= rhs && rhs.GetWalled();

        bool hasNoDoorCreator = false;
        hasNoDoorCreator |= !lhs || !lhs.GetDoorCreator();
        hasNoDoorCreator |= !rhs || !rhs.GetDoorCreator();

        SetStructure(storage, x, z, m_Door, hasWalls && !hasNoDoorCreator && lhs != rhs, rotation);
    }

    void SetStructure(SparseIntMatrix<GameObject> storage, int x, int z, Transform prefab, bool shouldExist, Quaternion rotation)
    {
        if (storage.Lookup(x, z) && !shouldExist)
        {
            // Destroy
            Destroy(storage.Lookup(x, z));
            storage.Clear(new IntVector2(x, z));
        }
        else if (!storage.Lookup(x, z) && shouldExist)
        {
            // Create
            Transform newStructure = Instantiate(prefab);
            newStructure.transform.rotation = rotation;
            newStructure.transform.position = IndexToGrid(new IntVector2(x, z));
            storage.Set(new IntVector2(x, z), newStructure.gameObject);
        }
    }
}
