
using System.Collections;
using UnityEngine;

public class Enemy_laboratory_robot_AttackState : Enemy_laboratory_robot_State
{
    public Enemy_laboratory_robot_AttackState(Enemy_laboratory_robot laboratoryRobot,
        Enemy_laboratory_robot_StateMachine stateMachine, string animBoolName) : base(laboratoryRobot,
        stateMachine, animBoolName)
    {

    }

    private Vector2 fireOrigin;
    private Vector2 currentDirection;

    public override void Enter()
    {
        base.Enter();
        
        // fireOrigin = (laboratoryRobot.player.position - laboratoryRobot.transform.position).normalized;
        // fireOrigin.Normalize();
        laboratoryRobot.StartCoroutine(FireGatlingGun());
        currentDirection  = (laboratoryRobot.player.position - laboratoryRobot.transform.position).normalized;

    }

    public override void Update()
    {
        base.Update();

        // 플레이어가 공격 범위 내에 있을 때 공격

        RotateGunTowardsPlayer();


        // 애니메이션 트리거가 호출되면 상태 전이
        if (triggerCalled)
        {
            enemyStateMachine.ChangeState(laboratoryRobot.BattleState);
        }
    }

    private void RotateGunTowardsPlayer()
    {
        Transform machinegundirection = laboratoryRobot.machinegundirection;
        Vector2 direction = laboratoryRobot.player.position - machinegundirection.transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 현재 회전 (Z축 기준)
        float currentAngle = machinegundirection.transform.eulerAngles.z;

        // 천천히 회전
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * laboratoryRobot.raySpeed);
        
        // 회전 적용 (Z축만 회전)
        machinegundirection.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    

    private IEnumerator FireGatlingGun()
    {




        for (int i = 0; i < laboratoryRobot.machin_gun_bullet; i++)
        {
            FireBullet();
            
            yield return new WaitForSeconds(laboratoryRobot.fireRate);
        }









        laboratoryRobot.cooldown = laboratoryRobot.cooltime;
        // 모든 발사 후 상태 전환
        triggerCalled = true;
    }

    private void FireBullet()
    {
       
        
        Transform gunTransform = laboratoryRobot.machinegundirection;

        Vector2 fireOrigin = gunTransform.position;
        Vector2 direction = gunTransform.right; // 또는 up - 총구 방향에 따라 변경
        float maxDistance = laboratoryRobot.attackDistance;

        // Raycast로 총알을 발사하듯 시뮬레이션
        RaycastHit2D hit = Physics2D.Raycast(fireOrigin, direction, maxDistance, laboratoryRobot.whatIsPlayer);

        // 시각적 디버그
        Vector2 endPoint = fireOrigin + direction * maxDistance;
        Debug.DrawLine(fireOrigin, endPoint, Color.red, 0.3f);

        // 타격 판정 처리
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Debug.Log("플레이어 명중!");
            // hit.collider.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }
}




