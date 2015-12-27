using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;

public class QuestConnected : Quest
{
    [SerializeField] Structure m_StructureOne;
    [SerializeField] Structure m_StructureTwo;

    enum ConnectionType
    {
        Ground,
        Water,
    };
    [SerializeField] ConnectionType m_Type = ConnectionType.Ground;

    public override string GetTextual()
    {
        return string.Format("Connect {0} and {1}{2}", m_StructureOne.name, m_StructureTwo.name, m_Type == ConnectionType.Water ? " via aqueducts" : "");
    }

    public override bool IsComplete()
    {
        if (m_Type == ConnectionType.Ground)
        {
            return IsConnected(IsWalkable);
        }
        else if (m_Type == ConnectionType.Water)
        {
            return IsConnected(IsAqueduct);
        }
        else
        {
            Assert.IsTrue(false);
            return true; // most likely to not prevent the game from continuing
        }
    }

    bool IsWalkable(Structure lhs, Structure rhs)
    {
        if (lhs.GetWalled() != rhs.GetWalled())
        {
            // One is walled, the other isn't; this is potentially a problem
            if (!lhs.GetWalled() && !lhs.GetDoorCreator() ||
                !rhs.GetWalled() && !rhs.GetDoorCreator())
            {
                // If either element isn't walled *and* isn't a door creator, then it can't reach the walled element
                return false;
            }
        }

        if (lhs.GetImpassable() || rhs.GetImpassable())
        {
            // we shall not pass
            return false;
        }

        return true;
    }

    bool IsAqueduct(Structure lhs, Structure rhs)
    {
        return lhs.GetWaterRelated() && rhs.GetWaterRelated(); // Aqueduct!
    }

    bool IsConnected(Func<Structure, Structure, bool> comparator)
    {
        Structure oneInstance = Manager.instance.GetStructureOfType(m_StructureOne);
        Structure twoInstance = Manager.instance.GetStructureOfType(m_StructureTwo);

        if (oneInstance == null || twoInstance == null)
        {
            // nope
            return false;
        }

        HashSet<IntVector2> connectedToOne = new HashSet<IntVector2>();
        Queue<IntVector2> processing = new Queue<IntVector2>();

        // seed with the first set
        connectedToOne.Add(oneInstance.GetOrigin());
        processing.Enqueue(oneInstance.GetOrigin());

        while (processing.Count != 0)
        {
            // pick an arbitrary in-process square, remove it from the set
            IntVector2 origin = processing.Dequeue();

            Structure element = Manager.instance.GetStructureFromIndex(origin);
            Assert.IsNotNull(element);
            if (element == null)
            {
                // I guess we don't have an element for some reason? brokes mctotes
                return false;
            }

            // process all adjacencies
            foreach (IntVector2 delta in IntVector2.GetManhattanAdjacencies())
            {
                IntVector2 target = origin + delta;

                if (connectedToOne.Contains(target))
                {
                    // already dealt with, move on
                    continue;
                }

                Structure alterElement = Manager.instance.GetStructureFromIndex(target);

                if (alterElement == null)
                {
                    // no target, abort
                    continue;
                }

                if (!comparator(element, alterElement))
                {
                    // not adjacent, whatever that means in context; abort
                    continue;
                }

                // We're adjacent!

                if (alterElement == twoInstance)
                {
                    // . . . aaand we're done, too!
                    return true;
                }

                // We're not done.
                // :(
                // Add our target to the connected-list for future lookups, and the processing-list so we'll handle it before being done
                connectedToOne.Add(target);
                processing.Enqueue(target);
            }
        }

        // If we got here, we finished the list and still didn't connect the two
        return false;
    }
}
