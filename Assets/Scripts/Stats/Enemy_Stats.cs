using System;
using UnityEngine;

public class Enemy_Stats : MonoBehaviour
{
    [Header("Hp info")]
    [SerializeField] public float maxHp = 100;

    [Header("Hp info")]
    [SerializeField] private int damage;
    [SerializeField] private int armor;
    [SerializeField] public float Groggy;
    public int difficulty = 1;

    private void Start()
    {
        
    }

    // public virtual void TakeDamage(int _damage)
    // {
    //     hp -= _damage;
    //
    //     if (hp < 0)
    //         Die();
    // }

    protected virtual void Die()
    {
        
    }
}
