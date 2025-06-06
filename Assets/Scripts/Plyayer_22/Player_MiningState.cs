using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_MiningState : Player_GroundedState
    {
        private readonly KeyCode _miningKey;
        
        public Player_MiningState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
        {
            _miningKey = KeyManager.instance.GetKeyCodeByName("Mining");
        }

        public override void Enter()
        {
            base.Enter();
            
            player.TileMining.CanMiningToFalse();
        }
        
        public override void Update()
        {
            base.Update();

            // === ➎ 채굴 키가 떼어졌거나, 채굴이 완료되어 isMining == false일 때 ===
            if (!Input.GetKey(_miningKey) || !player.TileMining.isMining)
            {
                // 채굴 중단 혹은 완료된 경우, 다시 IdleState로 전이
                playerStateMachine.ChangeState(player.IdleState);
            }

            player.SetVelocity(0,rigidbody.linearVelocity.y);
        }

        public override void Exit()
        {
            base.Exit();
            
            player.TileMining.CanMiningToTrue();
        }
    }
}