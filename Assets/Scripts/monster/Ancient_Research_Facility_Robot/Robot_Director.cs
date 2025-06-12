using UnityEngine;

public class Robot_Director : MonoBehaviour
{
    
    public GameObject Shockwave;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(Shockwave, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
