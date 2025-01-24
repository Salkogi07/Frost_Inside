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
        Flee // �߰����� �ൿ ����
    }

    public class Monster
    {
        // ������ ��ġ, ���� ��
        public int X { get; set; }
        public int Y { get; set; }
        public MonsterBehavior Behavior { get; set; }
        public int maxDetectingRange = 10;  // ���� �ִ� Ž�� ����
        public int minDetectingRange = 3;   // ���� �ּ� Ž�� ����

        // �÷��̾� ��ü, ���⼭�� Player Ŭ������ �̹� ���ǵ� ������ ����
        public Player Player { get; set; }

        // ���� ��ü�� Ŭ���� �������� �� ���� ����
        private static System.Random random = new System.Random();

        public void ChooseBehavior()
        {
            // �÷��̾���� �Ÿ��� ��������� behavior�� �������� ����

            // �Ÿ� ��� (�������� ���� �ʰ� �Ÿ� �������� ���� �� ����)
            int dx = Player.X - X;
            int dy = Player.Y - Y;
            int distanceSquared = dx * dx + dy * dy; // ������ �Ÿ�

            // ������ ������ ��
            int maxDetectingRangeSquared = maxDetectingRange * maxDetectingRange;
            int minDetectingRangeSquared = minDetectingRange * minDetectingRange;

            if (distanceSquared <= maxDetectingRangeSquared && distanceSquared > minDetectingRangeSquared)
            {
                // �ൿ�� �������� ���� (����: Move, Attack, Flee ��)
                Behavior = (MonsterBehavior)random.Next(1, Enum.GetValues(typeof(MonsterBehavior)).Length);
            }
            else
            {
                // �Ÿ��� �ָ� �̵� �ൿ�� �⺻���� ����
                Behavior = MonsterBehavior.Move;
            }

            // ���� Behavior�� �´� �Լ� ����
            switch (Behavior)
            {
                case MonsterBehavior.Move:
                    Move();
                    break;

                case MonsterBehavior.Attack:
                    Attack();
                    break;

                case MonsterBehavior.Flee:
                    Flee();  // �߰��� �ൿ ����
                    break;

                default:
                    Program.WarningLog($"�� �� ���� MonsterBehavior: {Enum.GetName(typeof(MonsterBehavior), Behavior)}");
                    break;
            }

            // �� �ý����� ����
            TurnSystem.ToggleIsPlayerTurn();
        }

        // �ൿ: �̵�
        private void Move()
        {
            // �̵� ���� ���� (���÷� �ܼ��� ���� �������� �̵�)
            Console.WriteLine("���Ͱ� �̵��մϴ�.");
            // X, Y ���� ������Ʈ�ϴ� ������ �߰�
        }

        // �ൿ: ����
        private void Attack()
        {
            // ���� ���� ����
            Console.WriteLine("���Ͱ� �����մϴ�.");
            // ���ݿ� ���� ������ �߰�
        }

        // �ൿ: ����
        private void Flee()
        {
            // ���� ���� ����
            Console.WriteLine("���Ͱ� ����Ĩ�ϴ�.");
            // ������ ���� ������ �߰�
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
            // ���÷� ���Ϳ� �÷��̾� ����
            Player player = new Player { X = 5, Y = 5 };
            Monster monster = new Monster { X = 0, Y = 0, Player = player };

            // ������ �ൿ�� ����
            monster.ChooseBehavior();
        }
    }

    public static class TurnSystem
    {
        // �÷��̾��� ���� �����ϴ� �ý���
        public static void ToggleIsPlayerTurn()
        {
            Console.WriteLine("�÷��̾� ������ ��ȯ�Ǿ����ϴ�.");
        }
    }
}

