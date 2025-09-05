using System;
using R3;
using UnityEngine;

public class GameTimerUIPresenter : MonoBehaviour
{
    [SerializeField] private GameTimerUIView view;
    

    private void Start()
    {
        TimerManager model = TimerManager.instance;
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