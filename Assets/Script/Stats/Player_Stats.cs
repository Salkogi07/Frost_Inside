using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player_Stats : MonoBehaviour
{
    Player_Move player_move;

    [Header("Player info")]
    [SerializeField] public bool isDead = false;

    [Header("Item info")]
    [SerializeField] public Stat mining;
    [SerializeField] public Stat damage;
    [SerializeField] public Stat armor;
    [SerializeField] public Stat lagging;

    [Header("Hp info")]
    [SerializeField] public float hp = 0;
    [SerializeField] public Stat maxHp;

    [Header("Stamina info")]
    [SerializeField] public float stamina = 0;
    [SerializeField] public Stat maxStamina;

    [Header("Weight info")]
    [SerializeField] public float weight = 0;
    [SerializeField] public Stat maxWeight;

    [Header("Temperature info")]
    [SerializeField] public float temperature = 100;
    [SerializeField] public Stat maxTemperature;
    [SerializeField] private float temperatureDropRate = 1f;
    [SerializeField] private int hpDropRate = 2;
    private float damageTimer = 0f;
    private bool isNearHeatSource = false; // 불 근처에 있는지 체크
    private bool isInColdZone = false; // 추운 지역인지 체크

    private void Awake()
    {
        player_move = GetComponent<Player_Move>();
    }

    private void Start()
    {
        hp = maxHp.GetValue();
        stamina = maxStamina.GetValue();
    }

    private void Update()
    {
        float hpValue = hp / maxHp.GetValue();
        float staminaValue = stamina / maxStamina.GetValue();
        float temperatureValue = temperature / maxTemperature.GetValue();
        UIManager.instance.UpdateHp(hpValue);
        UIManager.instance.UpdateStamina(staminaValue);
        UIManager.instance.UpdateTemperatureState(temperatureValue);

        HandleTemperature();
        HandleHp();
    }

    void HandleTemperature()
    {
        float dropRate = temperatureDropRate;

        if (player_move.moveInput != 0)
            dropRate *= 0.5f; // 움직이면 감소율 줄이기

        if (player_move.isSprinting)
            dropRate *= 0.25f; // 뛰고 있으면 온도 감소율 더 낮추기

        if (player_move.isAttack)
            dropRate *= 0.5f; // 공격 중에는 감소율 반으로

        temperature -= dropRate * Time.deltaTime;
        temperature = Mathf.Max(temperature, 0f);
    }

    void HandleHp()
    {
        float tempRatio = temperature;

        if (tempRatio == 0)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 1f)
            {
                TakeDamage(hpDropRate);
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    public void TakeDamage(int _damage)
    {
        if(isDead)
            return;

        hp -= _damage;
        if(hp <= 0)
        {
            hp = 0;
            Die();
            
        }
        else
        {
            UIManager.instance.ShowDamageEffect();
        }
    }

    void Die()
    {
        isDead = true;

        GetComponent<Player_ItemDrop>()?.GenerateDrop();
        Debug.Log("사망했습니다.");
    }

    public int GetMining()
    {
        return mining.GetValue();
    }

    public int GetDamage()
    {
        return damage.GetValue();
    }

    public int GetArmor()
    {
        return armor.GetValue();
    }

    public int GetLagging()
    {
        return lagging.GetValue();
    }
}
