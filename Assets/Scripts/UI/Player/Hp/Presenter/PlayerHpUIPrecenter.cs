using System;
using R3;
using UnityEngine;

public class PlayerHpUIPrecenter : MonoBehaviour
{
    [SerializeField] private PlayerHpUIView view;
    [SerializeField] private Player_Condition model;

    private void Start()
    {
        model.HpObservable.Subscribe(hp =>
        {
            view.UpdateHp(hp, model.GetMaxHp());
        });
    }
    
    public void SetPlayerModel(Player_Condition model)
    {
        this.model = model;
    }
}