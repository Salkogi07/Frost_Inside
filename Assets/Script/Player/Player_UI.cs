using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    public float invincibilityDuration = 0.5f;

    Player_Move playerMove;
    SpriteRenderer sprite;
    Rigidbody2D rb;

    public bool isInvincible  = false;

    [Header("Component")]
    [SerializeField] Image staminaImage;

    [Header("Hp info")]
    [SerializeField] private float hp = 0;
    [SerializeField] private float maxHp = 100;

    [Header("Stamina info")]
    [SerializeField] public float stamina = 0;
    [SerializeField] public float maxStamina = 100;

    private void Awake()
    {
        playerMove = GetComponent<Player_Move>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        float StaminaValue = stamina / maxStamina;
        staminaImage.fillAmount = StaminaValue;
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
