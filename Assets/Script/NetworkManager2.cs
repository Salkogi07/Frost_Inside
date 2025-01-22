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


    #region ��������
    void Awake() => Screen.SetResolution(960, 540, false);

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = PhotonNetwork.CountOfPlayers+"�� ������";
    }

    private void FixedUpdate()
    {
        if (isFixedUpdateEnabled)
        {
            if (gm.GD.msg == "�α��� �Ϸ�")
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


    #region ��
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

        ChatRPC("<color=green>" + PhotonNetwork.NickName + "���� �濡 �����߽��ϴ�.</color>");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "�� / " + PhotonNetwork.CurrentRoom.MaxPlayers + "�ִ�";
    }
    #endregion


    #region ä��
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
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
        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < chatText.Length; i++) chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }
    #endregion
}