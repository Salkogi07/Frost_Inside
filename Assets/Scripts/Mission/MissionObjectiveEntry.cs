// MissionObjectiveEntry.cs
using UnityEngine;
using TMPro;

public class MissionObjectiveEntry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text objectiveText;

    /// <summary>
    /// 이 목표 항목의 텍스트를 설정합니다.
    /// </summary>
    /// <param name="text">표시할 목표 내용</param>
    public void Setup(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = "- " + text; // 앞에 '-'를 붙여 목록처럼 보이게 합니다.
        }
    }
}