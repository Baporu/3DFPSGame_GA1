using System.Collections;
using TMPro;
using UnityEngine;

// 목표: 입력창에 포커스가 가면 살짝 커지고, 잘못 입력하면 좌우로 흔들려서 피드백을 준다.
[RequireComponent(typeof(TMP_InputField))]
public class UI_InputFeedback : MonoBehaviour
{
    private const float TwoPi = Mathf.PI * 2f;

    [SerializeField] private float _focusScale = 1.03f;
    [SerializeField] private float _focusDuration = 0.12f;
    [SerializeField] private float _shakeDuration = 0.35f;
    [SerializeField] private float _shakeMagnitude = 14f;
    [SerializeField] private float _shakeFrequency = 5f;

    public float FocusScale => _focusScale;
    public float ShakeDuration => _shakeDuration;
    public float ShakeMagnitude => _shakeMagnitude;

    private TMP_InputField _input;
    private RectTransform _rect;
    private Vector3 _baseScale;
    private Vector2 _shakeOrigin;
    private Coroutine _scaleCoroutine;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _input = GetComponent<TMP_InputField>();
        _rect = (RectTransform)transform;
        _baseScale = _rect.localScale;
    }

    private void OnEnable()
    {
        _input.onSelect.AddListener(OnSelected);
        _input.onDeselect.AddListener(OnDeselected);
    }

    private void OnDisable()
    {
        _input.onSelect.RemoveListener(OnSelected);
        _input.onDeselect.RemoveListener(OnDeselected);

        // 흔들리거나 커진 채로 꺼지면 다시 켰을 때 어긋나므로 원래 상태로 돌려놓는다.
        if (_shakeCoroutine != null)
        {
            _rect.anchoredPosition = _shakeOrigin;
        }
        _scaleCoroutine = null;
        _shakeCoroutine = null;
        _rect.localScale = _baseScale;
    }

    public void Shake()
    {
        // 목표: 진행 중이던 흔들림은 원위치로 되돌린 뒤 새로 시작한다.
        // 1. 이미 흔들리는 중이면 멈추고 원위치로 복귀한다.
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _rect.anchoredPosition = _shakeOrigin;
        }

        // 2. 지금 위치를 기준점으로 삼아 흔든다.
        _shakeOrigin = _rect.anchoredPosition;
        _shakeCoroutine = StartCoroutine(Shake_Coroutine());
    }

    private void OnSelected(string text)
    {
        ScaleTo(_baseScale * _focusScale);
    }

    private void OnDeselected(string text)
    {
        ScaleTo(_baseScale);
    }

    private void ScaleTo(Vector3 targetScale)
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        _scaleCoroutine = StartCoroutine(Scale_Coroutine(targetScale));
    }

    private IEnumerator Scale_Coroutine(Vector3 targetScale)
    {
        Vector3 startScale = _rect.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < _focusDuration)
        {
            _rect.localScale = Vector3.LerpUnclamped(startScale, targetScale, UIEase.OutCubic(elapsedTime / _focusDuration));

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _rect.localScale = targetScale;
        _scaleCoroutine = null;
    }

    private IEnumerator Shake_Coroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _shakeDuration)
        {
            // 시간이 지날수록 흔들림이 줄어든다.
            float progress = elapsedTime / _shakeDuration;
            float offset = Mathf.Sin(progress * _shakeFrequency * TwoPi) * _shakeMagnitude * (1f - progress);
            _rect.anchoredPosition = _shakeOrigin + Vector2.right * offset;

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _rect.anchoredPosition = _shakeOrigin;
        _shakeCoroutine = null;
    }
}
