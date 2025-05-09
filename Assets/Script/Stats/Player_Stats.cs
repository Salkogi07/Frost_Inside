using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player_Stats : MonoBehaviour
{

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

    private void Start()
    {
        hp = maxHp.GetValue();
        stamina = maxStamina.GetValue();
    }

    public int GetMining() => mining.GetValue();
    public int GetDamage() => damage.GetValue();
    public int GetArmor() => armor.GetValue();
    public int GetLagging() => lagging.GetValue();

    public float GetHp() => hp;
    public float GetStamina() => stamina;
    public float GetWeight() => weight;
    public float GetTemperature() => temperature;

    public void AddStamina(int value)
    {
        stamina += value;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina.GetValue());
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
}
