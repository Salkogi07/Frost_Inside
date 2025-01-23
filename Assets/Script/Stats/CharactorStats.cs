using System;
using UnityEngine;

public class CharactorStats : MonoBehaviour
{
    [Header("Item info")]
    [SerializeField] private Stat mining;
    [SerializeField] private Stat damage;
    [SerializeField] private Stat armor;
    [SerializeField] private Stat lagging;

    [Header("Hp info")]
    [SerializeField] private float hp = 0;
    [SerializeField] private float maxHp = 100;

    [Header("Stamina info")]
    [SerializeField] public float stamina = 0;
    [SerializeField] public float maxStamina = 100;

    [Header("Weight info")]
    [SerializeField] private float weight = 0;
    [SerializeField] private float maxWeight = 100;

    [Header("Temperature info")]
    [SerializeField] private float temperature = 100;
    [SerializeField] private float maxTemperature = 100;

    private void Start()
    {
        hp = maxHp;
        stamina = maxStamina;
    }
    
    public void TakeDamage(int _damage)
    {
        hp -= _damage;

        if (hp < 0)
            Die();
    }

    private void Die()
    {
        throw new NotImplementedException();
    }

    public float GetHp()
    {
        float hpValue = hp / maxHp;
        return hpValue;
    }

    public float GetStamina()
    {
        float staminaValue = stamina / maxStamina;
        return staminaValue;
    }

    public float GetTemperature()
    {
        float temperatureValue = temperature / maxTemperature;
        return temperatureValue;
    }

    public Stat GetMining()
    {
        return mining;
    }

    public Stat GetDamage()
    {
        return damage;
    }

    public Stat GetArmor()
    {
        return armor;
    }

    public Stat GetLagging()
    {
        return lagging;
    }
}
