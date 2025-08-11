using System;
using R3;
using UnityEngine;

public class GameTimerUIPresenter : MonoBehaviour
{
    [SerializeField] private GameTimerUIView view;
    

    private void Start()
    {
        GameManager model = GameManager.instance;
        model.HoursObservable.Subscribe(hours =>
        {
            view.UpdateTime(model.Hours, model.Minutes);
        });
        
        model.MinutesObservable.Subscribe(minutes =>
        {
            view.UpdateTime(model.Hours, model.Minutes);
        });
    }
}