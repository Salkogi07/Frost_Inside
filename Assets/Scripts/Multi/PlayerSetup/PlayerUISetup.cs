using Unity.Netcode;
using UnityEngine;

public class PlayerUISetup : NetworkBehaviour
{
    private GameObject Precenters;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Precenters = GameObject.FindGameObjectWithTag("PlayerPrecenters");
            if (Precenters != null)
            {
                SettingPlayerUI(gameObject);
            }
        }
    }
    
    private void SettingPlayerUI(GameObject player)
    {
        Player_Condition playerCondition = player.GetComponent<Player_Condition>();
        Precenters.GetComponent<PlayerHpUIPrecenter>().SetPlayerModel(playerCondition);
        Precenters.GetComponent<PlayerStaminaUIPresenter>().SetPlayerModel(playerCondition);
        Precenters.GetComponent<PlayerTemperatureUIPresenter>().SetPlayerModel(playerCondition);
        Precenters.GetComponent<PlayerWeightUIPresenter>().SetPlayerModel(playerCondition);
    }
}