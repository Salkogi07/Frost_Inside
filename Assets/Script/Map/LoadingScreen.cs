using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Image progressBar;
    public float minLoadTime = 2f; // �ּ� �ε� �ð� (��)

    void Start()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("GameScene");
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, progress, Time.deltaTime * 5f); // �ε巴��

            // �ּ� �ð� + �ε� �Ϸ�� ��쿡�� �� ��ȯ
            if (operation.progress >= 0.9f && timer >= minLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
