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

        private ReactiveProperty<float> currentHpPropertiy;
        public float CurrentHp { get => currentHpPropertiy.Value; set =>  currentHpPropertiy.Value = value; }

        private Observable<float> _currentHpObservable;
        public Observable<float> currentHpObservable => _currentHpObservable ??= currentHpPropertiy.AsObservable();
        
        public ReactiveProperty<float> currentStamina { get; private set; }
        public ReactiveProperty<float> currentWeight { get; private set; }
        public ReactiveProperty<float> currentTemperature { get; private set; }

        private void Awake()
        {
            _stats = GetComponent<Player_Stats>();
            
            currentHpPropertiy = new ReactiveProperty<float>(_stats.MaxHp.Value);
            currentStamina = new ReactiveProperty<float>(_stats.MaxStamina.Value);
            currentWeight = new ReactiveProperty<float>(_stats.MaxTemperature.Value);
            currentTemperature = new ReactiveProperty<float>(_stats.MaxWeight.Value);
        }

        private void Start()
        {
            currentHpPropertiy.Subscribe(hp =>
            {
                UIManager.instance.UpdateHp(currentHpPropertiy.Value,_stats.MaxHp.Value);
            });
            
            currentStamina.Subscribe(stamina =>
            {
                UIManager.instance.UpdateStamina(currentStamina.Value,_stats.MaxStamina.Value);
            });
            
            currentWeight.Subscribe(weight =>
            {
                UIManager.instance.UpdateWeight(currentWeight.Value,_stats.MaxWeight.Value);
            });
            
            currentTemperature.Subscribe(temp =>
            {
                UIManager.instance.UpdateTemperature(currentTemperature.Value,_stats.MaxTemperature.Value);
                UIManager.instance.UpdateTemperatureState(currentTemperature.Value,_stats.MaxTemperature.Value);
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
            float tempValue = currentTemperature.Value / _stats.MaxTemperature.Value;
            if (Temp(tempValue) == 2)
            {
                float targetStamina = _stats.MaxStamina.Value * 0.5f;
                
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
                    currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.Value);
            }
            else
            {
                currentStamina.Value += _stats.StaminaRecoverRate * Time.deltaTime;
                currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.Value);
            }
        }
        
        public void UseStaminaToSprint()
        {
            currentStamina.Value -= _stats.SprintCost * Time.deltaTime;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.Value);
        }
        
        public void UseStaminaToJump()
        {
            currentStamina.Value -= _stats.JumpCost;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.Value);
        }

        public bool CanSprint() => currentStamina.Value > _stats.SprintCost * Time.deltaTime;
        public bool CanJump() => currentStamina.Value > _stats.JumpCost;

        public void AddStamina(int value)
        {
            currentStamina.Value += value;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0f, _stats.MaxStamina.Value);
        }

        public void ChangeTemperature(float value)
        {
            currentTemperature.Value += value;
            currentTemperature.Value = Mathf.Clamp(currentTemperature.Value, 0f, _stats.MaxTemperature.Value);
        }
        
        public void TakeDamage(int _damage)
        {
            if(_isDead)
                return;

            CurrentHp -= _damage;
            if(CurrentHp <= 0)
            {
                CurrentHp = 0;
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