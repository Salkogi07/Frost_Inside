using UnityEngine;

public class PlayerUISetupTest : MonoBehaviour
{
    private GameObject _precenters;

    public void Awake()
    {
        _precenters = GameObject.FindGameObjectWithTag("PlayerPrecenters");
        if (_precenters != null)
        {
            SettingPlayerUI(gameObject);
        }
    }
    
    private void SettingPlayerUI(GameObject player)
    {
        Player_Condition playerCondition = player.GetComponent<Player_Condition>();
        _precenters.GetComponent<PlayerHpUIPrecenter>().SetPlayerModel(playerCondition);
        _precenters.GetComponent<PlayerStaminaUIPresenter>().SetPlayerModel(playerCondition);
        _precenters.GetComponent<PlayerTemperatureUIPresenter>().SetPlayerModel(playerCondition);
        _precenters.GetComponent<PlayerWeightUIPresenter>().SetPlayerModel(playerCondition);
    }
}