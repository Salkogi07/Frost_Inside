using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public GameObject playerObject;
    public Player_Stats playerStats;
    public Player_Move playerMove;
    public Player_ItemDrop playerDrop;

    public CinemachineCamera cam;

    public float temperatureDropRate = 1f;
    public int hpDropRate = 2;
    private float damageTimer = 0f;

    public bool isNearHeatSource = false; // �� ��ó�� �ִ��� üũ
    public bool isInColdZone = false; // �߿� �������� üũ

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(instance.gameObject);
    }

    private void Update()
    {
        HandleTemperature();
        HandleHp();
    }

    public void SettingCam()
    {
        Transform playerTransform = playerObject.transform;

        if (playerTransform != null)
        {
            cam.Follow = playerTransform;
            cam.LookAt = playerTransform;
        }
        else
        {
            Debug.LogWarning("Player�� ã�� �� �����ϴ�. �±� Ȯ�� �ʿ�.");
        }
    }

    private void HandleTemperature()
    {
        float dropRate = temperatureDropRate;

        if (playerMove.moveInput != 0)
            dropRate *= 0.5f; // �����̸� ������ ���̱�

        if (playerMove.isSprinting)
            dropRate *= 0.25f; // �ٰ� ������ �µ� ������ �� ���߱�

        if (playerMove.isAttack)
            dropRate *= 0.5f; // ���� �߿��� ������ ������

        playerStats.temperature -= dropRate * Time.deltaTime;
        playerStats.temperature = Mathf.Max(playerStats.GetTemperature(), 0f);
    }

    private void HandleHp()
    {
        float tempRatio = playerStats.GetTemperature();

        if (tempRatio == 0)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 1f)
            {
                playerStats.TakeDamage(hpDropRate);
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }
}