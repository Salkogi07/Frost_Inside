using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Player_Teleport : MonoBehaviour
{
    public Transform targetPos;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entered Player");
        if (other.tag == "Player")
        {
            Player player = other.GetComponent<Player>();
            if (player.IsOwner)
            {
                StartCoroutine(Teleport(player));
                
            }
        }
    }

    IEnumerator Teleport(Player _player)
    {
        GameObject camObj = GameObject.FindGameObjectWithTag("CinemachineCamera");
        camObj.GetComponent<CinemachinePositionComposer>().Damping = new Vector3(0, 0, 0);
        _player.Teleport(targetPos.position);
        yield return new  WaitForSeconds(0.1f);
        camObj.GetComponent<CinemachinePositionComposer>().Damping = new Vector3(1, 1, 1);
    }
}