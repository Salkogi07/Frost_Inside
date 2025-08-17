// using UnityEngine;
//
// public class Enemy_JumpState : EnemyState
// {
//     private Entity _entity;
//
//
//     public Enemy_JumpState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName, Entity entity) : base(enemy, enemyStateMachine, animBoolName)
//     {
//         _entity = entity;
//     }
//
//     public override void Enter()
//     {   
//         base.Enter();
//         jumping(rigidbody.linearVelocity.x,enemy.jumpForce);
//         
//         
//         
//     }
//
//     public override void Update()
//     {
//         base.Update();
//         
//         
//         if (enemy.JumPing && enemy.IsGroundDetected)
//         {
//             enemy.JumPing = false;
//         }
//     }
//     
//     public bool MeasureWallHeight()
//     {
//         
//         Vector2 startpoint = new Vector2(enemy.EnemyCheck().x + (enemy.wallCheckDistance * enemy.FacingDirection), enemy.EnemyCheck().y);
//            
//         while (enemy.jumpForce <= startpoint.y)
//         {
//             startpoint = new Vector2(startpoint.x, startpoint.y + 0.01f);
//             RaycastHit2D hit = Physics2D.Raycast(startpoint, Vector2.up, enemy.jumpForce, enemy.whatIsWall);
//             return true;
//         }
//         return false;
//     }
//     public void jumping(float xVelocity, float yVelocity)
//     {
//         
//         Debug.Log("너의 곁에 있으면 나는 행복해  "+rigidbody);
//         enemy.JumPing = true;
//         enemy.SetVelocity(xVelocity, yVelocity,enemy.JumPing);
//         
//         enemy.jumpingTime = enemy.jumpCoolTime;
//     }
// }