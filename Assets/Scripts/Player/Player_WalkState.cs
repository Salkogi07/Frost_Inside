using UnityEngine;

public class Player_WalkState : Player_GroundedState
{
    private float timeCool = 0.3f;
    private float timer;
    
    public Player_WalkState(Player player, Player_StateMachine playerStateMachine, string stateName) : base(player, playerStateMachine, stateName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (timeCool < timer)
        {
            Debug.Log("Cool");
            FMODUnity.RuntimeManager.PlayOneShot("event:/Walk Snow");
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
            
        player.Condition.StaminaRecovery();
        
        if (player.MoveInput == 0)
            playerStateMachine.ChangeState(player.IdleState);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        player.SetMoveSpeed(player.Stats.WalkSpeed);
        player.SetVelocity(player.MoveInput * player.CurrentSpeed, rigidbody.linearVelocity.y);
    }
}
