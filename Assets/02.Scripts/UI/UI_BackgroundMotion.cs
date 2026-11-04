using UnityEngine;

// 목표: 정지 이미지 배경이 천천히 확대/축소되며 흘러서 화면이 살아 있게 만든다. (Ken Burns)
public class UI_BackgroundMotion : MonoBehaviour
{
    private const float TwoPi = Mathf.PI * 2f;

    // 가장자리가 비지 않도록 기본 배율을 1보다 크게 둔다. 움직이는 범위가 이 여유분 안에 있어야 한다.
    [SerializeField] private float _baseScale = 1.1f;
    [SerializeField] private float _zoomAmount = 0.05f;
    [SerializeField] private float _zoomPeriod = 30f;
    [SerializeField] private Vector2 _driftDistance = new Vector2(30f, 15f);
    [SerializeField] private float _driftPeriod = 40f;

    public float BaseScale => _baseScale;
    public float ZoomAmount => _zoomAmount;
    public float ZoomPeriod => _zoomPeriod;
    public Vector2 DriftDistance => _driftDistance;
    public float DriftPeriod => _driftPeriod;

    private RectTransform _rect;
    private Vector2 _basePosition;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _basePosition = _rect.anchoredPosition;
    }

    private void Update()
    {
        float time = Time.unscaledTime;

        // 1. 기본 배율에서 시작해 천천히 커졌다가 돌아온다.
        float zoom = 0.5f - 0.5f * Mathf.Cos(time * TwoPi / Mathf.Max(_zoomPeriod, 0.01f));
        float scale = _baseScale + _zoomAmount * zoom;
        _rect.localScale = new Vector3(scale, scale, 1f);

        // 2. 가로/세로 주기를 다르게 해서 같은 궤적이 반복되는 느낌을 줄인다.
        float angle = time * TwoPi / Mathf.Max(_driftPeriod, 0.01f);
        Vector2 drift = new Vector2(Mathf.Sin(angle) * _driftDistance.x, Mathf.Sin(angle * 0.7f) * _driftDistance.y);
        _rect.anchoredPosition = _basePosition + drift;
    }
}
