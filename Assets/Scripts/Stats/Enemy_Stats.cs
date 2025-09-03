using System;
using UnityEngine;
namespace Stats
{
    public class Enemy_Stats : MonoBehaviour
    {

        [Header("Stat info")] 
        [SerializeField] private Stat damage;
        [SerializeField] private float armor;
        [SerializeField] public float Groggy;
        [SerializeField] public float speed;
        
        public int difficulty = 1;

        private void Start()
        {

        }
        

        protected virtual void Die()
        {

        }
    }
}
