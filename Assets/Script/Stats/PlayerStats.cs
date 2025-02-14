using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Item info")]
    [SerializeField] private Stat mining;
    [SerializeField] private Stat damage;
    [SerializeField] private Stat armor;
    [SerializeField] private Stat lagging;

    [Header("Hp info")]
    [SerializeField] public float hp = 0;
    [SerializeField] public float maxHp = 100;

    [Header("Stamina info")]
    [SerializeField] public float stamina = 0;
    [SerializeField] public float maxStamina = 100;

    [Header("Weight info")]
    [SerializeField] public float weight = 0;
    [SerializeField] public float maxWeight = 100;

    [Header("Temperature info")]
    [SerializeField] public float temperature = 100;
    [SerializeField] public float maxTemperature = 100;

    private void Start()
    {
        hp = maxHp;
        stamina = maxStamina;
    }

    public virtual void TakeDamage(int _damage)
    {
        hp -= _damage;

        if (hp < 0)
            Die();
    }

    protected virtual void Die()
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
