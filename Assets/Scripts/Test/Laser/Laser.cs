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
    
    private void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    
    private void Start()
    {
        FillLists();
        DisableLaser();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            EnableLaser();
        }

        if (Input.GetButton("Fire1"))
        {
            UpdateLaser();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            DisableLaser();
        }
        
        RotateToMouse();
    }

    void EnableLaser()
    {
        lineRenderer.enabled = true;

        for (int i = 0; i < particles.Count; i++)
            particles[i].Play();
    }

    void UpdateLaser()
    {
        var mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        var firePointPos = (Vector2)firePoint.position;
        
        lineRenderer.SetPosition(0, firePointPos);
        startVFX.transform.position = firePointPos;
        
        lineRenderer.SetPosition(1, mousePos);
        
        Vector2 direction = mousePos - firePointPos;
        RaycastHit2D hit = Physics2D.Raycast(firePointPos, direction.normalized, direction.magnitude, layerMask);

        if (hit)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        
        endVFX.transform.position = lineRenderer.GetPosition(1);
    }
    
    void DisableLaser()
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

        for (int i = 0; i < endVFX.transform.childCount; i++)
        {
            var ps = endVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if(ps != null)
                particles.Add(ps);
        }
    }
}