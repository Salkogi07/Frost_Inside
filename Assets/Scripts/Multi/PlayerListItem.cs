// --- START OF FILE PlayerListItem.cs ---

using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// 로비 내 각 플레이어의 정보를 표시하는 UI 아이템 클래스입니다.
/// </summary>
public class PlayerListItem : MonoBehaviour
{
    // 플레이어 정보
    public string PlayerName { get; set; }
    public SteamId PlayerSteamID { get; set; }
    public bool Ready { get; set; }
    public bool IsLobbyOwner { get; set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerStatusText;
    [SerializeField] private Image playerOwnerImage;
    [SerializeField] private RawImage playerAvatarImage;

    private bool avatarLoaded = false;

    /// <summary>
    /// 저장된 플레이어 정보로 UI 요소들을 업데이트합니다.
    /// </summary>
    public void SetPlayerValues()
    {
        playerNameText.text = PlayerName;
        playerOwnerImage.enabled = IsLobbyOwner;

        playerStatusText.text = Ready ? "Ready" : "Not Ready";
        playerStatusText.color = Ready ? Color.green : Color.red;
        
        // 아바타가 아직 로드되지 않았다면 비동기적으로 로드를 시작합니다.
        if (!avatarLoaded)
        {
            FetchAndSetAvatarAsync();
        }
    }
    
    /// <summary>
    /// Steamworks API를 통해 플레이어의 아바타를 비동기적으로 가져와 설정합니다.
    /// </summary>
    private async void FetchAndSetAvatarAsync()
    {
        if (PlayerSteamID.IsValid)
        {
            // SteamFriends.GetLargeAvatarAsync 사용하여 비동기 처리
            var avatarOpt = await SteamFriends.GetLargeAvatarAsync(PlayerSteamID);

            if (avatarOpt.HasValue)
            {
                // Steam 이미지를 Unity Texture2D로 변환
                Texture2D avatarTexture = ConvertSteamImageToTexture2D(avatarOpt.Value);
                if (avatarTexture != null)
                {
                    playerAvatarImage.texture = avatarTexture;
                    playerAvatarImage.gameObject.SetActive(true);
                    avatarLoaded = true; // 중복 로드 방지
                }
            }
            else
            {
                playerAvatarImage.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Steamworks 이미지 데이터를 Unity Texture2D로 변환합니다.
    /// </summary>
    private static Texture2D ConvertSteamImageToTexture2D(Steamworks.Data.Image image)
    {
        // RGBA32 형식의 새 텍스처 생성
        var texture = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.RGBA32, false, true);
        
        // 픽셀 데이터 로드 및 적용
        texture.LoadRawTextureData(image.Data);
        texture.Apply();

        return texture;
    }
}