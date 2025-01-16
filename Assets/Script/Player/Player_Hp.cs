using System.Collections;
using UnityEngine;

public class Player_Hp : MonoBehaviour
{
    public float invincibilityDuration = 0.5f;

    Player_Move playerMove;
    SpriteRenderer sprite;
    Rigidbody2D rb;

    public bool isInvincible  = false;

    private void Awake()
    {
        playerMove = GetComponent<Player_Move>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }
    
    public void Damage_HP(int _value)
    {
        if (isInvincible)
            return;
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        sprite.color = new Color(1, 1, 1, 0.5f);

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        sprite.color = new Color(1, 1, 1, 1f);
    }
}
