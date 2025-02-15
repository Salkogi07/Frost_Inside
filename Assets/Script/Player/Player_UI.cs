using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    PlayerStats stats;

    [Header("Component")]
    [SerializeField] Image hpImage;
    [SerializeField] Image staminaImage;
    [SerializeField] Image staminaFreezeImage;
    [SerializeField] Image temperatureImage; // 체온 상태 이미지
    [SerializeField] Image weightImage;
    [Space(10)]
    [SerializeField] Image timeImage; //시간 이미지
    [SerializeField] Text timeText; //시간 텍스트
    [SerializeField] private Sprite[] timeSprites; // 시간별 이미지 (3개 필요)
    [Space(10)]
    [SerializeField] private Sprite[] temperatureSprites; // 체온 단계별 이미지 (4개 필요)

    [Header("Freeze Edges")]
    [SerializeField] private Image[] freezeEdges; // 얼음 테두리 3장
    [SerializeField] private float fadeSpeed = 2f; // 알파 변화 속도

    // 각 테두리가 목표로 할 알파 값 (0~1 사이)
    private float[] targetAlphas;

    [Header("Inventory info")]
    [SerializeField] private GameObject inventoryObject;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        // freezeEdges 개수만큼 targetAlphas 초기화
        targetAlphas = new float[freezeEdges.Length];

        // 시작 시에는 전부 알파 0으로 세팅 (얼음 테두리 미표시)
        for (int i = 0; i < freezeEdges.Length; i++)
        {
            freezeEdges[i].gameObject.SetActive(true);
            SetImageAlpha(freezeEdges[i], 0f);
            targetAlphas[i] = 0f;
        }
    }

    private void Update()
    {
        UpdateInventory();
        UpdateHp();
        UpdateStamina();
        UpdateTemperatureState();
        UpdateTime();
        UpdateFreezeEdges();
    }

    private void UpdateFreezeEdges()
    {
        for (int i = 0; i < freezeEdges.Length; i++)
        {
            // 현재 알파
            float currentAlpha = freezeEdges[i].color.a;
            // 목표 알파로 서서히 보간
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlphas[i], Time.deltaTime * fadeSpeed);
            SetImageAlpha(freezeEdges[i], newAlpha);
        }
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }


    private void UpdateInventory()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            bool isInventory = !inventoryObject.activeSelf;
            inventoryObject.SetActive(isInventory);
        }
    }

    private void UpdateHp()
    {
        hpImage.fillAmount = stats.GetHp();
    }

    private void UpdateStamina()
    {
        staminaImage.fillAmount = stats.GetStamina();
    }

    private void UpdateTemperatureState()
    {
        float tempRatio = stats.GetTemperature();
        int tempState = 0;

        // 체온 구간에 따라 상태 구분
        if (tempRatio >= 0.75f)
        {
            tempState = 0; // 정상 체온
            staminaFreezeImage.gameObject.SetActive(false);

            // 테두리 전부 알파 0
            targetAlphas[0] = 0f;
            targetAlphas[1] = 0f;
            targetAlphas[2] = 0f;
        }
        else if (tempRatio >= 0.5f)
        {
            tempState = 1; // 약간 추움
            staminaFreezeImage.gameObject.SetActive(false);

            // 첫 번째 테두리만 서서히 나타나도록
            targetAlphas[0] = 1f;
            targetAlphas[1] = 0f;
            targetAlphas[2] = 0f;
        }
        else if (tempRatio >= 0.25f)
        {
            tempState = 2; // 많이 추움
            staminaFreezeImage.gameObject.SetActive(true);
            staminaFreezeImage.fillAmount = 0.5f;

            // 첫 번째, 두 번째 테두리까지 표시
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 0f;
        }
        else
        {
            tempState = 3; // 극도로 추움
            staminaFreezeImage.gameObject.SetActive(true);
            staminaFreezeImage.fillAmount = 1f;

            // 모든 테두리 다 표시
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 1f;
        }

        temperatureImage.sprite = temperatureSprites[tempState];
        temperatureImage.fillAmount = tempRatio;
    }


    private void UpdateTime()
    {
        timeText.text = GameManager.instance.hours.ToString("D2") + ":" + GameManager.instance.minutes.ToString("D2");

        if (GameManager.instance.hours < 17)  // 17시 전까지
        {
            timeImage.sprite = timeSprites[0];
        }
        else if (GameManager.instance.hours < 21)  // 21시 전까지
        {
            timeImage.sprite = timeSprites[1];
        }
        else // 21시 이후 (22시 포함)
        {
            timeImage.sprite = timeSprites[2];
        }

    }
}
