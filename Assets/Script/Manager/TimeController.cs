using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("시간 배속 설정")]
    [Tooltip("배속 토글에 사용할 키")]
    public KeyCode speedUpKey = KeyCode.F1;

    [Tooltip("빠른 배속 값")]
    public float fastTimeScale = 10f;

    [Tooltip("정상 속도 값")]
    public float normalTimeScale = 1f;

    private bool isFast = false;
    private float originalFixedDeltaTime;

    void Awake()
    {
        // 초기 fixedDeltaTime 저장 (기본값 0.02)
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        if (Input.GetKeyDown(speedUpKey))
        {
            isFast = !isFast;
            // timeScale 및 물리 주기 동시 변경
            Time.timeScale = isFast ? fastTimeScale : normalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            Debug.Log($"[TimeController] timeScale = {Time.timeScale}");
        }
    }
}
