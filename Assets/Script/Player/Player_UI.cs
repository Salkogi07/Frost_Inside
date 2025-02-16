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
    [SerializeField] Image temperatureImage; // ü�� ���� �̹���
    [SerializeField] Image weightImage;
    [Space(10)]
    [SerializeField] Image timeImage; //�ð� �̹���
    [SerializeField] Text timeText; //�ð� �ؽ�Ʈ
    [SerializeField] private Sprite[] timeSprites; // �ð��� �̹��� (3�� �ʿ�)
    [Space(10)]
    [SerializeField] private Sprite[] temperatureSprites; // ü�� �ܰ躰 �̹��� (4�� �ʿ�)

    [Header("Freeze Edges")]
    [SerializeField] private Image[] freezeEdges; // ���� �׵θ� 3��
    [SerializeField] private float fadeSpeed = 2f; // ���� ��ȭ �ӵ�

    // �� �׵θ��� ��ǥ�� �� ���� �� (0~1 ����)
    private float[] targetAlphas;

    [Header("Inventory info")]
    [SerializeField] private GameObject inventoryObject;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        // freezeEdges ������ŭ targetAlphas �ʱ�ȭ
        targetAlphas = new float[freezeEdges.Length];

        // ���� �ÿ��� ���� ���� 0���� ���� (���� �׵θ� ��ǥ��)
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
            // ���� ����
            float currentAlpha = freezeEdges[i].color.a;
            // ��ǥ ���ķ� ������ ����
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

        // ü�� ������ ���� ���� ����
        if (tempRatio >= 0.75f)
        {
            tempState = 0; // ���� ü��
            staminaFreezeImage.gameObject.SetActive(false);

            // �׵θ� ���� ���� 0
            targetAlphas[0] = 0f;
            targetAlphas[1] = 0f;
            targetAlphas[2] = 0f;
        }
        else if (tempRatio >= 0.5f)
        {
            tempState = 1; // �ణ �߿�
            staminaFreezeImage.gameObject.SetActive(false);

            // ù ��° �׵θ��� ������ ��Ÿ������
            targetAlphas[0] = 1f;
            targetAlphas[1] = 0f;
            targetAlphas[2] = 0f;
        }
        else if (tempRatio >= 0.25f)
        {
            tempState = 2; // ���� �߿�
            staminaFreezeImage.gameObject.SetActive(true);
            staminaFreezeImage.fillAmount = 0.5f;

            // ù ��°, �� ��° �׵θ����� ǥ��
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 0f;
        }
        else
        {
            tempState = 3; // �ص��� �߿�
            staminaFreezeImage.gameObject.SetActive(true);
            staminaFreezeImage.fillAmount = 1f;

            // ��� �׵θ� �� ǥ��
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
