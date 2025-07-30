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
        
    }
}