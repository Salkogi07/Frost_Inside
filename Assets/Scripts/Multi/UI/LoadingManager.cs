using UnityEngine;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LoadingManager : NetworkBehaviour
{
    public static LoadingManager instance;

    [Header("Objects")]
    [SerializeField] private GameObject loadingScreenObj;
    [SerializeField] private Animator animator;

    private readonly int hashShaderOut = Animator.StringToHash("Shader_Out");
    private readonly int hashShaderIn = Animator.StringToHash("Shader_In");

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            loadingScreenObj.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }
        
        // 클라이언트 본인이 정상적으로 방에 입장할 때 수동 처리
        if(!IsServer || !IsHost)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
        
        // 콜백 해제
        if(!IsServer || !IsHost)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }

    /// <summary>
    /// (호스트 전용) 네트워크 씬 로딩을 시작합니다.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!IsServer) return;
        
        StartCoroutine(HostLoadSceneCoroutine(sceneName));
    }

    private IEnumerator HostLoadSceneCoroutine(string sceneName)
    {
        loadingScreenObj.SetActive(true);
        animator.Play(hashShaderOut);
        yield return new WaitForSeconds(GetAnimationLength("Shader_Out"));

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        Debug.Log($"[LoadingManager] SceneEvent received: {sceneEvent.SceneEventType}");
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
            case SceneEventType.Synchronize:
                if (!IsServer)
                {
                    ShowLoadingScreen();
                }
                break;

            case SceneEventType.LoadEventCompleted:
                StartCoroutine(FadeInCoroutine());
                break;
        }
    }

    public void ShowLoadingScreen()
    {
        loadingScreenObj.SetActive(true);
        animator.Play(hashShaderOut);
    }
    
    private IEnumerator FadeInCoroutine()
    {
        animator.Play(hashShaderIn);
        yield return new WaitForSeconds(GetAnimationLength("Shader_In"));
        
        loadingScreenObj.SetActive(false);
    }
    
    private float GetAnimationLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 1f;
        
        return animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == clipName)?.length ?? 1f;
    }
}