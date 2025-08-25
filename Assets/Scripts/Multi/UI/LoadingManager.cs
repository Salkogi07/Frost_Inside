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
    
    [SerializeField] private GameObject backgroundObj;
    [SerializeField] private GameObject imageObj;

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
        if (NetworkManager.Singleton?.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }
        
        if(!IsServer || !IsHost)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton?.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
        
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

    // NetworkTransmission에서 직접 호출될 함수
    public void ShowLoadingScreen()
    {
        backgroundObj.SetActive(true);
        imageObj.SetActive(true);
        loadingScreenObj.SetActive(true);
        animator.Play(hashShaderOut);
    }
    
    // NetworkTransmission에서 직접 호출될 함수
    public void HideLoadingScreen()
    {
        StartCoroutine(FadeInCoroutine());
    }

    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        // 이제 이 이벤트 핸들러는 오직 '로컬 클라이언트의 로딩이 끝났음'을 감지하는 역할만 합니다.
        if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            Debug.Log($"[Client {sceneEvent.ClientId}] Scene load complete.");
            Debug.Log($"Scene Client : {sceneEvent.ClientId} Scene Loacal: {NetworkManager.Singleton.LocalClientId}");
            // 이 이벤트는 각 클라이언트 자신에게만 발생합니다.
            if (sceneEvent.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log($"[Client {sceneEvent.ClientId}] Scene load complete. Notifying server.");
                // 서버에게 "나 로딩 끝났어요" 라고 보고합니다.
                NetworkTransmission.instance.NotifyServerSceneLoadedServerRpc();
            }
        }
    }
    
    private IEnumerator FadeInCoroutine()
    {
        animator.Play(hashShaderIn);
        yield return new WaitForSeconds(GetAnimationLength("Shader_In"));
        loadingScreenObj.SetActive(false);
    }

    public void AnimLate(string clipName, string sceneName) => StartCoroutine(AnimLateCoroutine(clipName, sceneName));

    private IEnumerator AnimLateCoroutine(string clipName, string sceneName)
    {
        yield return new WaitForSeconds(GetAnimationLength(clipName));
        
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    
    private float GetAnimationLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 1f;
        
        return animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == clipName)?.length ?? 1f;
    }
}