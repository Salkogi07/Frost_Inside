using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    [SerializeField] protected float Maxhealth;
    [SerializeField] protected bool isDead = false;


    public virtual void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }
        ReduceHp(damage);
    }

    private void ReduceHp(float damage)
    {
        Maxhealth -= damage;

        if (Maxhealth < 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("뻐@거");
    }


}
