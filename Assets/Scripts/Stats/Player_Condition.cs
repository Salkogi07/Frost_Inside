﻿using System;
using UnityEngine;
using R3;
using Script.Plyayer_22;

namespace Stats
{
    public class Player_Condition : MonoBehaviour
    {
        private Player_Stats _stats;
        private Player _player;
        
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
            _stats = GetComponent<Player_Stats>();
        }

        private void Start()
        {
            currentHp = new ReactiveProperty<float>(_stats.MaxHp.GetValue());
            currentStamina = new ReactiveProperty<float>(_stats.MaxStamina.GetValue());
            currentWeight = new ReactiveProperty<float>(_stats.MaxTemperature.GetValue());
            currentTemperature = new ReactiveProperty<float>(_stats.MaxWeight.GetValue());
            
            currentHp.Subscribe(hp =>
            {
                UIManager.instance.UpdateHp(currentHp.Value,_stats.MaxHp.GetValue());
            });
            
            currentStamina.Subscribe(hp =>
            {
                UIManager.instance.UpdateStamina(currentStamina.Value,_stats.MaxStamina.GetValue());
            });
            
            currentWeight.Subscribe(hp =>
            {
                UIManager.instance.UpdateWeight(currentWeight.Value,_stats.MaxWeight.GetValue());
            });
            
            currentTemperature.Subscribe(hp =>
            {
                UIManager.instance.UpdateTemperature(currentTemperature.Value,_stats.MaxTemperature.GetValue());
                UIManager.instance.UpdateTemperatureState(currentTemperature.Value,_stats.MaxTemperature.GetValue());
            });
        }
        
        public bool CheckIsDead() => _isDead;

        private void Update()
        {
            HandleTemperature();
            HandleFrozenHp();
        }

        public void StaminaRecovery()
        {
            float tempValue = currentTemperature.Value / _stats.MaxTemperature.GetValue();
            
            if (Temp(tempValue) == 2)
            {
                float targetStamina = _stats.MaxStamina.GetValue() * 0.5f;
                
                if (currentStamina.Value > targetStamina)
                    currentStamina.Value -= _stats.staminaDecreaseRate * Time.deltaTime;
                else
                {
                    currentStamina.Value += _stats.StaminaRecoverRate * Time.deltaTime;
                    currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, targetStamina);
                }
            }
            else if (Temp(tempValue) == 3)
            {
                if (currentStamina.Value >= 0)
                    currentStamina.Value -= _stats.staminaDecreaseRate * Time.deltaTime;
                else
                    currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.GetValue());
            }
            else
            {
                currentStamina.Value += _stats.StaminaRecoverRate * Time.deltaTime;
                currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.GetValue());
            }
        }
        
        public void UseStaminaToSprint()
        {
            currentStamina.Value -= _stats.SprintCost * Time.deltaTime;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.GetValue());
        }
        
        public void UseStaminaToJump()
        {
            currentStamina.Value -= _stats.JumpCost;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.GetValue());
        }

        public bool CanSprint() => currentStamina.Value > _stats.SprintCost * Time.deltaTime;
        public bool CanJump() => currentStamina.Value > _stats.JumpCost;

        public void AddStamina(int value)
        {
            currentStamina.Value += value;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.GetValue());
        }

        public void ChangeTemperature(float value)
        {
            currentTemperature.Value += value;
            currentTemperature.Value = Mathf.Clamp(currentTemperature.Value, 0f, _stats.MaxTemperature.GetValue());
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
        
        private int Temp(float tempRatio)
        {
            int tempState;
            if (tempRatio >= 0.75f)
            {
                tempState = 0;
            }
            else if (tempRatio >= 0.5f)
            {
                tempState = 1;
            }
            else if (tempRatio >= 0.25f)
            {
                tempState = 2;
            }
            else
            {
                tempState = 3;
            }

            return tempState;
        }
    }
}