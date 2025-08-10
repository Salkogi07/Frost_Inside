using R3;
using UnityEngine;

public class PlayerStaminaUIPresenter : MonoBehaviour
{
    [SerializeField] private PlayerStaminaUIView view;
    [SerializeField] private Player_Condition model;

    private void Start()
    {
        model.StaminaObservable.Subscribe(stamina =>
        {
            view.UpdateHp(stamina, model.GeMaxStamina());
        });
    }
    
    public void SetPlayerModel(Player_Condition model)
    {
        this.model = model;
    }
}