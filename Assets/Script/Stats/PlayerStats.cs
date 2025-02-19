using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
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
