using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int hours = 8; // 현재 시간 (0 ~ 23)
    public int minutes = 0; // 현재 분 (0 ~ 59)
    private float timeSpeed = 1.875f; // 1.875초 = 게임 내 1분

    public delegate void OnTimeChanged(int hours, int minutes);
    public event OnTimeChanged onTimeChanged; // UI 업데이트 이벤트

    public bool isSetting = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(TimeFlow());
    }

    private IEnumerator TimeFlow()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeSpeed);
            minutes++;

            if (minutes >= 60)
            {
                minutes = 0;
                hours++;

                if (hours >= 24)
                {
                    hours = 0;
                }
            }

            // 이벤트 실행 (UI 갱신)
            onTimeChanged?.Invoke(hours, minutes);
        }
    }
}
