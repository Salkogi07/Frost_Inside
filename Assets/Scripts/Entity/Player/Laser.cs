using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Laser : MonoBehaviour
{
    public Camera cam;
    public LineRenderer lineRenderer;
    public Transform rotationPoint;
    public Transform firePoint;
    public GameObject startVFX;
    public GameObject endVFX;
    
    private Quaternion rotation;
    private List<ParticleSystem> particles = new List<ParticleSystem>();

    private void Awake()
    {
        if (cam == null)
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

    // 물리 로직을 제거하고 외부에서 받은 endPoint로 레이저를 그리도록 변경
    public void UpdateLaser(Vector2 endPoint)
    {
        RotateToMouse(); // 마우스 방향으로 회전하는 로직은 Owner만 실행
        UpdateLaserVisuals(endPoint, transform.rotation);
    }
    
    public void UpdateLaserVisuals(Vector2 endPoint, Quaternion newRotation)
    {
        transform.rotation = newRotation;

        var firePointPos = (Vector2)firePoint.position;
        lineRenderer.widthMultiplier = .2f;
        lineRenderer.SetPosition(0, firePoint.transform.position);
        startVFX.transform.position = firePointPos;
        
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
        Vector2 direction = cam.ScreenToWorldPoint(Input.mousePosition) - rotationPoint.position;
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

        for (int i = 0; i < endVFX.transform.childCount; i++)
        {
            var ps = endVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if(ps != null)
                particles.Add(ps);
        }
    }
}