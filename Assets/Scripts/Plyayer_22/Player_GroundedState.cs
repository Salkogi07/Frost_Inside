using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_GroundedState : PlayerState
    {
        public Player_GroundedState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
        {
        }

        public override void Update()
        {
            base.Update();
            
            KeyCode miningKey = KeyManager.instance.GetKeyCodeByName("Mining");
            if (player.IsGroundDetected && Input.GetKey(miningKey) && player.TileMining.CanMining())
            {
                // 채굴 가능 상태라면 채굴 상태로 전이
                playerStateMachine.ChangeState(player.MiningState);
                return;  // 이후의 점프/이동 로직은 실행하지 않음
            }
            
            //안전 장치
            if(player.IsGroundDetected)
                player.SetVelocity(rigidbody.linearVelocity.x, 0);

            if (rigidbody.linearVelocity.y < 0)
                playerStateMachine.ChangeState(player.FallState);
            
            if(player.Condition.CanSprint())
                if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Sprint")) && player.MoveInput != 0)
                    playerStateMachine.ChangeState(player.RunState);
            
            if(player.Condition.CanJump())
                if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Jump")))
                    playerStateMachine.ChangeState(player.JumpState);
        }
    }
}
