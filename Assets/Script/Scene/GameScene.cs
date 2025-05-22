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
        LoadingManager.instance.LoadScreenObj.GetComponent<Image>().material = mal;
    }

    private void Start()
    {
        StartCoroutine(StartLoad());
    }

    IEnumerator StartLoad()
    {
       
        timeline.Play();

        yield return new WaitWhile(() => timeline.state == PlayState.Playing);
        Debug.Log("Timeline 재생 종료");

        LoadingManager.instance.LoadScene("GameScene");
    }
}
