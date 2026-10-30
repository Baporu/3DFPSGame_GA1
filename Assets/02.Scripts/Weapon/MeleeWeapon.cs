using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float _damage = 25f;

    private PlayerMeleeAttack _meleeAttack;


    private void Awake()
    {
        _meleeAttack = GetComponentInParent<PlayerMeleeAttack>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        _meleeAttack.Hit(collider, _damage);
    }
}