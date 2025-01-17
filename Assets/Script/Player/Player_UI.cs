using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    public float invincibilityDuration = 0.5f;

    Player_Move playerMove;
    SpriteRenderer sprite;
    Rigidbody2D rb;

    public bool isInvincible = false;

    [Header("Component")]
    [SerializeField] Image staminaImage;
    [SerializeField] Image temperatureImage; // ü�� ���� �̹���
    [SerializeField] Image timeImage; //�ð� �̹���
    [SerializeField] Text timeText; //�ð� �̹���

    [Header("Time info")]
    [SerializeField] private Sprite[] timeSprites; // �ð��� �̹��� (3�� �ʿ�)

    [Header("Hp info")]
    [SerializeField] private float hp = 0;
    [SerializeField] private float maxHp = 100;

    [Header("Stamina info")]
    [SerializeField] public float stamina = 0;
    [SerializeField] public float maxStamina = 100;

    [Header("Weight info")]
    [SerializeField] private float weight = 0;
    [SerializeField] private float maxWeight = 100;

    [Header("Temperature info")]
    [SerializeField] private float temperature = 100;
    [SerializeField] private float maxTemperature = 100;
    [SerializeField] private Sprite[] temperatureSprites; // ü�� �ܰ躰 �̹��� (4�� �ʿ�)

    [Header("O2 info")]
    [SerializeField] private float O2 = 0;
    [SerializeField] private float maxO2 = 100;


    private void Awake()
    {
        playerMove = GetComponent<Player_Move>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateStamina();
        UpdateTemperatureState();
        UpdateTime();
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

    private void UpdateStamina()
    {
        float StaminaValue = stamina / maxStamina;
        staminaImage.fillAmount = StaminaValue;
    }

    private void UpdateTemperatureState()
    {
        float tempRatio = temperature / maxTemperature;
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

    public void DecreaseTemperature(float value)
    {
        temperature -= value;
        if (temperature < 0)
            temperature = 0;
    }

    public void Damage_HP(int _value)
    {
        if (isInvincible)
            return;
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        sprite.color = new Color(1, 1, 1, 0.5f);

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        sprite.color = new Color(1, 1, 1, 1f);
    }
}
