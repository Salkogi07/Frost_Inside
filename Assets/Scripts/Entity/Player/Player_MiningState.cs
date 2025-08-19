using UnityEngine;

public class Player_MiningState : PlayerState
{
    public Player_MiningState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
            
        player.SetVelocity(0, rigidbody.linearVelocity.y);
    }
}