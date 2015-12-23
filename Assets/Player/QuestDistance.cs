using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class QuestDistance : Quest
{
    [SerializeField] Structure m_StructureOne;
    [SerializeField] Structure m_StructureTwo;
    [SerializeField] int m_Distance;
    enum DistanceComparator
    {
        AtLeast,
        AtMost,
    };
    [SerializeField] DistanceComparator m_Comparator;

    public override string GetTextual()
    {
        if (m_Comparator == DistanceComparator.AtMost && m_Distance == 1)
        {
            // Special-case for English
            return string.Format("Place {0} and {1} adjacent", m_StructureOne.name, m_StructureTwo.name);
        }
        else
        {
            return string.Format("Place {0} and {1} {2} {3} tiles apart", m_StructureOne.name, m_StructureTwo.name, m_Comparator == DistanceComparator.AtLeast ? "at least" : "at most", m_Distance);
        }
    }

    public override bool IsComplete()
    {
        Structure one = Manager.instance.GetStructureOfType(m_StructureOne);
        Structure two = Manager.instance.GetStructureOfType(m_StructureTwo);
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
}
