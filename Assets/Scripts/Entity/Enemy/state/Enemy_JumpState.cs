// using Unity.Netcode;
// using UnityEngine;
//
// public class Enemy_JumpState : EnemyState
// {
//     private Entity _entity;
//     public EnemyJumpData _jumpData;
//     public string StateName;
//     private Vector2 originalGroundCheckLocalPos;
//     public Enemy_JumpState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName,EnemyJumpData JumpData) : base(enemy, enemyStateMachine, animBoolName)
//     {
//         _jumpData =  JumpData;
//     }
//
//     public override void Enter()
//     {   
//         base.Enter();
//         originalGroundCheckLocalPos = enemy.groundCheck.localPosition;
//         enemy.groundCheck.localPosition = new Vector2(0f, originalGroundCheckLocalPos.y);
//
//         // 수평 속도와 수직 속도를 함께 적용하여 '도약' 구현
//         float horizontalVelocity = _jumpData.jumpVelocity * enemy.FacingDirection;
//         enemy.SetVelocity(horizontalVelocity, _jumpData.jumpForce);
//         _jumpData.isJumping = true;
//     }
//     
//     public override void Update()
//     {
//         base.Update();
//         
//         if (_jumpData.isJumping && enemy.IsGroundDetected)
//         {
//             _jumpData.isJumping = false;
//             enemy.groundCheck.localPosition = originalGroundCheckLocalPos;
//             ChangeStates(StateName);
//         }
//     }
//
//     private void ChangeStates(string stateName)
//     {
//         if (stateName == "Chase_director")
//         {
//             enemyStateMachine.ChangeState(enemy.ChaseDirector);
//         }
//         if (stateName == "Move_director")
//         {
//             enemyStateMachine.ChangeState(enemy.MoveDirector);
//         }
//     }
// }