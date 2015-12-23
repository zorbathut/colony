using UnityEngine;
using System.Collections;

public struct IntVector2
{
    public int x;
    public int z;

    public IntVector2(int in_x, int in_z)
    {
        x = in_x;
        z = in_z;
    }

    public static IntVector2 operator-(IntVector2 lhs, IntVector2 rhs)
    {
        return new IntVector2(lhs.x - rhs.x, lhs.z - rhs.z);
    }
}
