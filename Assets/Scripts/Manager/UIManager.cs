using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
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
