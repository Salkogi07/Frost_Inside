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

            Debug.Log("air");

            if (player.moveInput.x != 0)
                player.SetVelocity(player.moveInput.x * (player.moveSpeed * player.inAirMoveMultiplier) , rb.linearVelocity.y);
        }
    }
}
