using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private GameObject _knife;

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

    public void ActivateCollider()
    {
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