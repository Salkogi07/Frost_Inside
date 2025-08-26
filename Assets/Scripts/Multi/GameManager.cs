// GameManager.cs (수정된 버전)

using R3;
using UnityEngine;
using Unity.Netcode;
using System.Collections; // IEnumerator 사용을 위해 추가

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    
    public GamePlayerSpawner gamePlayerSpawner;

    private NetworkVariable<int> networkTotalMinutes = new NetworkVariable<int>(
        8 * 60,
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    
    private ReactiveProperty<int> _hoursPropertiy = new ReactiveProperty<int>(8);
    private ReactiveProperty<int> _minutesPropertiy = new ReactiveProperty<int>(0);
    
    public int Hours
    {
        get => _hoursPropertiy.Value;
        set => _hoursPropertiy.Value = value;
    }
    public int Minutes
    {
        get => _minutesPropertiy.Value;
        set => _minutesPropertiy.Value = value;
    }
    
    private Observable<int> _hoursObservable;
    public Observable<int> HoursObservable => _hoursObservable ??= _hoursPropertiy.AsObservable();
    
    private Observable<int> _minutesObservable;
    public Observable<int> MinutesObservable => _minutesObservable ??= _minutesPropertiy.AsObservable();

    private float timeSpeed = 1f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        
        // IsServer 조건은 NetworkManager가 초기화된 후에 유효하므로 Awake에서 불안정할 수 있습니다.
        // GameManager는 씬에 미리 배치하고 NetworkObject를 붙여두는 것이 더 안정적입니다.
        // 현재 코드도 동작은 하겠지만, Spawn() 호출은 OnNetworkSpawn 이후에 하는 것이 좋습니다.
        if(IsServer)
            gameObject.GetComponent<NetworkObject>().Spawn();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("GameManager OnNetworkSpawn");
        networkTotalMinutes.OnValueChanged += OnTimeValueChanged;
        UpdateTime(networkTotalMinutes.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (networkTotalMinutes != null)
        {
            networkTotalMinutes.OnValueChanged -= OnTimeValueChanged;
        }
    }
    
    public void StartGameTimer()
    {
        if (IsServer)
        {
            Debug.Log("[GameManager] Server commands to start the game timer.");
            StartCoroutine(TimeFlow());
        }
    }

    private void OnTimeValueChanged(int previousValue, int newValue)
    {
        UpdateTime(newValue);
    }
    
    private void UpdateTime(int totalMinutes)
    {
        Hours = (totalMinutes / 60) % 24;
        Minutes = totalMinutes % 60;
    }

    private IEnumerator TimeFlow()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeSpeed);
            networkTotalMinutes.Value++;
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}