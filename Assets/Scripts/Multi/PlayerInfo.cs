// PlayerInfo.cs
// 역할: 플레이어의 데이터를 담는 순수 C# 클래스(DTO)입니다. MonoBehaviour가 아닙니다.

using System;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    public ulong ClientId;
    public ulong SteamId;
    public string SteamName;
    public bool IsReady;
    public int SelectedCharacterId = 0; // 0은 선택되지 않음을 의미
    public bool IsDead = false;
}