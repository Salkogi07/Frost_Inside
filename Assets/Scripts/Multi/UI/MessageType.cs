public enum MessageType
{
    PlayerMessage,  // 일반 플레이어 채팅
    GlobalSystem,   // 모든 플레이어에게 보이는 시스템 메시지 (입장/퇴장 등)
    PersonalSystem,  // 나에게만 보이는 시스템 메시지 (로비 참가 완료 등)
    AdminSystem // [추가] 관리자(킥/밴) 메시지 타입
}