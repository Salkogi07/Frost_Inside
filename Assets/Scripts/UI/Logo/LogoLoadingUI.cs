using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoLoadingUI : MonoBehaviour
{
    [SerializeField] private PlayableDirector logoDirector;
    
    // 인스펙터에서 설정할 변수들
    [Tooltip("페이드 인/아웃될 로고 이미지")]
    [SerializeField] private Image logoImage;
    
    [Tooltip("로고가 페이드인까지 기다리는 시간")]
    [SerializeField] private float startTime = 1.0f;

    [Tooltip("로고가 나타나는 데 걸리는 시간")]
    [SerializeField] private float fadeInTime = 2.0f;

    [Tooltip("로고 씬 이후에 불러올 씬의 이름")]
    [SerializeField] private string nextSceneName = "Setup";
    
    [Header("로고 스킵")]
    [SerializeField] private bool skipLogo = false;
    
    void Start()
    {
        if(skipLogo)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            StartCoroutine(FullSequence());
        }
    }

    private IEnumerator FullSequence()
    {
        logoDirector.gameObject.SetActive(false);
        yield return new WaitForSeconds(startTime);
        logoDirector.gameObject.SetActive(true);
        
        // PlayableDirector 재생 및 완료 대기
        if (logoDirector != null)
        {
            logoDirector.Play();
            // 타임라인 재생 시간만큼 대기 (타임라인 길이를 가져와야 함)
            yield return new WaitForSeconds((float)logoDirector.duration);
        }
        
        // 로고 페이드 인
        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));
        
        // 다음 씬 로드
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
        logoImage.gameObject.SetActive(true); // 페이드 시작 전에 항상 활성화
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