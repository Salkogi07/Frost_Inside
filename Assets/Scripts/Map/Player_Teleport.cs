using System;
using UnityEngine;

public class Player_Teleport : MonoBehaviour
{
    public Transform targetPos;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entered Player");
        if (other.tag == "Player")
        {
            Debug.Log("Entered Player");
            Camera.main.gameObject.transform.position = targetPos.position;
            other.GetComponent<Player>().Teleport(targetPos.position);
        }
    }
}