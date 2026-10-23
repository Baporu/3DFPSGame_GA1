using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    // Todo. ZombieHealthBar를 이용해서 체력과 스태미나 UI 구현
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    [SerializeField] private PlayerMove _playerMove;


    private void Update()
    {
        //_healthSlider.value = 플레이어 체력 / 플레이어 최대 체력;
        _staminaSlider.value = _playerMove.Stamina / PlayerMove.MaxStamina;
    }
}
