using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    public Material mal;
    public PlayableDirector timeline;

    private void Awake()
    {
        TestLoadingManager.instance.LoadScreenObj.GetComponent<Image>().material = mal;
    }

    private void Start()
    {
        StartCoroutine(StartLoad());
    }

    IEnumerator StartLoad()
    {
       
        timeline.Play();

        yield return new WaitWhile(() => timeline.state == PlayState.Playing);
        Debug.Log("Timeline ��� ����");

        TestLoadingManager.instance.LoadScene("GameScene");
    }
}
