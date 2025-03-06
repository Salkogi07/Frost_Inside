using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Component")]
    [SerializeField] Image hpImage;
    [SerializeField] Image staminaImage;
    [SerializeField] Image staminaFreezeImage;
    [SerializeField] private Sprite[] staminaFreezeSprite; // 얼음 테두리 3장
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
    private Coroutine damageCoroutine; // 현재 실행 중인 데미지 효과 코루틴 저장 변수

    [Header("Player Damage")]
    [SerializeField] private Image damageImage;
    public float fadeDuration = 0.5f; // 페이드 아웃 지속 시간

    // 각 테두리가 목표로 할 알파 값 (0~1 사이)
    private float[] targetAlphas;

    [Header("Inventory info")]
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private Image[] quickSlot;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        UpdateTime();
        UpdateFreezeEdges();
    }

    public void QuickSlotUpdate()
    {
        for (int i = 0; i < quickSlot.Length; i++)
        {
            if (i == Inventory.instance.selectedQuickSlot)
            {
                SetImageAlpha(quickSlot[i], 1f);
            }
            else
            {
                SetImageAlpha(quickSlot[i], 0.5f);
            }
        }
    }

    private void UpdateInventory()
    {
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Open Inventory")))
        {
            bool isInventory = !inventoryObject.activeSelf;
            inventoryObject.SetActive(isInventory);
        }
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

    public void ShowDamageEffect()
    {
        // 현재 실행 중인 코루틴이 있다면 중지
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        // 새로운 코루틴 실행
        damageCoroutine = StartCoroutine(PlayerDamageUI());
    }

    private IEnumerator PlayerDamageUI()
    {
        if (damageImage == null)
        {
            Debug.LogWarning("damageImage가 설정되지 않았습니다.");
            yield break;
        }

        // 데미지 효과 초기화
        damageImage.color = new Color(1, 1, 1, 1); // 완전 불투명 흰색
        damageImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            damageImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        damageImage.gameObject.SetActive(false); // 투명해지면 비활성화
        damageCoroutine = null; // 코루틴 종료 후 변수 초기화
    }

    public void UpdateHp(float value)
    {
        hpImage.fillAmount = value;
    }

    public void UpdateStamina(float value)
    {
        staminaImage.fillAmount = value;
    }

    public void UpdateTemperatureState(float value)
    {
        float tempRatio = value;
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
            staminaFreezeImage.sprite = staminaFreezeSprite[0];
            staminaFreezeImage.gameObject.SetActive(true);

            // 첫 번째, 두 번째 테두리까지 표시
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 0f;
        }
        else
        {
            tempState = 3; // 극도로 추움
            staminaFreezeImage.sprite = staminaFreezeSprite[1];
            staminaFreezeImage.gameObject.SetActive(true);

            // 모든 테두리 다 표시
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 1f;
        }

        temperatureImage.sprite = temperatureSprites[tempState];
        temperatureImage.fillAmount = tempRatio;
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
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
}
