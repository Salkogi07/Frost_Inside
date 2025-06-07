using UnityEngine;
using System;

[Serializable]
public class Monster_StatGroup
{
    [Header("int,stat")]
    public Stat attack;
    public Stat Armor;
    [Header("float")] 
    public float Groggy;
    public float Attack_speed;
    public float speed;
}
