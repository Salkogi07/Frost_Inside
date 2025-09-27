using FMODUnity;
using UnityEngine;

public class UIAudioTrigger : MonoBehaviour
{
    public void ButtonHover()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.buttonHover, transform.position);
    }
    
    public void ButtonClick()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.buttonClick, transform.position);
    }
}
