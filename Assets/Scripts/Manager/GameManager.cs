using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int hours = 8; // ���� �ð� (0 ~ 23)
    public int minutes = 0; // ���� �� (0 ~ 59)
    private float timeSpeed = 1.875f; // 1.875�� = ���� �� 1��

    public delegate void OnTimeChanged(int hours, int minutes);
    public event OnTimeChanged onTimeChanged; // UI ������Ʈ �̺�Ʈ

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

            // �̺�Ʈ ���� (UI ����)
            onTimeChanged?.Invoke(hours, minutes);
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
