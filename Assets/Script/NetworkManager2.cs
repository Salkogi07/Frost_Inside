using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager2 : MonoBehaviourPunCallbacks
{
    private NetworkManager2 s_instance;
    public NetworkManager2 Instance { get { return s_instance; } }

    [Header("Disconnect")]
    public InputField NickNameInput;

    [Header("Lobby")]
    public GameObject Lobby;
    public InputField RoomInput;
    public Text LobbyInfoText;
    public GameObject roomItemPrefab; // �� ��� ������ ������
    public Transform roomListContent; // �� ����� ǥ�õ� �θ� Transform

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

    private Dictionary<string, GameObject> roomItems = new Dictionary<string, GameObject>();

    public void joinRoom(int num)
    {
        // �� �Լ��� �� �̻� ������ �����Ƿ� �����ϰų� �����ؾ� �մϴ�.
        // OnRoomListUpdate���� ��ư Ŭ�� �� ���� JoinRoom�� ȣ���ϵ��� ����Ǿ����ϴ�.
        Debug.LogWarning("joinRoom �Լ��� �� �̻� ������ �ʽ��ϴ�.");
    }

    #region ��������
    void Awake() => Screen.SetResolution(960, 540, false);

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = PhotonNetwork.CountOfPlayers + "�� ������";
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
        base.OnConnectedToMaster();
        Debug.Log("�κ� ������ �õ��մϴ�.");

        Lobby.SetActive(true);
        Room.SetActive(false);
        register.SetActive(false);

        PhotonNetwork.NickName = NickNameInput.text;
        //PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    }


    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        Lobby.SetActive(false);
        Room.SetActive(false);
    }
    #endregion

    #region ��
    public void CreateRoom()
    {
        string roomName = RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 4 });
        Debug.Log($"�� ���� �õ�: {roomName}");
    }


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

    [PunRPC]
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
        if (!isInput)
        {
            for (int i = 1; i < chatText.Length; i++) chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }
    #endregion

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"OnRoomListUpdate ȣ���! �κ� ����: {PhotonNetwork.InLobby}");

        foreach (RoomInfo room in roomList)
        {
            Debug.Log($"�� �̸�: {room.Name}, �ο�: {room.PlayerCount}/{room.MaxPlayers}, ���� ����: {room.RemovedFromList}");

            // �� ����
            if (room.RemovedFromList)
            {
                if (roomItems.ContainsKey(room.Name))
                {
                    Debug.Log($"UI���� ���ŵ� ��: {room.Name}");
                    Destroy(roomItems[room.Name]);
                    roomItems.Remove(room.Name);
                }
                continue;
            }

            // ���� �� ������Ʈ
            if (roomItems.ContainsKey(room.Name))
            {
                Text roomNameText = roomItems[room.Name].GetComponentInChildren<Text>();
                if (roomNameText != null)
                {
                    roomNameText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
                    Debug.Log($"�� ������Ʈ: {room.Name}");
                }
            }
            else
            {
                // �� �� ����
                GameObject roomItem = Instantiate(roomItemPrefab, roomListContent);
                Text roomNameText = roomItem.GetComponentInChildren<Text>();
                roomNameText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
                roomItems.Add(room.Name, roomItem);

                Debug.Log($"���ο� �� ����: {room.Name}");

                // ��ư �̺�Ʈ �߰�
                Button joinButton = roomItem.GetComponent<Button>();
                joinButton.onClick.AddListener(() =>
                {
                    Debug.Log($"�� ���� �õ�: {room.Name}");
                    PhotonNetwork.JoinRoom(room.Name);
                });
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon ������ ����Ǿ����ϴ�.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ���������� �����߽��ϴ�.");
        Debug.Log($"���� �κ� ����: {PhotonNetwork.InLobby}");
    }


    public override void OnLeftLobby()
    {
        Debug.Log("�κ񿡼� �������ϴ�.");
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
}