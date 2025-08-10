using R3;
using UnityEngine;

public class PlayerWeightUIPresenter : MonoBehaviour
{
    [SerializeField] private PlayerWeightUIView view;
    [SerializeField] private Player_Condition model;

    private void Start()
    {
        model.WeightObservable.Subscribe(temp =>
        {
            view.UpdateWeight(temp, model.GetMaxWeight());
        });
    }
    
    public void SetPlayerModel(Player_Condition model)
    {
        this.model = model;
    }
}