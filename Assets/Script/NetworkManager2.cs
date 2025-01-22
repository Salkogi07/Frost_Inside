using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;

public class NetworkManager2 : MonoBehaviourPunCallbacks
{
    [Header("Disconnect")]
    public InputField NickNameInput;

    [Header("Lobby")]
    public GameObject Lobby;
    public InputField RoomInput;
    public Text LobbyInfoText;

    [Header("Room")]
    public GameObject Room;
    public Text ListText;
    public Text RoomInfoText;
    public Text[] chatText;
    public InputField ChatInput;

    [Header("ETC")]
    public Text StatusText;
    public PhotonView PV;

    public GoogleSheetManager gm;
    public GameObject register;
    private bool isFixedUpdateEnabled = true;

    List<RoomInfo> myList = new List<RoomInfo>();


    public void joinRoom(int num)
    {
        PhotonNetwork.JoinRoom(myList[num].Name);
    }


    #region 서버연결
    void Awake() => Screen.SetResolution(960, 540, false);

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = PhotonNetwork.CountOfPlayers+"명 접속중";
    }

    private void FixedUpdate()
    {
        if (isFixedUpdateEnabled)
        {
            if (gm.GD.msg == "로그인 완료")
            {
                login();
                isFixedUpdateEnabled = false;
            }
        }
    }

    public void login()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.JoinLobby();

        Lobby.SetActive(true);
        Room.SetActive(false);
        register.SetActive(false);

        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        myList.Clear();
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        Lobby.SetActive(false);
        Room.SetActive(false);
    }
    #endregion


    #region 방
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });

    public void LeaveRoom()
    {
        Lobby.SetActive(true);
        Room.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }


    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnJoinedRoom()
    {
        Lobby.SetActive(false);
        Room.SetActive(true);
        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < chatText.Length; i++) chatText[i].text = "";

        ChatRPC("<color=green>" + PhotonNetwork.NickName + "님이 방에 입장했습니다.</color>");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }
    #endregion


    #region 채팅
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < chatText.Length; i++)
            if (chatText[i].text == "")
            {
                isInput = true;
                chatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < chatText.Length; i++) chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }
    #endregion
}