using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTimerUIView : MonoBehaviour
{
    [Header("Time GUI")]
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] Slider timeSlider;
    
    public void UpdateTime(int hours, int minutes)
    {
        // 2. 위에서 만든 함수를 호출하여 0~1 사이의 값을 계산
        float sliderValue = ConvertTimeToSliderValue(hours, minutes);

        // 3. 슬라이더에 최종 값 적용
        timeSlider.value = sliderValue;
        
        timeText.text = hours.ToString("D2") + ":" + minutes.ToString("D2");
    }
    
    public float ConvertTimeToSliderValue(int hours, int minutes)
    {
        // 1. 현재 시간을 소수점 형태로 변환 (예: 9시 30분 -> 9.5f)
        float currentTime = hours + (float)minutes / 60.0f;

        // 2. 시간 범위 설정
        float startTime = 8.0f;   // 시작: 오전 8시
        float totalDuration = 16.0f; // 전체 길이: 16시간 (8시부터 24시까지)

        // 3. (현재시간 - 시작시간) / (전체시간) 공식 적용
        float normalizedTime = (currentTime - startTime) / totalDuration;
        
        return Mathf.Clamp01(normalizedTime);
    }
}