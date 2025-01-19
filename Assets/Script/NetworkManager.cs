using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks // MonoBehaviourPunCallbacks�� ���
{
    public Text StatusText;
    public Text info;
    public InputField roomInput, NickName;

    void Awake() => Screen.SetResolution(960, 540, false);

    void Update() => StatusText.text = PhotonNetwork.NetworkClientState.ToString();

    //����
    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster()
    {
        print("�������ӿϷ�");
        PhotonNetwork.LocalPlayer.NickName = NickName.text;
    }

    // ����
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnDisconnected(DisconnectCause cause) => print("�������");

    // �κ�
    public void JoinLobby() => PhotonNetwork.JoinLobby();
    public override void OnJoinedLobby() => print("�κ�����");

    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 });
    public void JoinRoom() => PhotonNetwork.JoinRoom(roomInput.text);
    public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 }, null);
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnCreatedRoom() => print("�游���Ϸ�");
    public override void OnJoinedRoom() => print("�������Ϸ�");

    // �� ���� ���� �ݹ�
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("����������: " + message); // ���⼭ ���� �޽����� ó��
    }

    // �� ����� ���� �ݹ�
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("�游������: " + message); // ���� �޽��� ó��
    }

    [ContextMenu("����")]
    public void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("���� �� �̸� : " + PhotonNetwork.CurrentRoom.Name);
            print("���� �� �ο��� : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("���� �� �ִ� �ο��� : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerstr = "�濡 �ִ� �÷��̾� ��� : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerstr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerstr);

            info.text = "���� �� �̸� : " + PhotonNetwork.CurrentRoom.Name + "\n" + "���� �� �ο��� : " + PhotonNetwork.CurrentRoom.PlayerCount + "\n" + "���� �� �ִ� �ο��� : " + PhotonNetwork.CurrentRoom.MaxPlayers; 
        }
        else
        {
            print("������ �ο� �� : " + PhotonNetwork.CountOfPlayers);
            print("�� ���� : " + PhotonNetwork.CountOfRooms);
            print("��� �濡 �ִ� �ο� �� : " + PhotonNetwork.CountOfPlayersInRooms);
            print("�κ� �ִ���? : " + PhotonNetwork.InLobby);
            print("����ƴ���? : " + PhotonNetwork.IsConnected);
            info.text = "������ �ο� �� : " + PhotonNetwork.CountOfPlayers+"\n"+ "�� ���� : " + 
                PhotonNetwork.CountOfRooms+"\n"+ "��� �濡 �ִ� �ο� �� : " + PhotonNetwork.CountOfPlayersInRooms+"\n"+
                "�κ� �ִ���? : " + PhotonNetwork.InLobby+"\n"+ "����ƴ���? : " + PhotonNetwork.IsConnected;
        }
    }
}
