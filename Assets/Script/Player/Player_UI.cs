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
    [SerializeField] Image temperatureImage; // 체온 상태 이미지
    [SerializeField] Image weightImage;
    [Space(10)]
    [SerializeField] Image timeImage; //시간 이미지
    [SerializeField] Text timeText; //시간 텍스트
    [SerializeField] private Sprite[] timeSprites; // 시간별 이미지 (3개 필요)
    [Space(10)]
    [SerializeField] private Sprite[] temperatureSprites; // 체온 단계별 이미지 (4개 필요)

    [Header("Inventory info")]
    [SerializeField] private GameObject inventoryObject;


    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        UpdateInventory();
        UpdateHp();
        UpdateStamina();
        UpdateTemperatureState();
        UpdateTime();
    }
    private void UpdateInventory()
    {
        if(Input.GetKeyDown(KeyCode.E))
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

        if (tempRatio >= 0.75f)
        {
            tempState = 0; // 정상 체온
        }
        else if (tempRatio >= 0.5f)
        {
            tempState = 1; // 약간 추움
        }
        else if (tempRatio >= 0.25f)
        {
            tempState = 2; // 많이 추움
        }
        else
        {
            tempState = 3; // 극도로 추움
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
