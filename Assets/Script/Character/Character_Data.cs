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
    public Sprite selectImage;
    public Sprite illustrationImage;
    public GameObject characterPrefab;
}
