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
    }

    public virtual void TakeDamage(int _damage)
    {
        hp -= _damage;

        UIManager.instance.ShowDamageEffect();

        if (hp < 0)
            Die();
    }

    protected virtual void Die()
    {
        throw new NotImplementedException();
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
