using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Stat
{
    [SerializeField] private int baseValue;

    public int modifier;

    public int GetValue()
    {
        int finalValue = baseValue;

        finalValue += modifier;

        return finalValue;
    }

    public void SetModifier(int _modifier)
    {
        modifier = _modifier;
    }

    public void RemoveModifier()
    {
        modifier = 0;
    }
}
