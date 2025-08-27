
    using UnityEngine;

    public class Enemy_MoveState : Move_director
    {

        public Enemy_MoveState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy,
            enemyStateMachine, animBoolName)
        {

        }

        public override void Enter()
        {
            base.Enter();


        }

        public override void Update()
        {
            base.Update();
            if (enemy.IsGroundDetected && enemy.IsWallDetected && enemy.GetState<Enemy_JumpState>() != null &&
                CanJumpOverWall())
            {

                enemy.GetState<Enemy_JumpState>().StateName = "Move_director";
                enemyStateMachine.ChangeState(enemy.GetState<Enemy_JumpState>());

            }
            else if (!enemy.GetState<Enemy_JumpState>()._jumpData.isJumping && enemy.IsGroundDetected == false ||
                     enemy.IsWallDetected)
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

        public bool CanJumpOverWall()
        {
            // 1. 점프해서 도달할 수 있는 최대 높이 계산
            float jumpForce = enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce;
            float gravity = Mathf.Abs(Physics2D.gravity.y * enemy.rb.gravityScale);
            // 현재 위치(발밑) 기준으로 얼마나 높이 올라갈 수 있는지 계산
            float maxJumpHeight = (jumpForce * jumpForce) / (2 * gravity);

            // 적이 점프했을 때 도달할 수 있는 월드 좌표상의 Y값
            float jumpPeakYPosition = enemy.transform.position.y + maxJumpHeight;

            // 2. 벽의 윗면 높이를 찾기 위한 레이캐스트 시작점 설정
            //    - 시작 X 위치: 벽 앞 (wallCheckDistance 만큼 앞)
            //    - 시작 Y 위치: 적의 현재 위치보다 최대 점프 높이만큼 위 (천장에 막히는 경우를 대비해 여유 공간 확보)
            Vector2 raycastStartPoint = new Vector2(
                enemy.transform.position.x + (enemy.wallCheckDistance * enemy.FacingDirection),
                enemy.transform.position.y + maxJumpHeight + 0.1f // 0.1f는 오차 방지를 위한 여유 값
            );

            // 3. 아래 방향으로 레이캐스트를 발사하여 벽의 윗면 찾기
            RaycastHit2D wallTopHit = Physics2D.Raycast(raycastStartPoint, Vector2.down, maxJumpHeight + 0.2f,
                enemy.whatIsGround | enemy.whatIsWall);

            // Gizmos를 통해 시각적으로 확인하려면 아래 주석을 해제하세요.
            // Debug.DrawRay(raycastStartPoint, Vector2.down * (maxJumpHeight + 0.2f), Color.red, 1f);

            if (wallTopHit.collider != null)
            {
                // 4. 벽의 윗면 높이와 점프 최대 높이 비교
                float wallTopYPosition = wallTopHit.point.y;

                // 점프했을 때의 최고 높이가 벽의 윗면보다 높으면 점프 가능
                if (jumpPeakYPosition > wallTopYPosition)
                {
                    // 추가 확인: 점프해서 넘어갔을 때 착지할 공간이 있는지 확인하면 더 좋습니다.
                    // (이 부분은 필요에 따라 추가 구현)
                    Debug.Log("이@재@명");
                    
                    return true;
                }

                // 점프 높이가 벽을 넘기에 부족함
                return false;
            }

            // 레이캐스트에 감지된 벽이 없다면, 점프를 방해할 장애물이 없는 것이므로 점프 가능
            Debug.Log("asdasdasfrr");
            return true;
        }
    }
