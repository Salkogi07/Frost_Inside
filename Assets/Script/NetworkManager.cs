using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks // MonoBehaviourPunCallbacks를 상속
{
    public Text StatusText;
    public Text info;
    public InputField roomInput, NickName;

    void Awake() => Screen.SetResolution(960, 540, false);

    void Update() => StatusText.text = PhotonNetwork.NetworkClientState.ToString();

    //연결
    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster()
    {
        print("서버접속완료");
        PhotonNetwork.LocalPlayer.NickName = NickName.text;
    }

    // 끊기
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnDisconnected(DisconnectCause cause) => print("연결끊김");

    // 로비
    public void JoinLobby() => PhotonNetwork.JoinLobby();
    public override void OnJoinedLobby() => print("로비접속");

    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 });
    public void JoinRoom() => PhotonNetwork.JoinRoom(roomInput.text);
    public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 }, null);
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnCreatedRoom() => print("방만들기완료");
    public override void OnJoinedRoom() => print("방참가완료");

    // 방 참가 실패 콜백
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("방참가실패: " + message); // 여기서 실패 메시지를 처리
    }

    // 방 만들기 실패 콜백
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("방만들기실패: " + message); // 실패 메시지 처리
    }

    [ContextMenu("정보")]
    public void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대 인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerstr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerstr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerstr);

            info.text = "현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name + "\n" + "현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount + "\n" + "현재 방 최대 인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers; 
        }
        else
        {
            print("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("로비에 있는지? : " + PhotonNetwork.InLobby);
            print("연결됐는지? : " + PhotonNetwork.IsConnected);
            info.text = "접속한 인원 수 : " + PhotonNetwork.CountOfPlayers+"\n"+ "방 개수 : " + 
                PhotonNetwork.CountOfRooms+"\n"+ "모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms+"\n"+
                "로비에 있는지? : " + PhotonNetwork.InLobby+"\n"+ "연결됐는지? : " + PhotonNetwork.IsConnected;
        }
    }
}
