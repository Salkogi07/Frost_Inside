using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_GroundedState : EntityState
    {
        public Player_GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
        {
        }

        public override void Update()
        {
            base.Update();
            
            if(player.IsGroundDetected)
                player.SetVelocity(rigidbody.linearVelocity.x, 0);

            if (rigidbody.linearVelocity.y < 0)
                stateMachine.ChangeState(player.FallState);


            if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Sprint")) && player.MoveInput != 0)
                stateMachine.ChangeState(player.RunState);

            
            if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Jump")))
                stateMachine.ChangeState(player.JumpState);
        }
    }
}
