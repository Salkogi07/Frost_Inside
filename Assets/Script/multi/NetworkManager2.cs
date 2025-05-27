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
    public GameObject roomItemPrefab; // 방 목록 아이템 프리팹
    public Transform roomListContent; // 방 목록이 표시될 부모 Transform

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
        // 이 함수는 더 이상 사용되지 않으므로 제거하거나 수정해야 합니다.
        // OnRoomListUpdate에서 버튼 클릭 시 직접 JoinRoom을 호출하도록 변경되었습니다.
        Debug.LogWarning("joinRoom 함수는 더 이상 사용되지 않습니다.");
    }

    #region 서버연결
    void Awake() => Screen.SetResolution(960, 540, false);

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = PhotonNetwork.CountOfPlayers + "명 접속중";
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
        Debug.Log("로비에 접속을 시도합니다.");

        Lobby.SetActive(true);
        Room.SetActive(false);
        register.SetActive(false);

        //PhotonNetwork.NickName = NickNameInput.text;
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    }


    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        Lobby.SetActive(false);
        Room.SetActive(false);
    }
    #endregion

    #region 방
    public void CreateRoom()
    {
        string roomName = RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 4 });
        Debug.Log($"방 생성 시도: {roomName}");
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

        ChatRPC("<color=green>" + PhotonNetwork.NickName + "님이 방에 입장했습니다.</color>");
    }

    /*public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }*/

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
        Debug.Log($"OnRoomListUpdate 호출됨! 로비 상태: {PhotonNetwork.InLobby}");

        foreach (RoomInfo room in roomList)
        {
            Debug.Log($"방 이름: {room.Name}, 인원: {room.PlayerCount}/{room.MaxPlayers}, 삭제 여부: {room.RemovedFromList}");

            // 방 제거
            if (room.RemovedFromList)
            {
                if (roomItems.ContainsKey(room.Name))
                {
                    Debug.Log($"UI에서 제거될 방: {room.Name}");
                    Destroy(roomItems[room.Name]);
                    roomItems.Remove(room.Name);
                }
                continue;
            }

            // 기존 방 업데이트
            if (roomItems.ContainsKey(room.Name))
            {
                Text roomNameText = roomItems[room.Name].GetComponentInChildren<Text>();
                if (roomNameText != null)
                {
                    roomNameText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
                    Debug.Log($"방 업데이트: {room.Name}");
                }
            }
            else
            {
                // 새 방 생성
                GameObject roomItem = Instantiate(roomItemPrefab, roomListContent);
                Text roomNameText = roomItem.GetComponentInChildren<Text>();
                roomNameText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
                roomItems.Add(room.Name, roomItem);

                Debug.Log($"새로운 방 생성: {room.Name}");

                // 버튼 이벤트 추가
                Button joinButton = roomItem.GetComponent<Button>();
                joinButton.onClick.AddListener(() =>
                {
                    Debug.Log($"방 입장 시도: {room.Name}");
                    PhotonNetwork.JoinRoom(room.Name);
                });
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 서버에 연결되었습니다.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비에 성공적으로 접속했습니다.");
        Debug.Log($"현재 로비 상태: {PhotonNetwork.InLobby}");
    }


    public override void OnLeftLobby()
    {
        Debug.Log("로비에서 나갔습니다.");
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