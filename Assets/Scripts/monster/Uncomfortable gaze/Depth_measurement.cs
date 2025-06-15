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

        if (Depth>= 48f)
        {
            Debug.Log("가가ㅏ가ㅏㅏㅏㅏㅏ가ㅏㄱ");
        }else if (Depth < 48f && Depth >= 4f)
        {
            Debug.Log("2");

        }else if (Depth < 4f && Depth >= -22f)
        {
            Debug.Log("33");

        }
        else if(Depth < -22f && Depth >= -46f)
        {
            Debug.Log("444");

        }
        
    }
    
}
