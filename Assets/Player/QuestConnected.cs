using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class QuestConnected : Quest
{
    [SerializeField] Structure m_StructureOne;
    [SerializeField] Structure m_StructureTwo;

    public override string GetTextual()
    {
        return string.Format("Connect {0} and {1}", m_StructureOne.name, m_StructureTwo.name);
    }

    public override bool IsComplete()
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

                if (alterElement.GetWalled() != element.GetWalled())
                {
                    // One is walled, the other isn't; this is potentially a problem
                    if (!element.GetWalled() && !element.GetDoorCreator() ||
                        !alterElement.GetWalled() && !alterElement.GetDoorCreator())
                    {
                        // If either element isn't walled *and* isn't a door creator, then it can't reach the walled element
                        continue;
                    }
                }

                if (element.GetImpassable() || alterElement.GetImpassable())
                {
                    // we shall not pass
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
