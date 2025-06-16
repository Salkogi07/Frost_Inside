using UnityEngine;

public class Depth_measurement : MonoBehaviour
{
    // public Transform enemy;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        measurement();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void measurement()
    {
        float Depth = transform.position.y;

        if (Depth>= 80f)
        {
            
        }else if (Depth < 80f && Depth >= 70f)
        {
            Debug.Log("2");

        }else if (Depth < 70f && Depth >= 60f)
        {
            Debug.Log("33");

        }
        else if(Depth < 60f && Depth >= 50f)
        {
            Debug.Log("444");

        }
        
    }
    
}
