using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLoadingManager : MonoBehaviour
{
    public static TestLoadingManager instance;

    public float minLoadTime = 2f;

    [SerializeField] public GameObject LoadScreenObj;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadGameScene(sceneName));
    }

    private IEnumerator LoadGameScene(string sceneName)
    {
        LoadScreenObj.SetActive(true);
        animator.Play("Shader_Out");

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;
            
            if (operation.progress >= 0.9f && timer >= minLoadTime)
            {
                operation.allowSceneActivation = true;
                animator.Play("Shader_In");
            }

            yield return null;
        }
    }
}