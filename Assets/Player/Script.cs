using UnityEngine;
using System.Collections;

public abstract class Script : MonoBehaviour
{
    // returns true if execution should continue
    public abstract bool Execute();
}
