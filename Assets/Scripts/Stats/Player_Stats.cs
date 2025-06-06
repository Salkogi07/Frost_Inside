using Stats;
using UnityEngine;

public class Player_Stats : MonoBehaviour
{
    [Header("Move Info")]
    public float WalkSpeed;
    public float RunSpeed;

    [Header("Stamina Info")] 
    public float SprintCost;
    public float JumpCost;
    public float StaminaRecoverRate;
    public float staminaDecreaseRate;
    
    [Header("Stats")]
    public Stat MaxHp;
    public Stat MaxStamina;
    public Stat MaxWeight;
    public Stat MaxTemperature;
    
    public Stat Mining;
    
    public Stat_DefenseGrup Defense;
    public Stat_OffenseGroup Offense;
}
