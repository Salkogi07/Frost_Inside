using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_AiredState : EntityState
    {
        public Player_AiredState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
        {
        }

        public override void Update()
        {
            base.Update();

            if (player.MoveInput != 0)
                player.SetVelocity(player.MoveInput * (player.CurrentSpeed * player.inAirMoveMultiplier) , rigidbody.linearVelocity.y);
        }
    }
}
