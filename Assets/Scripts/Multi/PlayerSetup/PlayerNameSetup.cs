using TMPro;
using UnityEngine;

public class PlayerNameSetup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    
    public void SetPlayerName(string _playerName)
    {
        playerName.text = _playerName;
    }
}
