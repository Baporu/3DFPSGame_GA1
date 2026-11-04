using System.Collections;
using UnityEngine;

// 목표: 로그인 화면이 열릴 때 제목이 내려오고 입력 폼이 아래에서 올라오며 나타난다. 모드 전환은 UI_Login이 맡는다.
public class UI_LoginIntro : MonoBehaviour
{
    [Header("제목")]
    [SerializeField] private RectTransform _title;
    [SerializeField] private float _titleDropDistance = 80f;
    [SerializeField] private float _titleDuration = 0.6f;
    [SerializeField] private float _floatAmplitude = 6f;
    [SerializeField] private float _floatSpeed = 1.2f;

    [Header("폼")]
    [SerializeField] private RectTransform _form;
    [SerializeField] private float _formDelay = 0.25f;
    [SerializeField] private float _formDuration = 0.5f;
    [SerializeField] private float _slideDistance = 40f;

    public float FormDelay => _formDelay;
    public float FormDuration => _formDuration;
    public float SlideDistance => _slideDistance;

    private Vector2 _titleBasePosition;
    private Vector2 _formBasePosition;
    private bool _isFloating;

    private void Awake()
    {
        _titleBasePosition = _title.anchoredPosition;
        _formBasePosition = _form.anchoredPosition;

        // 첫 프레임에 완성된 화면이 보였다가 사라지는 깜빡임이 없도록 미리 숨겨둔다.
        GetOrAddGroup(_title).alpha = 0f;

        // interactable을 끄면 버튼/입력창이 비활성 색(회색)으로 보이므로, 클릭 차단은 blocksRaycasts로 한다.
        CanvasGroup formGroup = GetOrAddGroup(_form);
        formGroup.alpha = 0f;
        formGroup.blocksRaycasts = false;
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

    private IEnumerator Intro_Coroutine()
    {
        // 목표: 제목이 먼저 내려오고, 조금 뒤에 입력 폼이 올라오며 나타난다.
        // 1. 제목이 위에서 내려오며 나타난다.
        StartCoroutine(Appear_Coroutine(_title, _titleBasePosition + Vector2.up * _titleDropDistance, _titleBasePosition, _titleDuration));
        yield return new WaitForSecondsRealtime(_formDelay);

        // 2. 폼이 아래에서 올라오며 나타난다.
        yield return Appear_Coroutine(_form, _formBasePosition + Vector2.down * _slideDistance, _formBasePosition, _formDuration);

        // 3. 다 나타나면 입력을 받고 제목이 떠다니기 시작한다.
        GetOrAddGroup(_form).blocksRaycasts = true;
        _isFloating = true;
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

    private CanvasGroup GetOrAddGroup(Component target)
    {
        if (!target.TryGetComponent(out CanvasGroup group))
        {
            group = target.gameObject.AddComponent<CanvasGroup>();
        }

        return group;
    }
}
