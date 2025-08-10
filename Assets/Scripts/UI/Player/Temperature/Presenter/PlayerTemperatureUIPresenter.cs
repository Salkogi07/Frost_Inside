using R3;
using UnityEngine;

public class PlayerTemperatureUIPresenter : MonoBehaviour
{
    [SerializeField] private PlayerTemperatureUIView view;
    [SerializeField] private Player_Condition model;

    private void Start()
    {
        model.TemperatureObservable.Subscribe(temp =>
        {
            view.UpdateTemperature(temp, model.GetMaxTemperature());
            view.UpdateEdges(temp, model.GetMaxTemperature());
        });
    }
    
    public void SetPlayerModel(Player_Condition model)
    {
        this.model = model;
    }
}