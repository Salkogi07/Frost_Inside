using UnityEngine;
using UnityEngine.UI;

public class SteppedFill : MonoBehaviour
{
    [Tooltip("도트(계단) 개수")]
    [SerializeField, Range(2, 100)] private int segments = 10;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image.type != Image.Type.Filled)
            Debug.LogWarning($"{name}: Image.Type이 Filled가 아니면 작동하지 않습니다.");
    }

    /// <summary>
    /// 0~1 사이 값을 받아 segments 만큼 계단화한 뒤 fillAmount에 설정
    /// </summary>
    public void SetNormalizedValue(float normalized)
    {
        float clamped = Mathf.Clamp01(normalized);
        float step = 1f / segments;
        // 현재 값을 segments 분할값 단위로 내림
        float quantized = Mathf.Floor(clamped / step) * step;
        _image.fillAmount = quantized;
    }
}
