// using UnityEngine;
//
//
//     public class Enemy_IdleState : Idle_director
//     {
//         
//         public Enemy_IdleState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
//         {
//             
//         }
//
//         public override void Enter()
//         {
//             base.Enter();
//             
//             stateTimer = enemy.IdleTime;
//         }
//
//         public override void Update()
//         {
//             base.Update();
//
//             if (stateTimer < 0)
//             {
//                 if (enemy.IsGroundDetected == false || enemy.IsWallDetected)
//                 {
//                     enemy.Flip();
//                 }
//                 enemyStateMachine.ChangeState(enemy.MoveDirector);
//             }
//                 
//             
//         }
//         
//     }
