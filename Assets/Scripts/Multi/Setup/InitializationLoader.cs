using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Steamworks;
using TMPro;
using UnityEngine.UI;

public class InitializationLoader : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("오류 발생 시 활성화할 경고 패널")]
    [SerializeField] private GameObject warningPanel;

    [Tooltip("경고 패널에 표시할 오류 메시지 텍스트")]
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Button quitButton;

    [Header("Steam 설정")]
    [Tooltip("개발 중에는 테스트용으로 480 (Spacewar)을 사용할 수 있습니다.")]
    [SerializeField] private uint appId = 480;
    
    [Header("로딩 설정")]
    [Tooltip("최소 로딩 시간(초)을 설정합니다. 초기화가 이 시간보다 빨리 끝나도 이 시간만큼 기다립니다.")]
    [SerializeField] private float minimumLoadTime = 2.0f; 

    // 스팀 초기화 성공 여부를 저장하는 정적 변수
    public static bool IsSteamInitialized { get; private set; }

    private void Start()
    {
        quitButton.onClick.AddListener(QuitGame);
        
        // 시작 시 경고 패널을 비활성화합니다.
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Warning Panel이 인스펙터에 할당되지 않았습니다!");
        }

        // 초기화 및 씬 로딩 코루틴을 시작합니다.
        StartCoroutine(InitializationSequence());
    }

    private IEnumerator InitializationSequence()
    {
        float startTime = Time.time;

        // --- 1단계: 스팀 초기화 시도 ---
        IsSteamInitialized = false;
        if (SteamClient.IsValid)
        {
            // 이미 유효한 경우, 초기화가 성공한 것으로 간주
            Debug.Log("Steamworks가 이미 초기화되어 있습니다.");
            IsSteamInitialized = true;
        }
        else
        {
            // 초기화되지 않은 경우에만 Init() 시도
            try
            {
                SteamClient.Init(appId);
                IsSteamInitialized = true;
                Debug.Log("Steamworks가 성공적으로 초기화되었습니다!");
                Debug.Log($"환영합니다, {SteamClient.Name} ({SteamClient.SteamId})");
            }
            catch (System.Exception e)
            {
                // 스팀 초기화 실패 시 (Steam 미실행 등)
                string errorMessage = $"Steam 초기화에 실패했습니다.\nSteam이 실행 중인지 확인해주세요.\n\n<size=18>오류: {e.Message}</size>";
                HandleFailure(errorMessage);
                yield break;
            }
        }
        
        // --- 2단계: NetworkManager 초기화 대기 ---
        Debug.Log("NetworkManager.Singleton을 기다리는 중...");
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        Debug.Log("NetworkManager.Singleton이 준비되었습니다.");
        
        // --- 3단계: 최소 로딩 시간 보장 ---
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumLoadTime)
        {
            yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
        }
        Debug.Log($"총 로딩 시간: {Time.time - startTime:F2}초");
        
        // --- 4단계: 메인 메뉴 씬 로드 ---
        Debug.Log("메뉴 씬을 로드합니다.");
        SceneManager.LoadScene("Menu");

    }

    private void HandleFailure(string message)
    {
        Debug.LogError(message);

        if (warningPanel != null)
        {
            if (warningText != null)
            {
                warningText.text = message;
            }
            else
            {
                Debug.LogError("Warning Text가 인스펙터에 할당되지 않았습니다!");
            }
            warningPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("종료 버튼이 클릭되었습니다. 애플리케이션을 종료합니다.");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}