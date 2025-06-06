using System;
using UnityEngine;

namespace Stats
{
    public class Player_Condition : MonoBehaviour
    {
        private Player_Stats stats;
        
        [Header("Player info")]
        private bool _isDead = false;
        
        public float temperatureDropRate = 1f;
        [SerializeField] private int frozenHpDropRate = 2;
        private float _frozenDamageTimer = 0f;

        public bool isNearHeatSource = false;
        public bool isInColdZone = false;

        public float currentHp { get; private set; }
        public float currentStamina { get; private set; }
        public float currentWeight { get; private set; }
        public float currentTemperature { get; private set; }

        private void Awake()
        {
            stats = GetComponent<Player_Stats>();
        }

        public bool CheckIsDead() => _isDead;

        private void Update()
        {
            HandleTemperature();
            HandleFrozenHp();
        }

        public void AddStamina(int value)
        {
            currentStamina += value;
            currentStamina = Mathf.Clamp(currentStamina, 0f, stats.MaxStamina.GetValue());
        }

        public void ChangeTemperature(float value)
        {
            currentTemperature += value;
            currentTemperature = Mathf.Clamp(currentTemperature, 0f, stats.MaxTemperature.GetValue());
        }
        
        public void TakeDamage(int _damage)
        {
            if(_isDead)
                return;

            currentHp -= _damage;
            if(currentHp <= 0)
            {
                currentHp = 0;
                Die();
            }
            else
            {
                UIManager.instance.ShowDamageEffect();
            }
        }

        void Die()
        {
            _isDead = true;

            GetComponent<Player_ItemDrop>()?.GenerateDrop();
            Debug.Log("죽음");
        }
        
        private void HandleTemperature()
        {
            float dropRate = temperatureDropRate;

            /*if (playerMove.moveInput != 0)
                dropRate *= 0.5f;

            if (playerMove.isSprinting)
                dropRate *= 0.25f;

            if (playerMove.isAttack)
                dropRate *= 0.5f;*/

            currentTemperature -= dropRate * Time.deltaTime;
            currentTemperature = Mathf.Max(currentTemperature, 0f);
        }

        private void HandleFrozenHp()
        {
            float tempRatio = currentTemperature;

            if (tempRatio <= 0)
            {
                _frozenDamageTimer += Time.deltaTime;

                if (_frozenDamageTimer >= 1f)
                {
                    TakeDamage(frozenHpDropRate);
                    _frozenDamageTimer = 0f;
                }
            }
            else
            {
                _frozenDamageTimer = 0f;
            }
        }
    }
}