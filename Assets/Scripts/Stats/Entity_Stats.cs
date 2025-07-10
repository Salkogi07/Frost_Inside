using System;
using UnityEngine;

namespace Stats
{
    public class Entity_Stats : MonoBehaviour
    {
        [SerializeField] protected float health;
        [SerializeField] protected bool isDead = false;
        public Stat MaxHealth;
        public Stat_OffenseGroup Offense;
        public Stat_DefenseGrup Defense;

        public void Start()
        {
            health = MaxHealth.Value;
        }

        public float GetEvasion()
        {
            float baseEvasion = Defense.Evasion.GetValue();
            
            return baseEvasion;
        }
        public virtual void TakeDamage(float damage , Transform damageDealer)
        {
        
            if (isDead)
            {
                return;
            }
            ReduceHp(damage);
        }

        private void ReduceHp(float damage)
        {
            health -= damage;

            if (health < 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            Debug.Log("뻐@거");
        }
    }
}