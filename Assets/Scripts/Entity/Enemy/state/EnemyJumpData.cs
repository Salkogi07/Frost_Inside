using System;
using UnityEngine;
[Serializable]
public class EnemyJumpData
{
    [Header("Jump Data")]
    public float jumpForce;
    public float jumpCoolTime;
    [HideInInspector]public bool isJumping;
    
    
    public EnemyJumpData Clone()
    {
        return new EnemyJumpData
        {
            jumpForce = this.jumpForce,
            jumpCoolTime = this.jumpCoolTime,
            isJumping = this.isJumping
        };
    }
}

