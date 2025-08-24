using System;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private Camera cam;
    public LineRenderer lineRenderer;
    public Transform firePoint;
    public GameObject startVFX;
    public GameObject endVFX;
    
    private Quaternion rotation;
    private List<ParticleSystem> particles = new List<ParticleSystem>();
    
    [SerializeField] private LayerMask layerMask;

    // 레이저 관련 프로퍼티 추가
    public float maxDistance = 10f; // 최대 사거리
    public RaycastHit2D LastHit { get; private set; }

    private void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    
    private void Start()
    {
        FillLists();
        DisableLaser();
    }

    public void EnableLaser()
    {
        lineRenderer.enabled = true;

        for (int i = 0; i < particles.Count; i++)
            particles[i].Play();
    }

    public void UpdateLaser()
    {
        var mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        var firePointPos = (Vector2)firePoint.position;
        
        lineRenderer.SetPosition(0, firePointPos);
        startVFX.transform.position = firePointPos;
        
        RotateToMouse(); // 마우스 방향으로 회전하는 로직은 계속 실행
        
        Vector2 direction = (mousePos - firePointPos).normalized;
        LastHit = Physics2D.Raycast(firePointPos, direction, maxDistance, layerMask);

        Vector2 endPoint;
        if (LastHit.collider != null)
        {
            // Raycast에 충돌한 지점까지 레이저를 그립니다.
            endPoint = LastHit.point;
        }
        else
        {
            // 충돌한 지점이 없으면 최대 사거리까지 레이저를 그립니다.
            endPoint = firePointPos + direction * maxDistance;
        }
        
        lineRenderer.SetPosition(1, endPoint);
        endVFX.transform.position = endPoint;
    }
    
    public void DisableLaser()
    {
        lineRenderer.enabled = false;
        
        for (int i = 0; i < particles.Count; i++)
            particles[i].Stop();
    }

    void RotateToMouse()
    {
        Vector2 direction = cam.ScreenToWorldPoint(Input.mousePosition) - firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation.eulerAngles = new Vector3(0, 0, angle);
        transform.rotation = rotation;
    }
    
    private void FillLists()
    {
        for (int i = 0; i < startVFX.transform.childCount; i++)
        {
            var ps = startVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if(ps != null)
                particles.Add(ps);
        }

        // 오류가 발생했던 부분을 수정한 for 루프입니다.
        for (int i = 0; i < endVFX.transform.childCount; i++)
        {
            var ps = endVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if(ps != null)
                particles.Add(ps);
        }
    }
}