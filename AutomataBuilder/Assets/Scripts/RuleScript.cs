using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleScript : MonoBehaviour
{
    public int ruleIndex;

    public void DeleteRule()
    {
        GameObject.FindGameObjectWithTag("Tape").GetComponent<TapeScript>().RemoveRule(ruleIndex);
    }
}
