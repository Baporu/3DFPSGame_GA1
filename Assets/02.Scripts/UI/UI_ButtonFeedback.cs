using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 목표: 버튼에 마우스를 올리거나 누르면 크기가 변해서 반응이 느껴지게 한다.
[RequireComponent(typeof(Button))]
public class UI_ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _hoverScale = 1.08f;
    [SerializeField] private float _pressScale = 0.94f;
    [SerializeField] private float _duration = 0.15f;

    public float HoverScale => _hoverScale;
    public float PressScale => _pressScale;
    public float Duration => _duration;

    private Button _button;
    private Vector3 _baseScale;
    private bool _isHover;
    private bool _isPressed;
    private Coroutine _scaleCoroutine;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        // 비활성화될 때 크기를 원래대로 돌려놓아야 다시 켰을 때 커진 채로 남지 않는다.
        _isHover = false;
        _isPressed = false;
        _scaleCoroutine = null;
        transform.localScale = _baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        _isHover = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHover = false;
        _isPressed = false;
        Refresh();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        _isPressed = true;
        Refresh();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        Refresh();
    }

    private void Refresh()
    {
        // 목표: 현재 상태(누름 > 호버 > 기본) 순서로 목표 크기를 정하고 거기까지 부드럽게 변한다.
        // 1. 누른 상태가 가장 우선이다.
        float targetScale = 1f;
        if (_isPressed)
        {
            targetScale = _pressScale;
        }
        else if (_isHover)
        {
            targetScale = _hoverScale;
        }

        // 2. 진행 중인 변화는 끊고 지금 크기에서 새로 시작한다.
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        _scaleCoroutine = StartCoroutine(Scale_Coroutine(_baseScale * targetScale));
    }

    private IEnumerator Scale_Coroutine(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            float t = UIEase.OutBack(elapsedTime / _duration);
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        _scaleCoroutine = null;
    }
}
