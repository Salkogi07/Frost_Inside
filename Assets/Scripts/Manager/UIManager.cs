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
    [SerializeField] private Sprite[] staminaFreezeSprite;
    private SteppedFill steppedFill;
    [SerializeField] Image temperatureImage;
    [SerializeField] private Sprite[] temperatureSprites;
    [SerializeField] Image weightImage;
    [Space(10)]
    [SerializeField] Image timeImage;
    [SerializeField] Text timeText;
    [SerializeField] private Sprite[] timeSprites;
    [Space(10)]

    [Header("Freeze Edges")]
    [SerializeField] private Image[] freezeEdges;
    [SerializeField] private float fadeSpeed = 2f;
    private Coroutine damageCoroutine;

    [Header("Player Damage")]
    [SerializeField] private Image damagePanel;
    [SerializeField] private Image damageImage;
    public float fadeDuration = 0.5f;
    
    private float[] targetAlphas;

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

        steppedFill = staminaImage.gameObject.GetComponent<SteppedFill>();
        
        targetAlphas = new float[freezeEdges.Length];
        
        for (int i = 0; i < freezeEdges.Length; i++)
        {
            freezeEdges[i].gameObject.SetActive(true);
            SetImageAlpha(freezeEdges[i], 0f);
            targetAlphas[i] = 0f;
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
        UpdateFreezeEdges();
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

    private void UpdateTime()
    {
        timeText.text = GameManager.instance.hours.ToString("D2") + ":" + GameManager.instance.minutes.ToString("D2");

        if (GameManager.instance.hours < 17)
        {
            timeImage.sprite = timeSprites[0];
        }
        else if (GameManager.instance.hours < 21)
        {
            timeImage.sprite = timeSprites[1];
        }
        else
        {
            timeImage.sprite = timeSprites[2];
        }
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

    public void UpdateHp(float value,  float max)
    {
        float hpValue = value / max;
        hpImage.fillAmount = hpValue;
    }

    public void UpdateStamina(float value,  float max)
    {
        float staminaValue = value / max;
        steppedFill.SetNormalizedValue(staminaValue);
    }
    
    public void UpdateWeight(float value,  float max)
    {
        float weightValue = value / max;
        //무계변경
    }

    public void UpdateTemperature(float value,  float max)
    {
        float temperatureValue = value / max;
        temperatureImage.fillAmount = temperatureValue;
    }

    public void UpdateTemperatureState(float value,  float max)
    {
        float tempRatio =  value / max;
        int tempState;
        
        if (tempRatio >= 0.75f)
        {
            tempState = 0;
            staminaFreezeImage.gameObject.SetActive(false);
            
            targetAlphas[0] = 0f;
            targetAlphas[1] = 0f;
            targetAlphas[2] = 0f;
        }
        else if (tempRatio >= 0.5f)
        {
            tempState = 1;
            staminaFreezeImage.gameObject.SetActive(false);
            
            targetAlphas[0] = 1f;
            targetAlphas[1] = 0f;
            targetAlphas[2] = 0f;
        }
        else if (tempRatio >= 0.25f)
        {
            tempState = 2;
            staminaFreezeImage.sprite = staminaFreezeSprite[0];
            staminaFreezeImage.gameObject.SetActive(true);
            
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 0f;
        }
        else
        {
            tempState = 3;
            staminaFreezeImage.sprite = staminaFreezeSprite[1];
            staminaFreezeImage.gameObject.SetActive(true);
            
            targetAlphas[0] = 1f;
            targetAlphas[1] = 1f;
            targetAlphas[2] = 1f;
        }
        
        temperatureImage.sprite = temperatureSprites[tempState];
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
            float currentAlpha = freezeEdges[i].color.a;
            
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlphas[i], Time.deltaTime * fadeSpeed);
            SetImageAlpha(freezeEdges[i], newAlpha);
        }
    }
}
