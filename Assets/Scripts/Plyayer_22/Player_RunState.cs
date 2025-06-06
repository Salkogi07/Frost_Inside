﻿using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_RunState : Player_GroundedState
    {
        public Player_RunState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
        {
        }
        
        public override void Update()
        {
            base.Update();

            player.PlayerCondition.UseStaminaToSprint();
            
            if (player.MoveInput == 0)
                playerStateMachine.ChangeState(player.IdleState);
            
            if (Input.GetKeyUp(KeyManager.instance.GetKeyCodeByName("Sprint")))
                playerStateMachine.ChangeState(player.WalkState);

            if (player.PlayerCondition.currentStamina.Value <= 0)
                playerStateMachine.ChangeState(player.WalkState);
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();
            
            player.SetMoveSpeed(player.Stats.RunSpeed);
            player.SetVelocity(player.MoveInput * player.CurrentSpeed, rigidbody.linearVelocity.y);
        }
    }
}