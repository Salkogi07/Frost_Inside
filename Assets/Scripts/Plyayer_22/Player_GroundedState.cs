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
            
            //안전 장치
            if(player.IsGroundDetected)
                player.SetVelocity(rigidbody.linearVelocity.x, 0);

            if (rigidbody.linearVelocity.y < 0)
                playerStateMachine.ChangeState(player.FallState);

            if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Sprint")) && player.MoveInput != 0)
                playerStateMachine.ChangeState(player.RunState);
            
            if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Jump")))
                playerStateMachine.ChangeState(player.JumpState);
        }
    }
}
