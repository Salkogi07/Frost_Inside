using System.Collections;
using UnityEngine;

public class SnowCrusier_Open : MonoBehaviour
{
    [SerializeField] private float waitSecond;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        yield return new WaitForSeconds(waitSecond);
        anim.Play("open");
    }
    
    public void Close()
    {
        anim.Play("close");
    }

    public void Move()
    {
        anim.Play("move");
    }
}
