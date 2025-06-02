using UnityEngine;

public class LoadingObject : MonoBehaviour
{
    [SerializeField] private GameObject LoadingObj;

    public void OffScreen()
    {
        gameObject.SetActive(false);
    }

    public void OnObject()
    {
        LoadingObj.SetActive(true);
    }

    public void OffObject()
    {
        LoadingObj.SetActive(false);
    }
}
