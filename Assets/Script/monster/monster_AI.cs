using System;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.Random;

public class monster_AI : MonoBehaviour
{
    public enum MonsterBehavior
    {
        Move,
        Attack,
        Flee // 추가적인 행동 예시
    }

    public class Monster
    {
        // 몬스터의 위치, 범위 등
        public int X { get; set; }
        public int Y { get; set; }
        public MonsterBehavior Behavior { get; set; }
        public int maxDetectingRange = 10;  // 예시 최대 탐지 범위
        public int minDetectingRange = 3;   // 예시 최소 탐지 범위

        // 플레이어 객체, 여기서는 Player 클래스가 이미 정의된 것으로 가정
        public Player Player { get; set; }

        // 랜덤 객체를 클래스 레벨에서 한 번만 생성
        private static System.Random random = new System.Random();

        public void ChooseBehavior()
        {
            // 플레이어와의 거리가 가까워지면 behavior를 랜덤으로 설정

            // 거리 계산 (제곱근을 쓰지 않고 거리 제곱으로 비교할 수 있음)
            int dx = Player.X - X;
            int dy = Player.Y - Y;
            int distanceSquared = dx * dx + dy * dy; // 제곱된 거리

            // 제곱된 범위로 비교
            int maxDetectingRangeSquared = maxDetectingRange * maxDetectingRange;
            int minDetectingRangeSquared = minDetectingRange * minDetectingRange;

            if (distanceSquared <= maxDetectingRangeSquared && distanceSquared > minDetectingRangeSquared)
            {
                // 행동을 랜덤으로 결정 (예시: Move, Attack, Flee 등)
                Behavior = (MonsterBehavior)random.Next(1, Enum.GetValues(typeof(MonsterBehavior)).Length);
            }
            else
            {
                // 거리가 멀면 이동 행동을 기본으로 설정
                Behavior = MonsterBehavior.Move;
            }

            // 현재 Behavior에 맞는 함수 실행
            switch (Behavior)
            {
                case MonsterBehavior.Move:
                    Move();
                    break;

                case MonsterBehavior.Attack:
                    Attack();
                    break;

                case MonsterBehavior.Flee:
                    Flee();  // 추가된 행동 예시
                    break;

                default:
                    Program.WarningLog($"알 수 없는 MonsterBehavior: {Enum.GetName(typeof(MonsterBehavior), Behavior)}");
                    break;
            }

            // 턴 시스템을 제어
            TurnSystem.ToggleIsPlayerTurn();
        }

        // 행동: 이동
        private void Move()
        {
            // 이동 로직 구현 (예시로 단순히 일정 방향으로 이동)
            Console.WriteLine("몬스터가 이동합니다.");
            // X, Y 값을 업데이트하는 로직을 추가
        }

        // 행동: 공격
        private void Attack()
        {
            // 공격 로직 구현
            Console.WriteLine("몬스터가 공격합니다.");
            // 공격에 대한 로직을 추가
        }

        // 행동: 도망
        private void Flee()
        {
            // 도망 로직 구현
            Console.WriteLine("몬스터가 도망칩니다.");
            // 도망을 위한 로직을 추가
        }
    }

    public class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public static class Program
    {
        public static void WarningLog(string message)
        {
            Console.WriteLine("Warning: " + message);
        }

        public static void Main()
        {
            // 예시로 몬스터와 플레이어 생성
            Player player = new Player { X = 5, Y = 5 };
            Monster monster = new Monster { X = 0, Y = 0, Player = player };

            // 몬스터의 행동을 선택
            monster.ChooseBehavior();
        }
    }

    public static class TurnSystem
    {
        // 플레이어의 턴을 관리하는 시스템
        public static void ToggleIsPlayerTurn()
        {
            Console.WriteLine("플레이어 턴으로 전환되었습니다.");
        }
    }
}

