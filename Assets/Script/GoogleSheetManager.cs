using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;



[System.Serializable]
public class GoogleData
{
    public string order, result, msg, value;
}


public class GoogleSheetManager : MonoBehaviour
{
    const string URL = "https://script.google.com/macros/s/AKfycbzc_jVXZDyzGESM7sC63Od3cZxgrnFZAzC__-Jiy3RKs3s993N0rWyEByz-kI3uingZ/exec";
    public GoogleData GD;
    public InputField IDInput, PassInput;
    string id, pass;

    public GameObject register;
    public GameObject Lobby;
    public Text Status;

    bool SetIDPass()
    {
        id = IDInput.text.Trim();
        pass = PassInput.text.Trim();

        if (id == "" || pass == "") return false;
        else return true;
    }


    public void Register()
    {
        if (!SetIDPass())
        {
            print("���̵� �Ǵ� ��й�ȣ�� ����ֽ��ϴ�");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "register");
        form.AddField("id", id);
        form.AddField("pass", pass);

        StartCoroutine(Post(form));
    }


    public void Login()
    {
        if (!SetIDPass())
        {
            print("���̵� �Ǵ� ��й�ȣ�� ����ֽ��ϴ�");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "login");
        form.AddField("id", id);
        form.AddField("pass", pass);

        StartCoroutine(Post(form));
    }


    void OnApplicationQuit()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "logout");

        StartCoroutine(Post(form));
    }

    IEnumerator Post(WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form)) // �ݵ�� using�� ����Ѵ�
        {
            yield return www.SendWebRequest();

            if (www.isDone) Response(www.downloadHandler.text);
            else print("���� ������ �����ϴ�.");
        }
    }

    void Response(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        GD = JsonUtility.FromJson<GoogleData>(json);
        
        Status.text = GD.msg.ToString();
        Debug.Log(GD.msg);

        if (GD.result == "ERROR")
        {
            print(GD.order + "�� ������ �� �����ϴ�. ���� �޽��� : " + GD.msg);
            return;
        }
        print(GD.order + "�� �����߽��ϴ�. �޽��� : " + GD.msg);
    }
}