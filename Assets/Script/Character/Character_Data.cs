using UnityEngine;

public enum Character
{
    Doctor,
    Engineer,
    Explorer,
    Miner
}

[CreateAssetMenu(fileName = "new Character", menuName = "Data/Character")]
public class Character_Data : ScriptableObject
{
    public Character character;
    public Sprite selectSprite;
    public Sprite illustrationSprite;

    [Header("Character Prefab")]
    public GameObject characterPrefab;
}
