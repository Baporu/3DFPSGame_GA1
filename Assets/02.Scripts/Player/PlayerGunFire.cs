using System;
using System.Collections;
using UnityEngine;

// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("스탯")]
    [SerializeField] private int _maxBulletCount = 30;
    public int MaxBulletCount => _maxBulletCount;
    [SerializeField] private float _reloadingTime = 3f;
    [SerializeField] private float _range = 100f; // 사거리
    [SerializeField] private float _damage = 10f; // 공격력
    [SerializeField] private float _fireInterval = 0.1f; // 발사 속도

    [SerializeField] private GameObject[] _muzzleFlashes;
    [SerializeField] private float _flashTime = 0.05f;

    [Header("총알 궤적 이펙트")]
    [SerializeField] private LineRenderer _bulletTracer;
    [SerializeField] private Transform _muzzlePoint; // 궤적 시작 위치(총구)
    [SerializeField] private float _tracerTime = 0.2f;

    private float _fireTimer = 0f;
    private int _bulletCount = 0;
    public int BulletCount => _bulletCount;

    private Coroutine _reloadCoroutine = null;
    private Coroutine _tracerCoroutine = null;
    private Animator _animator;

    public event Action<int, int> OnBulletChanged;


    private void Awake()
    {
        _bulletCount = _maxBulletCount;
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }


    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // 0. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;

        // 1. 마우스 왼쪽 버튼 입력 받기
        bool isMouseVisible = Cursor.visible;
        if (!isMouseVisible && Input.GetButton("Fire1") && _fireTimer <= 0f && _bulletCount > 0)
        {
            _bulletCount--;
            _fireTimer = _fireInterval;

            _animator.SetTrigger("Attack");
            OnBulletChanged?.Invoke(_bulletCount, _maxBulletCount);

            Shoot();

            // 이 방식은: 이 모노비헤이비어가 등록한 모든 코루틴을 실행 취소
            // StopAllCoroutines();

            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                Debug.Log("재장전 취소!");
                _reloadCoroutine = null;
            }
        }

        // 2. R키 누르면 총알 재장전 시작
        if (Input.GetKeyDown(KeyCode.R) && _reloadCoroutine == null)
        {
            _reloadCoroutine = StartCoroutine(Reloading_Coroutine());
        }
    }

    private IEnumerator Reloading_Coroutine()
    {
        Debug.Log("재장전 시작!");

        yield return new WaitForSeconds(_reloadingTime);

        _bulletCount = _maxBulletCount;
        OnBulletChanged?.Invoke(_bulletCount, _maxBulletCount);

        Debug.Log("재장전 완료!");
        _reloadCoroutine = null;
    }


    private void Shoot()
    {
        StartCoroutine(MuzzleFlash_Coroutine());

        // Ray: 레이저(시작위치, 날아갈 방향)
        // Physics.RayCast: 레이저를 발사
        // RayCastHit: 레이저가 물체와 부딛히면 그 정보를 저장하는 구조체

        // 2. 레이(광선)를 생성하고 발사될 위치와 진행 방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        // 4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);

        // 5. 총구에서 맞은 지점(못 맞췄다면 사거리 끝)까지 총알 궤적을 그린다.
        Vector3 endPoint = isHit ? hitInfo.point : ray.origin + ray.direction * _range;
        ShowBulletTracer(endPoint);

        if (isHit)
        {
            // Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 대미지: {_damage}");

            // 맞은 자리에 총알 자국 이펙트 만들기
            ParticleSystem effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point; // 맞은 자리
            effect.transform.forward = hitInfo.normal; // 수직 위치로 회전
            effect.Play();

            // 대미지 구조체 만들기
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            damageInfo.HitPoint = hitInfo.point;
            damageInfo.HitNormal = hitInfo.normal;

            hitInfo.collider.GetComponent<IDamageable>()?.TakeDamage(damageInfo);

            /*
            hitInfo.collider.GetComponent<Zombie>()?.TakeDamage(_damage);
            hitInfo.collider.GetComponent<Drum>()?.TakeDamage(_damage);
            hitInfo.collider.GetComponent<Box>()?.TakeDamage(_damage);
            */

            // 황유찬 :         Player
            // 김민쉬:     Zombie, Boss
            // 박현서:  Drum, Box, Bird
        }
    }

    private void ShowBulletTracer(Vector3 endPoint)
    {
        if (_bulletTracer == null) return;

        Vector3 startPoint = _muzzlePoint != null ? _muzzlePoint.position : Camera.main.transform.position;
        _bulletTracer.SetPosition(0, startPoint);
        _bulletTracer.SetPosition(1, endPoint);

        // 연사 중에도 궤적이 겹쳐 꺼지지 않도록 이전 코루틴을 취소하고 다시 시작한다.
        if (_tracerCoroutine != null)
        {
            StopCoroutine(_tracerCoroutine);
        }
        _tracerCoroutine = StartCoroutine(BulletTracer_Coroutine());
    }

    private IEnumerator BulletTracer_Coroutine()
    {
        _bulletTracer.enabled = true;

        yield return new WaitForSeconds(_tracerTime);

        _bulletTracer.enabled = false;
        _tracerCoroutine = null;
    }

    private IEnumerator MuzzleFlash_Coroutine()
    {
        int randomIndex = UnityEngine.Random.Range(0, _muzzleFlashes.Length);

        GameObject flash = _muzzleFlashes[randomIndex];
        flash.SetActive(true);

        yield return new WaitForSeconds(_flashTime);

        flash.SetActive(false);
    }
}