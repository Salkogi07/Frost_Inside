using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("�ð� ��� ����")]
    [Tooltip("��� ��ۿ� ����� Ű")]
    public KeyCode speedUpKey = KeyCode.F1;

    [Tooltip("���� ��� ��")]
    public float fastTimeScale = 10f;

    [Tooltip("���� �ӵ� ��")]
    public float normalTimeScale = 1f;

    private bool isFast = false;
    private float originalFixedDeltaTime;

    void Awake()
    {
        // �ʱ� fixedDeltaTime ���� (�⺻�� 0.02)
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        if (Input.GetKeyDown(speedUpKey))
        {
            isFast = !isFast;
            // timeScale �� ���� �ֱ� ���� ����
            Time.timeScale = isFast ? fastTimeScale : normalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            Debug.Log($"[TimeController] timeScale = {Time.timeScale}");
        }
    }
}
