using UnityEngine;
using UnityEngine.UI;

public class SteppedFill : MonoBehaviour
{
    [Tooltip("��Ʈ(���) ����")]
    [SerializeField, Range(2, 100)] private int segments = 10;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image.type != Image.Type.Filled)
            Debug.LogWarning($"{name}: Image.Type�� Filled�� �ƴϸ� �۵����� �ʽ��ϴ�.");
    }

    /// <summary>
    /// 0~1 ���� ���� �޾� segments ��ŭ ���ȭ�� �� fillAmount�� ����
    /// </summary>
    public void SetNormalizedValue(float normalized)
    {
        float clamped = Mathf.Clamp01(normalized);
        float step = 1f / segments;
        // ���� ���� segments ���Ұ� ������ ����
        float quantized = Mathf.Floor(clamped / step) * step;
        _image.fillAmount = quantized;
    }
}
