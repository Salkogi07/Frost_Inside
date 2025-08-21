﻿using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Logo appears")]
    [field: SerializeField] public EventReference logoAppear {get; private set;}
    
    [field: Header("Player Snow Move")]
    [field: SerializeField] public EventReference playerSnowMove {get; private set;}
    
    [field: Header("Logo Appears")]
    [field: SerializeField] public EventReference logoAppears {get; private set;}
    
    [field: Header("Loading")]
    [field: SerializeField] public EventReference loading {get; private set;}
    
    public static FMODEvents instance { get; private set; }
    
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}