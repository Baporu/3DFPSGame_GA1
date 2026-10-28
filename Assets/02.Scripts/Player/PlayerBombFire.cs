using UnityEngine;

// 마우스 올느쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;
    [SerializeField] private float _minThrowPower = 10f;
    [SerializeField] private float _maxThrowPower = 20f;
    [SerializeField] private float _maxChargeTime = 5f;
    
    private bool _isCharging;
    private float _throwPower;
    private float _chargeSpeed;
    
    
    private void Awake()
    {
        _throwPower = _minThrowPower;
        _chargeSpeed = (_maxThrowPower - _minThrowPower) / _maxChargeTime;
    }
    
    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return;

        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire2"))
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
            Shoot();
            _throwPower = _minThrowPower;
        }
    }

    private void Shoot()
    {
        // 2. 발사 위치에서 수류탄 생성하기
        Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

        // 3. 생성된 수류탄을 시선 방향으로 던지기
        Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
        rigidbody.AddForce(Camera.main.transform.forward * _throwPower, ForceMode.Impulse);
    }

}
