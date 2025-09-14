using System;
using Stats;
using UnityEngine;

public class Enemy_Bomb_Monkey_Health : Enemy_Health
{
    private Enemy_Stats stats;
    public float currentHealth;

    protected override void Awake()
    {
        base.Awake();
        stats = GetComponent<Enemy_Stats>();
    }

    public override void TakeDamage(int damage, Transform damageDealer)
    {
        if (!IsServer) return;

        // Bomb Monkey의 최대 체력을 기준으로 넉백 계산
        EntityMaxHealth = stats.maxHealth.GetValue();

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // 이미 자폭 로직이 상태 머신에 있으므로, 체력으로 죽는 경우는 여기서 처리
            GetComponent<Enemy_Bomb_Monkey>().Deading();
        }

        base.TakeDamage(damage, damageDealer);
    }
}