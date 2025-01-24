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
    [SerializeField] Image temperatureImage; // ü�� ���� �̹���
    [SerializeField] Image weightImage;
    [Space(10)]
    [SerializeField] Image timeImage; //�ð� �̹���
    [SerializeField] Text timeText; //�ð� �ؽ�Ʈ
    [SerializeField] private Sprite[] timeSprites; // �ð��� �̹��� (3�� �ʿ�)
    [Space(10)]
    [SerializeField] private Sprite[] temperatureSprites; // ü�� �ܰ躰 �̹��� (4�� �ʿ�)

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
            tempState = 0; // ���� ü��
        }
        else if (tempRatio >= 0.5f)
        {
            tempState = 1; // �ణ �߿�
        }
        else if (tempRatio >= 0.25f)
        {
            tempState = 2; // ���� �߿�
        }
        else
        {
            tempState = 3; // �ص��� �߿�
        }

        temperatureImage.sprite = temperatureSprites[tempState];
        temperatureImage.fillAmount = tempRatio;
    }


    private void UpdateTime()
    {
        timeText.text = GameManager.instance.hours.ToString("D2") + ":" + GameManager.instance.minutes.ToString("D2");

        if (GameManager.instance.hours < 17)  // 17�� ������
        {
            timeImage.sprite = timeSprites[0];
        }
        else if (GameManager.instance.hours < 21)  // 21�� ������
        {
            timeImage.sprite = timeSprites[1];
        }
        else // 21�� ���� (22�� ����)
        {
            timeImage.sprite = timeSprites[2];
        }

    }
}
