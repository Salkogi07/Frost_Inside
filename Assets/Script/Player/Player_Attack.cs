using System.Collections;
using UnityEngine;

public class Player_Attack : MonoBehaviour
{
    Player_Move playerMove;
    Player_Stats stats;

    private float curTime;
    public float coolTime = 0.5f;
    public Transform pos;
    public Vector2 boxSize;
    private int playerAttack = 100;
    public GameObject AttackEffect;
    public AudioClip attackSound;
    private AudioSource audioSource;

    private void Awake()
    {
        playerMove = GetComponent<Player_Move>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (curTime <= 0)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !playerMove.isAttack)
            {
                audioSource.PlayOneShot(attackSound);
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    GameObject testEffect = Instantiate(AttackEffect, pos);
                    Destroy(testEffect, 0.5f);
                }
                //playerMove.isAttack = true;
                curTime = coolTime;
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }
}