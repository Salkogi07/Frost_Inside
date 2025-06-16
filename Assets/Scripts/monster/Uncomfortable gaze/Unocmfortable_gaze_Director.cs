using UnityEngine;

public class Unocmfortable_gaze_Director : MonoBehaviour
{
    private Depth_measurement depth_measurement;
    public int outrage = 1;

    // Update is called once per frame
    void Update()
    {
        depth_measurement = GetComponent<Depth_measurement>();
    }

    public void Start()
    {
        depth_measurement.measurement();
        // switch (outrage)
        // {
        //     case 1:
        //         outrage_1();       
        //     break;
        //     
        //     case 2:
        //         
        //         break;
        //     case 3:
        //         
        //         break;
        //     case 4:
        //         
        //         break;
        // }
        
        
    }
    
    
    
}
