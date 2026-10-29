using System;
using UnityEngine;

// 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;
    [SerializeField] private int _maxBombCount = 3;
    [SerializeField] private float _minThrowPower = 10f;
    [SerializeField] private float _maxThrowPower = 20f;
    [SerializeField] private float _maxChargeTime = 5f;

    private bool _isCharging;
    private int _bombCount;
    public int BombCount => _bombCount;
    private float _throwPower;
    private float _chargeSpeed;

    private Animator _animator;
    private Player _player;

    public Action<int> OnBombFired;


    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _player = GetComponent<Player>();

        _bombCount = _maxBombCount;
        _throwPower = _minThrowPower;
        _chargeSpeed = (_maxThrowPower - _minThrowPower) / _maxChargeTime;
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return;

        if (_player.WeaponMode != WeaponMode.Normal)
            return;

        UpdateBomb();
    }

    private void UpdateBomb()
    {
        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire2") && _bombCount > 0)
        {
            _isCharging = true;
        }

        if (Input.GetButton("Fire2") && _isCharging)
        {
            _throwPower += _chargeSpeed * Time.deltaTime;
            _throwPower = Mathf.Min(_throwPower, _maxThrowPower);
        }

        if (Input.GetButtonUp("Fire2") && _isCharging)
        {
            _isCharging = false;

            _animator.SetTrigger("Throw");
            Shoot();

            _throwPower = _minThrowPower;
        }
    }

    private void Shoot()
    {
        _bombCount--;

        // 2. 발사 위치에서 수류탄 생성하기
        Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

        // 3. 생성된 수류탄을 시선 방향으로 던지기
        Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
        rigidbody.AddForce(Camera.main.transform.forward * _throwPower, ForceMode.Impulse);

        OnBombFired?.Invoke(_bombCount);
    }
}