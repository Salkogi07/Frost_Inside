// ChatManager.cs
// 역할: 채팅 UI와 관련된 모든 기능을 전담합니다.

using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;
    
    [SerializeField] private TMP_InputField chatInput;
    
    [SerializeField] private Transform chatContent;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private GameObject chatMessagePrefab;
    
    [SerializeField] private int maxMessages = 250;

    private List<GameObject> messageObjects = new List<GameObject>();
    
    public bool IsChatting { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        IsChatting = chatInput.isFocused;
        
        if (Input.GetKeyDown(KeyCode.Return) && !IsChatting)
        {
            ToggleChatBox();
        }
    }
    
    private void ToggleChatBox()
    {
        if (chatInput.gameObject.activeSelf)
        {
            if (EventSystem.current.currentSelectedGameObject == chatInput.gameObject)
            {
                StartCoroutine(SendMessageAfterDelay());
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
            }
        }
    }
    
    private IEnumerator ScrollToBottomCoroutine()
    {
        // UI 요소가 재배치될 때까지 현재 프레임의 끝에서 대기
        yield return new WaitForEndOfFrame();

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    private IEnumerator SendMessageAfterDelay()
    {
        // IME 조합이 끝날 때까지 한 프레임 대기 (WaitForEndOfFrame)
        yield return new WaitForEndOfFrame();

        string messageToSend = chatInput.text;  // 확정된 한글 포함됨
        chatInput.text = "";

        EventSystem.current.SetSelectedGameObject(null);

        if (!string.IsNullOrWhiteSpace(messageToSend))
        {
            Debug.Log("메시지 전송: " + messageToSend);
            NetworkTransmission.instance.SendChatMessageServerRpc(messageToSend);
        }
    }

    public void AddMessage(string message, MessageType type)
    {
        if (messageObjects.Count >= maxMessages)
        {
            Destroy(messageObjects[0]);
            messageObjects.RemoveAt(0);
        }

        var newMsgObj = Instantiate(chatMessagePrefab, chatContent);
        var msgText = newMsgObj.GetComponent<TMP_Text>();
        msgText.text = message;
        
        switch (type)
        {
            case MessageType.PlayerMessage:
                // 기본값 (흰색, 보통)
                msgText.fontStyle = FontStyles.Normal;
                msgText.color = Color.white;
                break;
            case MessageType.GlobalSystem:
                // 전체 시스템 메시지 (노란색, 이탤릭)
                msgText.fontStyle = FontStyles.Normal;
                msgText.color = Color.yellow;
                break;
            case MessageType.PersonalSystem:
                // 개인 시스템 메시지 (하늘색, 이탤릭)
                msgText.fontStyle = FontStyles.Normal;
                msgText.color = Color.cyan;
                break;
            case MessageType.AdminSystem:
                // 관리자 메시지 (빨간색, 이탤릭)
                msgText.fontStyle = FontStyles.Italic;
                msgText.color = Color.red;
                break;
        }
        
        messageObjects.Add(newMsgObj);

        StartCoroutine(ScrollToBottomCoroutine());
    }
}