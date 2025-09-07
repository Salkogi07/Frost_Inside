public class Player_DeathState : PlayerState
{
    public Player_DeathState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        player.GetComponent<Player_ItemDrop>().DropAllItems();
    }
}