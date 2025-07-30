using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyFriendDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyOwnerNameText;
    [SerializeField] private RawImage lobbyOwnerImage;
    [SerializeField] private Button joinButton;
    
    private ulong lobbyId;

    public void Setup(ulong _lobbyId, ulong _lobbyOwnerSteamId, string _lobbyOwnerName)
    {
        lobbyId = _lobbyId;
        lobbyOwnerNameText.text = _lobbyOwnerName;
        LoadAvatar(_lobbyOwnerSteamId);
        joinButton.onClick.AddListener(JoinLobby);
    }
    
    private void JoinLobby()
    {
        GameNetworkManager.instance.JoinLobbyWithID(lobbyId);
        Debug.Log("JoinLobbyWithID: " + lobbyId + "");
    }
    
    private async void LoadAvatar(ulong steamId)
    {
        var avatar = await SteamFriends.GetLargeAvatarAsync(steamId);
        if (avatar.HasValue && this != null) // 오브젝트가 파괴되지 않았는지 확인
        {
            lobbyOwnerImage.texture = ConvertSteamImageToTexture2D(avatar.Value);
            lobbyOwnerImage.color = Color.white;
        }
    }

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