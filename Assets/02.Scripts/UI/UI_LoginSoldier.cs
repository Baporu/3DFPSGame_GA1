using System.Collections;
using UnityEngine;

// 목표: 로그인 화면의 병사가 일정 주기마다 사격하고, 총구 이펙트와 약한 빛이 함께 켜지게 한다.
public class UI_LoginSoldier : MonoBehaviour
{
    private const string FireTrigger = "Fire";

    [Header("참조")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject[] _muzzleFlashes;
    [SerializeField] private Light _muzzleLight;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private ParticleSystem _smokePrefab;

    [Header("사격 주기")]
    [SerializeField] private float _initialDelay = 1.5f;
    [SerializeField] private float _minInterval = 2.5f;
    [SerializeField] private float _maxInterval = 4f;
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _shotInterval = 0.12f;

    [Header("총구 이펙트")]
    [SerializeField] private float _flashTime = 0.05f;
    [SerializeField] private float _peakIntensity = 3f;
    [SerializeField] private float _lightDuration = 0.12f;
    [SerializeField] private AnimationCurve _lightCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(1f, 0f));

    public float MinInterval => _minInterval;
    public float MaxInterval => _maxInterval;
    public int BurstCount => _burstCount;
    public float PeakIntensity => _peakIntensity;

    private Coroutine _lightCoroutine;

    private void Awake()
    {
        // 시작할 때는 이펙트와 빛이 모두 꺼져 있어야 한다.
        if (_muzzleLight != null)
        {
            _muzzleLight.intensity = 0f;
        }

        foreach (GameObject flash in _muzzleFlashes)
        {
            flash.SetActive(false);
        }
    }

    private void Start()
    {
        StartCoroutine(FireLoop_Coroutine());
    }

    private IEnumerator FireLoop_Coroutine()
    {
        // 목표: 일정 시간 쉬었다가 짧게 연사하는 동작을 계속 반복한다.
        yield return new WaitForSecondsRealtime(_initialDelay);

        while (true)
        {
            // 1. 다음 사격까지 랜덤한 시간만큼 기다린다.
            yield return new WaitForSecondsRealtime(Random.Range(_minInterval, _maxInterval));

            // 2. 정해진 발수만큼 연사한다.
            for (int i = 0; i < _burstCount; i++)
            {
                Shoot();
                yield return new WaitForSecondsRealtime(_shotInterval);
            }

            // 3. 연사가 끝나면 총구에서 연기가 한 번 피어오른다.
            ShowSmoke();
        }
    }

    private void Shoot()
    {
        _animator.SetTrigger(FireTrigger);

        StartCoroutine(MuzzleFlash_Coroutine());

        // 연사 중에도 빛이 겹쳐 튀지 않도록 이전 펄스를 취소하고 다시 시작한다.
        if (_lightCoroutine != null)
        {
            StopCoroutine(_lightCoroutine);
        }
        _lightCoroutine = StartCoroutine(LightPulse_Coroutine());
    }

    private IEnumerator MuzzleFlash_Coroutine()
    {
        int randomIndex = Random.Range(0, _muzzleFlashes.Length);

        GameObject flash = _muzzleFlashes[randomIndex];
        flash.SetActive(true);

        yield return new WaitForSecondsRealtime(_flashTime);

        flash.SetActive(false);
    }

    private IEnumerator LightPulse_Coroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _lightDuration)
        {
            _muzzleLight.intensity = _lightCurve.Evaluate(elapsedTime / _lightDuration) * _peakIntensity;

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _muzzleLight.intensity = 0f;
        _lightCoroutine = null;
    }

    private void ShowSmoke()
    {
        if (_smokePrefab == null) return;

        ParticleSystem smoke = Instantiate(_smokePrefab, _muzzlePoint.position, Quaternion.identity);
        smoke.Play();
        Destroy(smoke.gameObject, 4f);
    }
}
