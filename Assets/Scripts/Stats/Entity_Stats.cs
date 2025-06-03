using UnityEngine;

namespace Stats
{
    public class Entity_Stats : MonoBehaviour
    {
        public Stat MaxHealth;
        public Stat_OffenseGroup Offense;
        public Stat_DefenseGrup Defense;

        public float GetEvasion()
        {
            float baseEvasion = Defense.Evasion.GetValue();
            
            return baseEvasion;
        }
    }
}