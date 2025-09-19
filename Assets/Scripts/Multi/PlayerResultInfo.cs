// PlayerResultInfo.cs
using Unity.Netcode;
using Unity.Collections;

// 플레이어의 최종 상태를 정의하는 열거형
public enum PlayerStatus : byte
{
    Survived, // 생존 (크루저에 탑승)
    Missing,  // 실종 (살아있지만 크루저에 없음)
    Deceased  // 사망
}

// 네트워크를 통해 동기화될 결산 정보 구조체
public struct PlayerResultInfo : INetworkSerializable
{
    public ulong ClientId;
    public FixedString64Bytes SteamName; // 네트워크 전송을 위해 FixedString 사용
    public PlayerStatus Status;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref SteamName);
        serializer.SerializeValue(ref Status);
    }
}