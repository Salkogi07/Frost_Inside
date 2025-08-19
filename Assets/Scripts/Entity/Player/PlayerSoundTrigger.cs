using UnityEngine;

public class PlayerSoundTrigger : MonoBehaviour
{
    public void WalkSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Walk Snow");   
    }
}