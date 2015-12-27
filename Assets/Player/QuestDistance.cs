using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class QuestDistance : Quest
{
    [SerializeField] Structure m_Structure;
    [SerializeField] List<Structure> m_Targets;

    [SerializeField] int m_Distance;
    enum DistanceComparator
    {
        AtLeast,
        AtMost,
    };
    [SerializeField] DistanceComparator m_Comparator;

    List<Structure> GetActiveTargets()
    {
        List<Structure> output = new List<Structure>();
        Builder builder = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>();

        foreach (Structure structure in m_Targets)
        {
            if (builder.HasStructure(structure))
            {
                output.Add(structure);
            }
        }

        return output;
    }

    string StringizeActiveTargets()
    {
        List<Structure> targets = GetActiveTargets();

        string result = "";
        for (int i = 0; i < targets.Count; ++i)
        {
            if (i > 0 && targets.Count > 2)
            {
                result = result + ", ";
            }
            if (i > 0 && i == targets.Count - 1)
            {
                result = result + " and ";
            }
            result = result + targets[i].name;
        }

        return result;
    }

    public override string GetTextual()
    {
        if (m_Comparator == DistanceComparator.AtMost && m_Distance == 1)
        {
            // Special-case for English
            return string.Format("Place {0} adjacent to {1}", m_Structure.name, StringizeActiveTargets());
        }
        else
        {
            return string.Format("Place {0} {2} {3} tiles away from {1}", m_Structure.name, StringizeActiveTargets(), m_Comparator == DistanceComparator.AtLeast ? "at least" : "at most", m_Distance);
        }
    }

    bool ValidateDistances(Structure lhs, Structure rhs)
    {
        Structure one = Manager.instance.GetStructureOfType(lhs);
        Structure two = Manager.instance.GetStructureOfType(rhs);
        if (one == null || two == null)
        {
            return false;
        }

        // Get difference of position
        IntVector2 distanceVec = (one.GetOrigin() - two.GetOrigin());

        // Absolute distance
        distanceVec.x = Mathf.Abs(distanceVec.x);
        distanceVec.z = Mathf.Abs(distanceVec.z);

        // Subtract building sizes to deal with large buildings; plus two because our default building size is just the size it's occupying
        distanceVec.x = distanceVec.x - one.GetWidth() - two.GetWidth() + 2;
        distanceVec.z = distanceVec.z - one.GetLength() - two.GetLength() + 2;

        // Clamp to zero, dealing with large buildings that overlap on an axis
        distanceVec.x = Mathf.Max(distanceVec.x, 0);
        distanceVec.z = Mathf.Max(distanceVec.z, 0);

        // Calculate manhattan distance
        int distance = distanceVec.x + distanceVec.z;

        if (m_Comparator == DistanceComparator.AtLeast)
        {
            return distance >= m_Distance;
        }
        else if (m_Comparator == DistanceComparator.AtMost)
        {
            return distance <= m_Distance;
        }
        else
        {
            Assert.IsTrue(false);
            // okay sure just go on
            return true;
        }
    }

    public override bool IsComplete()
    {
        List<Structure> targets = GetActiveTargets();

        foreach (Structure target in targets)
        {
            if (!ValidateDistances(m_Structure, target))
            {
                return false;
            }
        }

        return true;
    }
}
