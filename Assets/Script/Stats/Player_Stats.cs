using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player_Stats : MonoBehaviour
{
    Player_Move player_move;

    [Header("Item info")]
    [SerializeField] private Stat mining;
    [SerializeField] private Stat damage;
    [SerializeField] private Stat armor;
    [SerializeField] private Stat lagging;

    [Header("Hp info")]
    [SerializeField] public float hp = 0;
    [SerializeField] public Stat maxHp;

    [Header("Stamina info")]
    [SerializeField] public float stamina = 0;
    [SerializeField] public Stat maxStamina;

    [Header("Weight info")]
    [SerializeField] public float weight = 0;
    [SerializeField] public Stat maxWeigh;

    [Header("Temperature info")]
    [SerializeField] public float temperature = 100;
    [SerializeField] public Stat maxTemperature;


    [Header("Temperature Info")]
    [SerializeField] private float temperatureDropRate = 1f;
    [SerializeField] private int hpDropRate = 2;
    private float damageTimer = 0f;
    private bool isNearHeatSource = false; // �� ��ó�� �ִ��� üũ
    private bool isInColdZone = false; // �߿� �������� üũ

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
            dropRate *= 0.5f; // �����̸� ������ ���̱�

        if (player_move.isSprinting)
            dropRate *= 0.25f; // �ٰ� ������ �µ� ������ �� ���߱�

        if (player_move.isAttack)
            dropRate *= 0.5f; // ���� �߿��� ������ ������

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

    public virtual void TakeDamage(int _damage)
    {
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

    protected virtual void Die()
    {
        player_move.isDead = true;
        Debug.Log("����߽��ϴ�.");
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
