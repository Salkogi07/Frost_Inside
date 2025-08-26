using System;
using UnityEngine;
[Serializable]
public class EnemyJumpData
{
    [Header("Jump Data")]
    public float jumpForce;
    public float jumpCoolTime;
    public bool isJumping = false;
    // [HideInInspector]
    
    // public EnemyJumpData Clone()
    // {
    //     return new EnemyJumpData
    //     {
    //         jumpForce = this.jumpForce,
    //         jumpCoolTime = this.jumpCoolTime,
    //         isJumping = this.isJumping
    //     };
    // }
}

