using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    [Header("Time GUI")]
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] Slider timeSlider;
    [Space(10)]
    
    private Coroutine damageCoroutine;

    [Header("Player Damage")]
    [SerializeField] private Image damagePanel;
    [SerializeField] private Image damageImage;
    public float fadeDuration = 0.5f;

    [Header("Inventory info")]
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private Image[] quickSlot;
    [SerializeField] private Image pocket;

    [SerializeField] private Image[] itemImage;
    public UI_ItemToolTip itemToolTip;

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
    }

    private void Start()
    {
        //QuickSlotUpdate();
    }

    private void Update()
    {
        //UpdateInventory();
        //UpdateQuickSlot();
        UpdateTime();
    }

    private void UpdateQuickSlot()
    {
        for (int i = 0; i < quickSlot.Length; i++)
        {
            UpdateQuickSlo_View(Inventory.instance.quickSlotItems[i], i);
        }
    }

    public void UpdatePocket()
    {
        pocket.gameObject.SetActive(Inventory.instance.isPocket);
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

    public void UpdateQuickSlo_View(Inventory_Item _newItem, int index)
    {
        itemImage[index].color = Color.white;

        if (_newItem != null && _newItem.data != null)
        {
            itemImage[index].sprite = _newItem.data.icon;
        }
        else
        {
            itemImage[index].sprite = null;
            itemImage[index].color = Color.clear;
        }
    }

    private void UpdateInventory()
    {
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Open Inventory")))
        {
            if (!SettingManager.Instance.IsOpenSetting())
            {
                Inventory.instance.isInvenOpen = !Inventory.instance.isInvenOpen;
                inventoryObject.SetActive(Inventory.instance.isInvenOpen);
            }
        }
    }

    /// <summary>
    /// 게임 플레이 시간 표시
    /// </summary>
    private void UpdateTime()
    {
        timeText.text = GameManager.instance.hours.ToString("D2") + ":" + GameManager.instance.minutes.ToString("D2");

        // 1. 현재 시간을 변수에 저장
        int currentHours = GameManager.instance.hours;
        int currentMinutes = GameManager.instance.minutes;

        // 2. 위에서 만든 함수를 호출하여 0~1 사이의 값을 계산
        float sliderValue = ConvertTimeToSliderValue(currentHours, currentMinutes);

        // 3. 슬라이더에 최종 값 적용
        timeSlider.value = sliderValue;
    }
    
    public float ConvertTimeToSliderValue(int hours, int minutes)
    {
        // 1. 현재 시간을 소수점 형태로 변환 (예: 9시 30분 -> 9.5f)
        float currentTime = hours + (float)minutes / 60.0f;

        // 2. 시간 범위 설정
        float startTime = 8.0f;   // 시작: 오전 8시
        float totalDuration = 16.0f; // 전체 길이: 16시간 (8시부터 24시까지)

        // 3. (현재시간 - 시작시간) / (전체시간) 공식 적용
        float normalizedTime = (currentTime - startTime) / totalDuration;
        
        return Mathf.Clamp01(normalizedTime);
    }
    
    public void ShowDamageEffect()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        
        damageCoroutine = StartCoroutine(PlayerDamageUI());
    }

    private IEnumerator PlayerDamageUI()
    {
        if (damageImage == null)
        {
            Debug.LogWarning("damageImage.");
            yield break;
        }
        
        damageImage.color = new Color(1, 1, 1, 1);

        damageImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            damageImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        damageImage.gameObject.SetActive(false);
        damageCoroutine = null;
    }
    
    private void SetImageAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
