using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 목표: 로그인 화면이 열릴 때 제목과 입력 요소가 차례로 나타나고, 로그인/회원가입 패널을 부드럽게 전환한다.
public class UI_LoginIntro : MonoBehaviour
{
    [Header("제목")]
    [SerializeField] private RectTransform _title;
    [SerializeField] private float _titleDropDistance = 80f;
    [SerializeField] private float _titleDuration = 0.6f;
    [SerializeField] private float _floatAmplitude = 6f;
    [SerializeField] private float _floatSpeed = 1.2f;

    [Header("패널")]
    [SerializeField] private RectTransform _loginPanel;
    [SerializeField] private RectTransform _registerPanel;
    [SerializeField] private Button _goRegisterButton;
    [SerializeField] private Button _backButton;

    [Header("요소 등장")]
    [SerializeField] private float _stagger = 0.08f;
    [SerializeField] private float _itemDuration = 0.45f;
    [SerializeField] private float _slideDistance = 40f;
    [SerializeField] private float _panelFadeDuration = 0.2f;

    public float Stagger => _stagger;
    public float ItemDuration => _itemDuration;
    public float SlideDistance => _slideDistance;

    private readonly Dictionary<RectTransform, Vector2> _basePositions = new Dictionary<RectTransform, Vector2>();
    private Vector2 _titleBasePosition;
    private RectTransform _currentPanel;
    private bool _isFloating;
    private bool _isTransitioning;

    private void Awake()
    {
        _titleBasePosition = _title.anchoredPosition;
        _currentPanel = _loginPanel;

        CacheBasePositions(_loginPanel);
        CacheBasePositions(_registerPanel);

        // 첫 프레임에 완성된 화면이 보였다가 사라지는 깜빡임이 없도록 미리 숨겨둔다.
        GetOrAddGroup(_title).alpha = 0f;
        HideItems(_loginPanel);
    }

    private void OnEnable()
    {
        _goRegisterButton.onClick.AddListener(ShowRegister);
        _backButton.onClick.AddListener(ShowLogin);
    }

    private void OnDisable()
    {
        _goRegisterButton.onClick.RemoveListener(ShowRegister);
        _backButton.onClick.RemoveListener(ShowLogin);
    }

    private void Start()
    {
        StartCoroutine(Intro_Coroutine());
    }

    private void Update()
    {
        if (!_isFloating) return;

        float offset = Mathf.Sin(Time.unscaledTime * _floatSpeed) * _floatAmplitude;
        _title.anchoredPosition = _titleBasePosition + Vector2.up * offset;
    }

    public void ShowLogin()
    {
        if (_isTransitioning || _currentPanel == _loginPanel) return;

        StartCoroutine(SwitchPanel_Coroutine(_loginPanel));
    }

    public void ShowRegister()
    {
        if (_isTransitioning || _currentPanel == _registerPanel) return;

        StartCoroutine(SwitchPanel_Coroutine(_registerPanel));
    }

    private IEnumerator Intro_Coroutine()
    {
        // 목표: 제목이 먼저 내려오고, 조금 뒤에 패널 요소들이 순서대로 나타난다.
        _isTransitioning = true;

        // interactable을 끄면 버튼/입력창이 비활성 색(회색)으로 보이므로, 클릭 차단은 blocksRaycasts로 한다.
        CanvasGroup panelGroup = GetOrAddGroup(_loginPanel);
        panelGroup.blocksRaycasts = false;

        // 1. 제목이 위에서 내려오며 나타난다.
        StartCoroutine(Appear_Coroutine(_title, _titleBasePosition + Vector2.up * _titleDropDistance, _titleBasePosition, _titleDuration));
        yield return new WaitForSecondsRealtime(_titleDuration * 0.4f);

        // 2. 패널 요소가 순서대로 나타난다.
        yield return PlayItems_Coroutine(_loginPanel);

        panelGroup.blocksRaycasts = true;
        _isFloating = true;
        _isTransitioning = false;
    }

    private IEnumerator SwitchPanel_Coroutine(RectTransform nextPanel)
    {
        // 목표: 지금 패널이 사라진 뒤 다음 패널의 요소가 차례로 나타난다.
        _isTransitioning = true;

        // 1. 지금 패널을 입력 불가로 만들고 페이드아웃한다.
        CanvasGroup currentGroup = GetOrAddGroup(_currentPanel);
        currentGroup.blocksRaycasts = false;

        float elapsedTime = 0f;
        while (elapsedTime < _panelFadeDuration)
        {
            currentGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / _panelFadeDuration);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _currentPanel.gameObject.SetActive(false);
        currentGroup.alpha = 1f;
        currentGroup.blocksRaycasts = true;

        // 2. 다음 패널을 켜고 요소를 숨긴 상태에서 순서대로 등장시킨다.
        CanvasGroup nextGroup = GetOrAddGroup(nextPanel);
        nextGroup.blocksRaycasts = false;
        HideItems(nextPanel);
        nextPanel.gameObject.SetActive(true);

        yield return PlayItems_Coroutine(nextPanel);

        nextGroup.blocksRaycasts = true;
        _currentPanel = nextPanel;
        _isTransitioning = false;
    }

    private IEnumerator PlayItems_Coroutine(RectTransform panel)
    {
        int index = 0;
        foreach (RectTransform item in panel)
        {
            StartCoroutine(DelayedAppear_Coroutine(item, index * _stagger));
            index++;
        }

        // 마지막 요소까지 다 나타날 때까지 기다린다.
        yield return new WaitForSecondsRealtime(index * _stagger + _itemDuration);
    }

    private IEnumerator DelayedAppear_Coroutine(RectTransform item, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        Vector2 endPosition = _basePositions[item];
        yield return Appear_Coroutine(item, endPosition + Vector2.down * _slideDistance, endPosition, _itemDuration);
    }

    private IEnumerator Appear_Coroutine(RectTransform target, Vector2 startPosition, Vector2 endPosition, float duration)
    {
        CanvasGroup group = GetOrAddGroup(target);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = UIEase.OutCubic(elapsedTime / duration);
            group.alpha = t;
            target.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        group.alpha = 1f;
        target.anchoredPosition = endPosition;
    }

    private void CacheBasePositions(RectTransform panel)
    {
        foreach (RectTransform item in panel)
        {
            _basePositions[item] = item.anchoredPosition;
        }
    }

    private void HideItems(RectTransform panel)
    {
        foreach (RectTransform item in panel)
        {
            GetOrAddGroup(item).alpha = 0f;
            item.anchoredPosition = _basePositions[item] + Vector2.down * _slideDistance;
        }
    }

    private CanvasGroup GetOrAddGroup(Component target)
    {
        if (!target.TryGetComponent(out CanvasGroup group))
        {
            group = target.gameObject.AddComponent<CanvasGroup>();
        }

        return group;
    }
}
