
    using UnityEngine;

    public class Enemy_MoveState : Move_director
    {
        
        public Enemy_MoveState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            
            // if (enemy.IsGroundDetected == false || enemy.IsWallDetected)
            //     enemy.Flip();
        }

        public override void Update()
        {
            base.Update();
            if(enemy.IsWallDetected && enemy.GetState<Enemy_JumpState>() != null && MeasureWallHeight())
            {
            
                enemy.GetState<Enemy_JumpState>().StateName = "Move_director";
                enemyStateMachine.ChangeState(enemy.GetState<Enemy_JumpState>());
            
            }else if (enemy.IsGroundDetected == false || enemy.IsWallDetected)
            {
                Debug.Log("야 기분쪼타");
                enemyStateMachine.ChangeState(enemy.IdleDirector);
            }
            
            
                
            
            

        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();
            
            enemy.SetVelocity(enemy.MoveSpeed * enemy.FacingDirection, rigidbody.linearVelocity.y);
        }
        
        
        // public bool MeasureWallHeight()
        // {
        //     
        //     Vector2 startpoint = new Vector2(enemy.transform.position.x + (enemy.wallCheckDistance * enemy.FacingDirection), enemy.transform.position.y);
        //        
        //     while (enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce <= startpoint.y)
        //     {
        //         startpoint = new Vector2(startpoint.x, startpoint.y + 0.01f);
        //         RaycastHit2D hit = Physics2D.Raycast(startpoint, Vector2.up, enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce, enemy.whatIsWall);
        //         return true;
        //     }
        //     return false;
        // }
        public bool MeasureWallHeight()
        {
            float jumpHeight = enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce;

            // 벽의 앞쪽 위치 (바로 앞 좌표)
            Vector2 startPoint = new Vector2(
                enemy.transform.position.x + (enemy.wallCheckDistance * enemy.FacingDirection),
                enemy.transform.position.y
            );

            // 점프 가능한 최대 높이까지 검사
            for (float yOffset = 0; yOffset <= jumpHeight; yOffset += 0.05f)
            {
                Vector2 checkPoint = new Vector2(startPoint.x, startPoint.y + yOffset);

                // 벽이 있는지 체크 (위로 올라가면서 벽이 없는 공간을 찾는 방식)
                RaycastHit2D hit = Physics2D.Raycast(checkPoint, Vector2.right * enemy.FacingDirection, 0.1f, enemy.whatIsWall);

                if (hit.collider == null)
                {
                    // 이 높이(yOffset)에서는 벽이 뚫려있음 → 점프 가능
                    return true;
                }
            }

            // 끝까지 벽이 막혀 있으면 점프 불가
            return false;
        }
    }
