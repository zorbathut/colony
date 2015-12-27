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

        // Adjust based on building size and position
        if (distanceVec.x > 0)
        {
            // Subtract second building width, minus one; this adjusts for large buildings
            distanceVec.x -= two.GetWidth() - 1;
            
            // Clamp to zero
            distanceVec.x = Mathf.Max(distanceVec.x, 0);
        }
        else
        {
            // Invert
            distanceVec.x *= -1;

            // Subtract first building width, minus one; this adjusts for large buildings
            distanceVec.x -= one.GetWidth() - 1;

            // Clamp to zero
            distanceVec.x = Mathf.Max(distanceVec.x, 0);
        }

        if (distanceVec.z > 0)
        {
            // Subtract second building width, minus one; this adjusts for large buildings
            distanceVec.z -= two.GetLength() - 1;

            // Clamp to zero
            distanceVec.z = Mathf.Max(distanceVec.z, 0);
        }
        else
        {
            // Invert
            distanceVec.z *= -1;

            // Subtract first building width, minus one; this adjusts for large buildings
            distanceVec.z -= one.GetLength() - 1;

            // Clamp to zero
            distanceVec.z = Mathf.Max(distanceVec.z, 0);
        }

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
