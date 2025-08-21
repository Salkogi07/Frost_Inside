using FMOD.Studio;
using UnityEngine;

public class PlayerSoundTrigger : MonoBehaviour
{
    private EventInstance _playerSnowMove;

    private void Awake()
    {
        _playerSnowMove = AudioManager.instance.CreateInstance(FMODEvents.instance.playerSnowMove);
    }
    
    public void PlaySnowWalk()
    {
        _playerSnowMove.start();
    }
}