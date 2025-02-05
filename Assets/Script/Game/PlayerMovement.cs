using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public GameObject PlayerModel;

    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            if(PlayerModel.activeSelf == false)
            {
                SetPosition();
                PlayerModel.SetActive(true);
            }

            if (authority)
            {
                Movement();
            }
        }
    }

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-15,15), -3.55f, 0.0f);
    }

    public void Movement()
    {
        float xDirection = Input.GetAxisRaw("Horizontal");

        Vector2 moveDirection = new Vector2(xDirection, 0.0f).normalized;

        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }
}
