using UnityEngine;

public enum MeleeHitMode
{
    Collider,
    Fan
}

public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private GameObject _knife;
    [SerializeField] private MeleeHitMode _meleeHitMode = MeleeHitMode.Collider;

    [Header("부채꼴")]
    [SerializeField] private float _range = 2f;
    [SerializeField] private float _angle = 90f;
    [SerializeField] private LayerMask _targetMask;

    private Player _player;
    private Animator _animator;
    private Collider _knifeCollider;

    private int _meleeLayerIndex;


    private void Awake()
    {
        _player = GetComponentInParent<Player>();
        _animator = GetComponentInChildren<Animator>();
        _knifeCollider = _knife.GetComponentInChildren<Collider>(true);
        _meleeLayerIndex = _animator.GetLayerIndex("Melee Layer");
    }

    private void OnEnable()
    {
        _player.OnModeChanged += RefreshKnife;
    }

    private void OnDisable()
    {
        _player.OnModeChanged -= RefreshKnife;
    }

    private void Start()
    {
        RefreshKnife(_player.WeaponMode);

        DeactivateCollider();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // 근접 모드에서 마우스 오른쪽 버튼을 누르면, 쉬고 있을 때만 휘두른다.
        if (Input.GetButtonDown("Fire2") && _player.WeaponMode == WeaponMode.Melee && IsResting())
        {
            DeactivateCollider();
            _animator.SetTrigger("Slash");
        }
    }

    public void FanAttack()
    {
        if (_meleeHitMode != MeleeHitMode.Fan)
            return;

        Debug.Log("부채꼴 공격!");

        Collider[] colliders = Physics.OverlapSphere(_player.transform.position, _range, _targetMask);
        foreach (Collider collider in colliders)
        {
            Vector3 origin = _player.transform.position;

            // 적의 타격점을 찾는다.
            Vector3 hitPoint = collider.ClosestPoint(origin);

            // 방향을 판단한다.
            Vector3 direction = hitPoint - origin;
            direction.y = 0f;
            float angle = Vector3.Angle(_player.transform.forward, direction);

            if (angle > _angle / 2f)
                continue;

            // 적과 나 사이에 벽이 있으면 때리지 않는다.
            if (Physics.Linecast(origin, hitPoint, LayerMask.NameToLayer("Default")))
                continue;

            if (collider.TryGetComponent(out IDamageable damageable))
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.Value = 25f;
                damageInfo.HitPoint = collider.ClosestPoint(origin);
                damageInfo.HitNormal = (transform.position - collider.ClosestPoint(origin)).normalized;

                damageable.TakeDamage(damageInfo);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 플레이하지 않을 때도 불리므로, Awake에서 찾는 _player 대신 여기서 찾는다.
        Player player = GetComponentInParent<Player>();
        if (player == null) return;

        Vector3 origin = player.transform.position;
        Vector3 forward = player.transform.forward;
        Gizmos.color = Color.yellow;

        // 앞쪽을 y축으로 -Angle/2, +Angle/2만큼 돌린 두 방향이 부채꼴의 양 끝이다.
        Vector3 left = Quaternion.Euler(0f, -_angle / 2f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0f, _angle / 2f, 0f) * forward;
        Gizmos.DrawLine(origin, origin + left * _range);
        Gizmos.DrawLine(origin, origin + right * _range);

        // 호: 10도마다 짧은 선을 이어 그린다.
        int count = Mathf.Max(1, Mathf.CeilToInt(_angle / 10f));
        Vector3 previous = origin + left * _range;
        for (int i = 1; i <= count; i++)
        {
            float angle = -_angle / 2f + _angle * i / count;
            Vector3 next = origin + Quaternion.Euler(0f, angle, 0f) * forward * _range;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }

    public void ActivateCollider()
    {
        if (_meleeHitMode != MeleeHitMode.Collider)
            return;

        _knifeCollider.enabled = true;
    }

    public void DeactivateCollider()
    {
        _knifeCollider.enabled = false;
    }

    public void Hit(Collider collider, float damage)
    {
        if (collider.CompareTag("Player"))
            return;

        DamageInfo damageInfo = new DamageInfo();
        damageInfo.Value = damage;

        collider.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
    }

    // Melee Layer가 Empty에 있고 다른 상태로 넘어가는 중도 아니면 쉬고 있는 것이다.
    private bool IsResting()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_meleeLayerIndex);
        return state.IsName("Empty") && !_animator.IsInTransition(_meleeLayerIndex);
    }

    // 무기 모드가 바뀌었다는 알림이 오면 불리는 함수(콜백). 단검은 근접 모드에서만 보인다.
    private void RefreshKnife(WeaponMode mode)
    {
        _knife.SetActive(mode == WeaponMode.Melee);
    }
}