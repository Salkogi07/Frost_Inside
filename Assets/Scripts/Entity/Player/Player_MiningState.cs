using UnityEngine;

public class Player_MiningState : Player_GroundedState
{
    public Player_MiningState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player,
        playerStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0, rigidbody.linearVelocity.y); // 채굴 시작 시 수평 이동 정지
        player.Laser.EnableLaser();
    }

    public override void Update()
    {
        // base.Update()를 호출하지 않아 점프, 달리기 등의 전환을 막습니다.

        // 마이닝 키를 떼거나, 땅에서 떨어지거나, 채굴이 불가능한 상태가 되면 Idle 상태로 전환
        if (Input.GetKeyUp(KeyManager.instance.GetKeyCodeByName("Mining")) || !player.IsGroundDetected || !player.CanMine)
        {
            playerStateMachine.ChangeState(player.IdleState);
            return;
        }
        
        // 플레이어의 TileMining 로직을 호출하여 채굴을 업데이트합니다.
        player.TileMining.HandleMiningAndLaserUpdate();
    }

    public override void FixedUpdate()
    {
        // 채굴 중에는 물리적 움직임이 없으므로 base.FixedUpdate()를 호출하지 않습니다.
        // 필요한 경우 여기에 별도의 물리 로직을 추가할 수 있습니다.
    }

    public override void Exit()
    {
        base.Exit();
        player.TileMining.StopMining(); // 상태를 나갈 때 채굴 관련 변수 초기화
        player.Laser.DisableLaser(); // 레이저 비활성화
    }
}