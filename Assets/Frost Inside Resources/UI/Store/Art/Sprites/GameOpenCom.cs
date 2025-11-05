using System;
using UnityEngine;

public class GameOpenCom : MonoBehaviour
{
    public GameObject gameOpen;
    public GameObject test;
    
    bool isEnter = false;
    
    public void Open()
    {
        gameOpen.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            test.SetActive(true);
            isEnter = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            test.SetActive(false);
            isEnter = false;
        }
    }

    private void Update()
    {
        if (isEnter)
        {
            KeyCode interactionKey = KeyManager.instance.GetKeyCodeByName("Interaction");
            
            if (Input.GetKeyDown(interactionKey))
            {
                Open();
            }
        }
    }
}
