using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    /////////////////////////////////////////////
    // SINGLETON
    //

    static Manager s_Manager = null;
    public static Manager instance
    {
        get
        {
            return s_Manager;
        }
    }

    /////////////////////////////////////////////
    // VARIABLES
    //

    [SerializeField] Transform m_Pillar;
    [SerializeField] Transform m_Wall;
    [SerializeField] Transform m_Door;
    [SerializeField] Transform m_Aqueduct;
    [SerializeField] Transform m_AqueductEndcap;

    [SerializeField] Structure m_AqueductStructure; // the buildable aqueduct; used to tell if we're aqueduct-ready

    // li'l bit of abstraction here
    class SparseIntMatrix<T> where T : Object
    {
        Dictionary<int, Dictionary<int, T>> m_Data = new Dictionary<int, Dictionary<int, T>>();

        public T Lookup(int x, int z)
        {
            if (!m_Data.ContainsKey(x))
            {
                return null;
            }

            if (!m_Data[x].ContainsKey(z))
            {
                return null;
            }

            return m_Data[x][z];
        }

        public T Lookup(IntVector2 position)
        {
            return Lookup(position.x, position.z);
        }

        public void Set(IntVector2 position, T structure)
        {
            Assert.IsNull(Lookup(position));

            if (!m_Data.ContainsKey(position.x))
            {
                m_Data[position.x] = new Dictionary<int, T>();
            }

            m_Data[position.x][position.z] = structure;
        }

        public void Set(int x, int z, T structure)
        {
            Set(new IntVector2(x, z), structure);
        }

        public void Clear(IntVector2 position)
        {
            if (Lookup(position))
            {
                m_Data[position.x][position.z] = null;
            }
            else
            {
                Assert.IsNotNull(Lookup(position));
            }
        }

        public void Clear(int x, int z)
        {
            Clear(new IntVector2(x, z));
        }
    }
    SparseIntMatrix<Structure> m_WorldLookup = new SparseIntMatrix<Structure>();

    List<Structure> m_StructureList = new List<Structure>();
    
    // The various dynamically-built structural elements
    // Each of them is positioned at the top-left of the index, when appropriate; pillars at the top-left corner, walls and doors at either the top edge or the left edge
    // This means if we add a structure at (x,z), with size (width,length), we can rescan all relevant structural elements by going through [(x,z),(x+width+1,z+length+1))
    // Walls and Doors are stored with two different matrices, one for each horizontal and vertical
    // AqueductEndcaps are asymmetrical and include four arrays, one for each cardinal direction
    enum Alignment {
        Horizontal,
        Vertical,
        HorizontalBackwards,
        VerticalBackwards,
    };
    SparseIntMatrix<GameObject> m_Pillars = new SparseIntMatrix<GameObject>();
    SparseIntMatrix<GameObject>[] m_Walls = new SparseIntMatrix<GameObject>[2] { new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>() };
    SparseIntMatrix<GameObject>[] m_Doors = new SparseIntMatrix<GameObject>[2] { new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>() };
    SparseIntMatrix<GameObject>[] m_Aqueducts = new SparseIntMatrix<GameObject>[2] { new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>() };
    SparseIntMatrix<GameObject>[] m_AqueductEndcaps = new SparseIntMatrix<GameObject>[4] { new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>(), new SparseIntMatrix<GameObject>() };

    // Quests
    struct QuestLinkage
    {
        public Quest quest;
        public QuestDisplay display;
    }
    List<QuestLinkage> m_Quests = new List<QuestLinkage>();

    /////////////////////////////////////////////
    // INFRASTRUCTURE
    //

    public virtual void Awake()
    {
        Assert.IsNull(s_Manager);
        s_Manager = this;
    }

    /////////////////////////////////////////////
    // COORDINATES
    //

    public Vector3 ClampToGrid(Vector3 input)
    {
        Vector3 result = new Vector3(Mathf.Round(input.x / Constants.GridSize) * Constants.GridSize, input.y + 10, Mathf.Round(input.z / Constants.GridSize) * Constants.GridSize);

        // Find the y position of the real ground
        RaycastHit hit;
        if (Physics.Raycast(result, Vector3.down, out hit, 20, 1 << Layers.BuildTarget))
        {
            result.y = hit.point.y;
        }
        else
        {
            // well okay, this is going to be weird
            result.y = input.y;
        }

        return result;
    }

    public IntVector2 ClampToIndex(Vector3 input)
    {
        return new IntVector2(Mathf.RoundToInt(input.x / Constants.GridSize), Mathf.RoundToInt(input.z / Constants.GridSize));
    }

    public Vector3 IndexToGrid(IntVector2 input)
    {
        return new Vector3(input.x * Constants.GridSize, 0, input.z * Constants.GridSize);
    }

    /////////////////////////////////////////////
    // STRUCTURE PLACEMENT
    //

    public bool PlaceAttempt(Structure structure, Vector3 position, out string errorMessage)
    {
        errorMessage = null;

        IntVector2 target = ClampToIndex(position);
        IntVector2 playerTarget = ClampToIndex(GameObject.FindGameObjectWithTag(Tags.Player).transform.position);

        for (int x = 0; x < structure.GetWidth(); ++x)
        {
            for (int z = 0; z < structure.GetLength(); ++z)
            {
                if (m_WorldLookup.Lookup(target.x + x, target.z + z))
                {
                    errorMessage = "That building would overlap another building.";
                    return false;
                }

                if (playerTarget.x == target.x + x && playerTarget.z == target.z + z)
                {
                    errorMessage = "Standing in a construction zone is dangerous.";
                    return false;
                }
            }
        }

        Structure newStructure = Instantiate(structure);
        for (int x = 0; x < structure.GetWidth(); ++x)
        {
            for (int z = 0; z < structure.GetLength(); ++z)
            {
                m_WorldLookup.Set(target.x + x, target.z + z, newStructure);
            }
        }
        newStructure.Initialize(structure, target); // initialize after worldlookup set; that way the internal validation code works
        newStructure.transform.position = ClampToGrid(position) + new Vector3((newStructure.GetWidth() - 1) * Constants.GridSize / 2, 0, (newStructure.GetLength() - 1) * Constants.GridSize / 2);

        m_StructureList.Add(newStructure);

        // reprocess our structural elements
        ReprocessStructural(target, newStructure);

        return true;
    }

    // returns removed structure
    public Structure Remove(Vector3 position, out string errorMessage)
    {
        errorMessage = null;

        Structure removalTarget = m_WorldLookup.Lookup(ClampToIndex(position));
        if (!removalTarget)
        {
            errorMessage = "There's nothing to remove.";
            return null;
        }

        IntVector2 target = removalTarget.GetOrigin();

        for (int x = 0; x < removalTarget.GetWidth(); ++x)
        {
            for (int z = 0; z < removalTarget.GetLength(); ++z)
            {
                Assert.IsTrue(m_WorldLookup.Lookup(target.x + x, target.z + z) == removalTarget);
                if (m_WorldLookup.Lookup(target.x + x, target.z + z) == removalTarget)
                {
                    m_WorldLookup.Clear(target.x + x, target.z + z);
                }
            }
        }

        m_StructureList.Remove(removalTarget);

        ReprocessStructural(target, removalTarget);

        Structure template = removalTarget.GetTemplate();

        Destroy(removalTarget.gameObject);

        return template;
    }

    public Structure GetStructureFromCoordinate(Vector3 position)
    {
        return GetStructureFromIndex(ClampToIndex(position));
    }

    public Structure GetStructureFromIndex(IntVector2 position)
    {
        return m_WorldLookup.Lookup(position);
    }

    public Structure GetStructureOfType(Structure structure)
    {
        foreach (Structure testStructure in m_StructureList)
        {
            if (testStructure.GetTemplate() == structure)
            {
                return testStructure;
            }
        }

        return null;
    }

    /////////////////////////////////////////////
    // QUESTS
    //

    public void AddQuest(Quest quest)
    {
        QuestDisplay display = GameObject.FindGameObjectWithTag(Tags.UI).GetComponent<MainUI>().AddQuestDisplay();

        QuestLinkage linkage = new QuestLinkage();
        linkage.quest = quest;
        linkage.display = display;
        m_Quests.Add(linkage);

        EvaluateQuests(); // update graphics; hilariously inefficient, but with ~20 quests at most, doesn't matter
    }

    public void RemoveQuest(Quest quest)
    {
        foreach (QuestLinkage linkage in m_Quests)
        {
            if (linkage.quest == quest)
            {
                GameObject.FindGameObjectWithTag(Tags.UI).GetComponent<MainUI>().RemoveQuestDisplay(linkage.display);
                m_Quests.Remove(linkage);
                break;
            }
        }
    }

    public bool EvaluateQuests()
    {
        bool allComplete = true;

        foreach (QuestLinkage linkage in m_Quests)
        {
            bool complete = linkage.quest.IsComplete();

            allComplete &= complete;

            linkage.display.UpdateDisplay(linkage.quest.GetTextual(), complete);
        }

        return allComplete;
    }

    /////////////////////////////////////////////
    // INTERNAL (STRUCTURAL)
    //

    IntVector2 m_minimumRecalculated = new IntVector2(0, 0);
    IntVector2 m_maximumRecalculated = new IntVector2(0, 0);
    void ReprocessStructural(IntVector2 origin, Structure structure)
    {
        // extra -1 range on both x and y are for the sake of the aqueduct system
        for (int x = origin.x - 1; x < origin.x + structure.GetWidth() + 1; ++x)
        {
            for (int z = origin.z - 1; z < origin.z + structure.GetLength() + 1; ++z)
            {
                RecalculateTile(x, z);
            }
        }
    }

    public void ReprocessAllStructural()
    {
        // Terribly inefficient, but happens rarely
        for (int x = m_minimumRecalculated.x; x <= m_maximumRecalculated.x; ++x)
        {
            for (int z = m_minimumRecalculated.z; z <= m_maximumRecalculated.z; ++z)
            {
                RecalculateTile(x, z);
            }
        }
    }

    void RecalculateTile(int x, int z)
    {
        m_minimumRecalculated.x = Mathf.Min(m_minimumRecalculated.x, x);
        m_minimumRecalculated.z = Mathf.Min(m_minimumRecalculated.z, z);
        m_maximumRecalculated.x = Mathf.Max(m_maximumRecalculated.x, x);
        m_maximumRecalculated.z = Mathf.Max(m_maximumRecalculated.z, z);

        // there is just no clean way to do this
        RecalculatePillar(x, z);
        RecalculateWall(x, z, m_Walls[(int)Alignment.Horizontal], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z - 1), Quaternion.identity);
        RecalculateWall(x, z, m_Walls[(int)Alignment.Vertical], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x - 1, z), Quaternion.AngleAxis(90, Vector3.up));
        RecalculateDoor(x, z, m_Doors[(int)Alignment.Horizontal], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z - 1), Quaternion.identity);
        RecalculateDoor(x, z, m_Doors[(int)Alignment.Vertical], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x - 1, z), Quaternion.AngleAxis(90, Vector3.up));
        RecalculateAqueduct(x, z, m_Aqueducts[(int)Alignment.Horizontal], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z - 1), Quaternion.identity);
        RecalculateAqueduct(x, z, m_Aqueducts[(int)Alignment.Vertical], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x - 1, z), Quaternion.AngleAxis(90, Vector3.up));
        RecalculateAqueductEndcap(x, z, m_AqueductEndcaps[(int)Alignment.Horizontal], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z - 1), Quaternion.identity);
        RecalculateAqueductEndcap(x, z, m_AqueductEndcaps[(int)Alignment.Vertical], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x - 1, z), Quaternion.AngleAxis(90, Vector3.up));
        RecalculateAqueductEndcap(x, z, m_AqueductEndcaps[(int)Alignment.HorizontalBackwards], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x, z + 1), Quaternion.AngleAxis(180, Vector3.up));
        RecalculateAqueductEndcap(x, z, m_AqueductEndcaps[(int)Alignment.VerticalBackwards], m_WorldLookup.Lookup(x, z), m_WorldLookup.Lookup(x + 1, z), Quaternion.AngleAxis(270, Vector3.up));
    }

    void RecalculatePillar(int x, int z)
    {
        // Pillar is aligned at the corner of a grid, so we compare everything "adjacent" to that corner
        IntVector2[] adjacencies = {
            new IntVector2(0, 0),
            new IntVector2(-1, 0),
            new IntVector2(0, -1),
            new IntVector2(-1, -1),
        };

        // There is a pillar if there is at least one surrounding structure with the Walls flag and not all surrounding structures are identical
        bool hasWalls = false;
        bool heterogenous = false;
        Structure reference = m_WorldLookup.Lookup(x, z);
        foreach (IntVector2 delta in adjacencies)
        {
            Structure piece = m_WorldLookup.Lookup(x + delta.x, z + delta.z);
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

    void RecalculateAqueduct(int x, int z, SparseIntMatrix<GameObject> storage, Structure lhs, Structure rhs, Quaternion rotation)
    {
        // An aqueduct exists iff both structures have the WaterRelated flag. It's pretty simple.
        bool hasAqueduct = true;
        hasAqueduct &= lhs && lhs.GetWaterRelated();
        hasAqueduct &= rhs && rhs.GetWaterRelated();

        // this might be a performance concern, worry about it if things get slow
        hasAqueduct &= GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>().HasStructure(m_AqueductStructure);

        SetStructure(storage, x, z, m_Aqueduct, hasAqueduct, rotation);
    }

    void RecalculateAqueductEndcap(int x, int z, SparseIntMatrix<GameObject> storage, Structure lhs, Structure rhs, Quaternion rotation)
    {
        // An aqueduct exists iff the first structure is WaterRelated and the second isn't.
        bool lhsFits = lhs && lhs.GetWaterRelated();
        bool rhsFits = !(rhs && rhs.GetWaterRelated());

        // this might be a performance concern, worry about it if things get slow
        bool hasAqueduct = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>().HasStructure(m_AqueductStructure);

        SetStructure(storage, x, z, m_AqueductEndcap, lhsFits && rhsFits && hasAqueduct, rotation);
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
