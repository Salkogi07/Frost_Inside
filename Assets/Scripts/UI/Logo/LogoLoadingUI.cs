using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoLoadingUI : MonoBehaviour
{
    // 인스펙터에서 설정할 변수들
    [Tooltip("페이드 인/아웃될 로고 이미지")]
    [SerializeField] private Image logoImage;
    
    [Tooltip("로고가 페이드인까지 기다리는 시간")]
    [SerializeField] private float startTime = 1.0f;
    
    [Tooltip("로고가 나타나는 데 걸리는 시간")]
    [SerializeField] private float fadeInTime = 2.0f;

    [Tooltip("로고가 완전히 나타난 후 유지되는 시간")]
    [SerializeField] private float stayTime = 2.0f;

    [Tooltip("로고가 사라지는 데 걸리는 시간")]
    [SerializeField] private float fadeOutTime = 2.0f;

    [Tooltip("로고 씬 이후에 불러올 씬의 이름")]
    [SerializeField] private string nextSceneName = "Setup";

    void Start()
    {
        // 코루틴을 시작합니다.
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        logoImage.gameObject.SetActive(false);
        yield return new WaitForSeconds(startTime);
        logoImage.gameObject.SetActive(true);
        
        // 2. Fade In (서서히 나타나기)
        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));

        // 3. Stay (잠시 대기)
        yield return new WaitForSeconds(stayTime);

        // 4. Fade Out (서서히 사라지기)
        yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));

        // 5. 다음 씬으로 이동
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// 이미지의 알파값을 조절하여 페이드 효과를 주는 코루틴
    /// </summary>
    /// <param name="startAlpha">시작 알파값 (0~1)</param>
    /// <param name="endAlpha">목표 알파값 (0~1)</param>
    /// <param name="duration">지속 시간</param>
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color currentColor = logoImage.color;

        while (timer < duration)
        {
            // 경과 시간에 따라 알파값 계산 (선형 보간)
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            
            // 이미지의 색상에 새로운 알파값 적용
            logoImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            
            // 타이머 업데이트
            timer += Time.deltaTime;

            // 다음 프레임까지 대기
            yield return null;
        }

        // 페이드가 끝난 후 목표 알파값으로 정확히 설정
        logoImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, endAlpha);
    }
}
