using Unity.Netcode;
using UnityEngine;
using System;

public abstract class Enemy_Health : Entity_Health
{
    [SerializeField] protected Stat maxHealthStat;
    
    // 현재 체력을 모든 클라이언트와 동기화하기 위한 네트워크 변수입니다.
    // 서버만 값을 쓸 수 있고, 모두가 읽을 수 있습니다.
    public NetworkVariable<int> CurrentHealth { get; } = new NetworkVariable<int>();

    // 서버에서만 사용되는 사망 상태 플래그입니다.
    protected bool isDead_Server;

    public override void OnNetworkSpawn()
    {
        // 서버에서만 초기 체력 설정 및 상태 초기화를 진행합니다.
        if (IsServer)
        {
            ResetHealth();
        }

        // 클라이언트에서는 체력 변화 시 VFX를 재생하도록 콜백을 등록할 수 있습니다.
        // 예: CurrentHealth.OnValueChanged += OnHealthChangedClient;
    }

    /// <summary>
    /// (서버 전용) 피해를 받고 체력을 감소시킵니다.
    /// </summary>
    public override void TakeDamage(int damage, Transform damageDealer)
    {
        // 서버가 아니거나, 이미 죽었다면 피해를 받지 않습니다.
        if (!IsServer || isDead_Server) return;

        base.TakeDamage(damage, damageDealer); // 넉백 및 VFX 로직 실행
        
        int newHealth = CurrentHealth.Value - damage;

        if (newHealth <= 0)
        {
            newHealth = 0;
            CurrentHealth.Value = newHealth;
            Die(); // 체력이 0 이하면 사망 처리
        }
        else
        {
            CurrentHealth.Value = newHealth;
        }
    }

    /// <summary>
    /// (서버 전용) 사망 로직을 처리합니다. 자식 클래스에서 구체적인 행동을 정의합니다.
    /// </summary>
    protected virtual void Die()
    {
        isDead_Server = true;
        // 이 메서드는 자식 클래스에서 override하여 상태 머신 변경 등을 처리합니다.
    }

    /// <summary>
    /// (서버 전용) 오브젝트 풀에서 재사용될 때 체력과 상태를 초기화합니다.
    /// </summary>
    public void ResetHealth()
    {
        if (!IsServer) return;

        EntityMaxHealth = maxHealthStat.GetValue();
        CurrentHealth.Value = (int)EntityMaxHealth;
        isDead_Server = false;
    }
}