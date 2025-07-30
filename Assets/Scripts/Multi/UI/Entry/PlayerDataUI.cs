// PlayerItemDataUI.cs
// 역할: 로비에 있는 개별 플레이어의 정보를 시각적으로 표시하는 UI 프리팹 스크립트.

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using TMPro;
using UnityEngine.UI;
using Steamworks;
using Unity.Netcode;

public class PlayerDataUI : MonoBehaviour
{
    [SerializeField] private RawImage profileImage;
    [SerializeField] private Image playerOwnerImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text readyCheckText;
    [SerializeField] public Button kickButton;
    //[SerializeField] private TMP_Text selectedCharacterText; // 캐릭터 이름 표시

    public void Setup(PlayerInfo info, bool isHostPlayer)
    {
        kickButton.onClick.AddListener(() => KickBanPanelUI.instance.OpenPanel(info.ClientId, info.SteamName));
        UpdateUI(info, isHostPlayer);
        LoadAvatar(info.SteamId);
    }
    
    public void UpdateUI(PlayerInfo info, bool isHostPlayer)
    {
        playerNameText.text = info.SteamName;
        
        playerOwnerImage.enabled = isHostPlayer;

        if (NetworkManager.Singleton.IsHost)
        {
            kickButton.gameObject.SetActive(!isHostPlayer);
        }
        
        readyCheckText.text = info.IsReady ? "Ready" : "Not Ready";
        readyCheckText.color = info.IsReady ? Color.green : Color.red;
        
        /*if (info.SelectedCharacterId == -1)
        {
            selectedCharacterText.text = "Choosing...";
            selectedCharacterText.color = UnityEngine.Color.gray;
        }
        else
        {
            // TODO: 실제 캐릭터 ID에 맞는 이름으로 변경 (예: CharacterDatabase.GetName(info.SelectedCharacterId))
            selectedCharacterText.text = $"Character {info.SelectedCharacterId + 1}";
            selectedCharacterText.color = UnityEngine.Color.white;
        }*/
    }
    
    private async void LoadAvatar(ulong steamId)
    {
        var avatar = await SteamFriends.GetLargeAvatarAsync(steamId);
        if (avatar.HasValue && this != null) // 오브젝트가 파괴되지 않았는지 확인
        {
            profileImage.texture = ConvertSteamImageToTexture2D(avatar.Value);
            profileImage.color = Color.white;
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