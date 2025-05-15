using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Hp info")]
    [SerializeField] public float hp = 0;
    [SerializeField] public float maxHp = 100;

    [SerializeField] private int damage;
    [SerializeField] private int armor;
    public int difficulty = 1;

    private void Start()
    {
        hp = maxHp;
    }

    public virtual void TakeDamage(int _damage)
    {
        hp -= _damage;

        if (hp < 0)
            Die();
    }

    protected virtual void Die()
    {
        throw new NotImplementedException();
    }
}
