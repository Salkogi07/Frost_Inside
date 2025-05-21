using UnityEngine;

public class Character_UI : MonoBehaviour
{
    [SerializeField] private Transform parent_character;
    private Select_Character_UI[] ui;

    private void Awake()
    {
        ui = parent_character.GetComponentsInChildren<Select_Character_UI>();
    }

    private void Start()
    {
        UpdateCharacter_SelectUI();
    }

    public void UpdateCharacter_SelectUI()
    {
        foreach(Select_Character_UI data in ui)
        {
            data.UpdateUI();
        }
    }
}
