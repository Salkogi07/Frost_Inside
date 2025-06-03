using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_RunState : Player_GroundedState
    {
        public Player_RunState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
        {
        }
        
        public override void Update()
        {
            base.Update();
            
            if (player.MoveInput == 0)
                stateMachine.ChangeState(player.IdleState);
            
            if (Input.GetKeyUp(KeyManager.instance.GetKeyCodeByName("Sprint")))
                stateMachine.ChangeState(player.WalkState);
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            player.SetVelocity(player.MoveInput * player.RunSpeed, rigidbody.linearVelocity.y);
        }
    }
}