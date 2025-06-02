using UnityEngine;
using UnityEngine.UI;

public class WipeController : MonoBehaviour
{
    private Animator _animator;
    private Image _image;
    private readonly int _cicleSizeId = Shader.PropertyToID("_Lerp");

    public float circleSize = 0;

    private void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
        _image = gameObject.GetComponent<Image>();
    }

    private void Update()
    {
        _image.materialForRendering.SetFloat(_cicleSizeId, circleSize);
    }
}
