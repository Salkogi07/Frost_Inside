using R3;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    // NetworkVariable: 서버에서만 값을 쓰고, 모든 클라이언트가 읽을 수 있도록 설정
    // 게임 시작 후 총 몇 분이 흘렀는지를 동기화하는 것이 시간/분 단위를 따로 하는 것보다 안정적입니다.
    private NetworkVariable<int> networkTotalMinutes = new NetworkVariable<int>(
        8 * 60, // 초기값: 8시 0분 (8 * 60)
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

    private float timeSpeed = 1f; // 1.875초 = 게임 내 1분

    private void Awake()
    {
        if (instance == null)
            instance = this;
        
        if(IsServer)
            gameObject.GetComponent<NetworkObject>().Spawn();
    }

    // NetworkBehaviour가 스폰될 때 호출되는 함수 (Start와 유사)
    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        // NetworkVariable의 값이 변경될 때마다 OnTimeValueChanged 함수를 호출하도록 등록
        networkTotalMinutes.OnValueChanged += OnTimeValueChanged;

        // 초기 UI 업데이트를 위해 현재 값으로 한 번 호출
        UpdateTime(networkTotalMinutes.Value);

        // 서버(호스트)일 경우에만 시간 흐름 코루틴을 시작
        if (IsServer)
        {
            Debug.Log("Is Timer Start");
            StartCoroutine(TimeFlow());
        }
    }

    // NetworkBehaviour가 파괴될 때 호출
    public override void OnNetworkDespawn()
    {
        // 메모리 누수 방지를 위해 이벤트 리스너 해제
        if (networkTotalMinutes != null)
        {
            networkTotalMinutes.OnValueChanged -= OnTimeValueChanged;
        }
    }

    // NetworkVariable의 값이 변경될 때 (서버 -> 클라이언트) 호출되는 콜백 함수
    private void OnTimeValueChanged(int previousValue, int newValue)
    {
        UpdateTime(newValue);
    }
    
    // 총 분(minutes) 값을 시간과 분으로 변환하고 이벤트를 호출하는 함수
    private void UpdateTime(int totalMinutes)
    {
        Hours = (totalMinutes / 60) % 24;
        Minutes = totalMinutes % 60;
        // Debug.Log($"시간 업데이트: {CurrentHours:D2}:{CurrentMinutes:D2}");
    }

    // 이 코루틴은 오직 서버(호스트)에서만 실행됩니다.
    private System.Collections.IEnumerator TimeFlow()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeSpeed);
            
            // 서버에서만 NetworkVariable의 값을 변경.
            // 이 값이 변경되면 모든 클라이언트의 OnTimeValueChanged가 자동으로 호출됩니다.
            networkTotalMinutes.Value++;
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}