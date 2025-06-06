using System;
using UnityEngine;
using R3;

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

        public ReactiveProperty<float> currentHp { get; private set; }
        public ReactiveProperty<float> currentStamina { get; private set; }
        public ReactiveProperty<float> currentWeight { get; private set; }
        public ReactiveProperty<float> currentTemperature { get; private set; }

        private void Awake()
        {
            stats = GetComponent<Player_Stats>();
        }

        private void Start()
        {
            currentHp = new ReactiveProperty<float>(stats.MaxHp.GetValue());
            currentStamina = new ReactiveProperty<float>(stats.MaxStamina.GetValue());
            currentWeight = new ReactiveProperty<float>(stats.MaxTemperature.GetValue());
            currentTemperature = new ReactiveProperty<float>(stats.MaxWeight.GetValue());
        }
        public bool CheckIsDead() => _isDead;

        private void Update()
        {
            currentHp.Subscribe(hp =>
            {
                UIManager.instance.UpdateHp(currentHp.Value,stats.MaxHp.GetValue());
            });
            
            currentStamina.Subscribe(hp =>
            {
                UIManager.instance.UpdateStamina(currentStamina.Value,stats.MaxStamina.GetValue());
            });
            
            currentWeight.Subscribe(hp =>
            {
                UIManager.instance.UpdateWeight(currentWeight.Value,stats.MaxWeight.GetValue());
            });
            
            currentTemperature.Subscribe(hp =>
            {
                UIManager.instance.UpdateTemperature(currentTemperature.Value,stats.MaxTemperature.GetValue());
                UIManager.instance.UpdateTemperatureState(currentTemperature.Value,stats.MaxTemperature.GetValue());
            });
            
            HandleTemperature();
            HandleFrozenHp();
        }

        public void AddStamina(int value)
        {
            currentStamina.Value += value;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, stats.MaxStamina.GetValue());
        }

        public void ChangeTemperature(float value)
        {
            currentTemperature.Value += value;
            currentTemperature.Value = Mathf.Clamp(currentTemperature.Value, 0f, stats.MaxTemperature.GetValue());
        }
        
        public void TakeDamage(int _damage)
        {
            if(_isDead)
                return;

            currentHp.Value -= _damage;
            if(currentHp.Value <= 0)
            {
                currentHp.Value = 0;
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

            currentTemperature.Value -= dropRate * Time.deltaTime;
            currentTemperature.Value = Mathf.Max(currentTemperature.Value, 0f);
        }

        private void HandleFrozenHp()
        {
            float tempRatio = currentTemperature.Value;

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