using UnityEngine;

public class PlayerMeleeEvent : MonoBehaviour
{
    private PlayerMeleeAttack _meleeAttack;


    private void Start()
    {
        _meleeAttack = GetComponentInParent<PlayerMeleeAttack>();
    }

    public void ActivateCollider()
    {
        _meleeAttack.ActivateCollider();
    }

    public void DeactivateCollider()
    {
        _meleeAttack.DeactivateCollider();
    }
}