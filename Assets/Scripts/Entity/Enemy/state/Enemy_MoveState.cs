//
//     using UnityEngine;
//
//     public class Enemy_MoveState : Move_director
//     {
//
//         public Enemy_MoveState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy,
//             enemyStateMachine, animBoolName)
//         {
//
//         }
//
//         public override void Enter()
//         {
//             base.Enter();
//
//
//         }
//
//         public override void Update()
//         {
//             base.Update();
//             if (enemy.IsWallDetected && CanPerformLeap())
//             {
//                 enemy.GetState<Enemy_JumpState>().StateName = "Move_director";
//                 enemyStateMachine.ChangeState(enemy.GetState<Enemy_JumpState>());
//             }
//             else if (!enemy.GetState<Enemy_JumpState>()._jumpData.isJumping && (enemy.IsWallDetected || !enemy.IsGroundDetected))
//             {
//                 enemyStateMachine.ChangeState(enemy.IdleDirector);
//             }
//         }
//
//         public override void FiexedUpdate()
//         {
//             base.FiexedUpdate();
//
//             enemy.SetVelocity(enemy.MoveSpeed * enemy.FacingDirection, rigidbody.linearVelocity.y);
//         }
//
//
//     // public bool MeasureWallHeight()
//     // {
//     //     
//     //     Vector2 startpoint = new Vector2(enemy.transform.position.x + (enemy.wallCheckDistance * enemy.FacingDirection), enemy.transform.position.y);
//     //        
//     //     while (enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce <= startpoint.y)
//     //     {
//     //         startpoint = new Vector2(startpoint.x, startpoint.y + 0.01f);
//     //         RaycastHit2D hit = Physics2D.Raycast(startpoint, Vector2.up, enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce, enemy.whatIsWall);
//     //         return true;
//     //     }
//     //     return false;
//     // }
// }
