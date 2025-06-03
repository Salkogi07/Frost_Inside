using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Stat
{
    [SerializeField] private int baseValue;

    public List<int> Modifiers;

    public int GetValue()
    {
        int finalValue = baseValue;

        foreach (int modifier in Modifiers)
        {
            finalValue += modifier;
        }
        return finalValue;
    }

    public void SetDefaultValue(int _value)
    {
        baseValue = _value;
    }

    public void AddModifier(int _modifier)
    {
        Modifiers.Add(_modifier);
    }

    public void RemoveModifier(int _modifier)
    {
        Modifiers.Remove(_modifier);
    }
}
