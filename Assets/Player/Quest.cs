using UnityEngine;
using System.Collections;

public abstract class Quest : MonoBehaviour
{
    public abstract string GetTextual();
    public abstract bool IsComplete();
}
