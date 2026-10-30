using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerWeapons : MonoBehaviour
{
    [SerializeField] private Image _weaponImage;
    [SerializeField] private Sprite[] _modeIcons;
    [SerializeField] private TextMeshProUGUI _weaponModeText;
    [SerializeField] private TextMeshProUGUI _bulletCountText;
    [SerializeField] private TextMeshProUGUI _bombCountText;

    private Player _player;
    private PlayerGunFire _playerGunFire;
    private PlayerBombFire _playerBombFire;


    private void Awake()
    {
        if (_weaponImage == null)
        {
            _weaponImage = GetComponentInChildren<Image>();
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _player = player.GetComponent<Player>();
        _playerGunFire = player.GetComponent<PlayerGunFire>();
        _playerBombFire = player.GetComponent<PlayerBombFire>();

        _player.OnModeChanged += OnPlayerModeChanged;
        _playerGunFire.OnBulletChanged += OnPlayerBulletChanged;
        _playerBombFire.OnBombFired += OnPlayerBombFired;

        OnPlayerModeChanged(_player.WeaponMode);
        OnPlayerBulletChanged(_playerGunFire.BulletCount, _playerGunFire.MaxBulletCount);
        OnPlayerBombFired(_playerBombFire.BombCount);
    }

    private void OnDestroy()
    {
        _player.OnModeChanged -= OnPlayerModeChanged;
        _playerGunFire.OnBulletChanged -= OnPlayerBulletChanged;
        _playerBombFire.OnBombFired -= OnPlayerBombFired;
    }

    private void OnPlayerModeChanged(WeaponMode mode)
    {
        _weaponImage.sprite = _modeIcons[(int)mode];

        switch (mode)
        {
            case WeaponMode.Normal:
                _weaponModeText.text = "일반 모드";
                break;

            case WeaponMode.Sniper:
                _weaponModeText.text = "저격 모드";
                break;

            case WeaponMode.Melee:
                _weaponModeText.text = "근접 모드";
                break;

            default:
                Debug.LogWarning("무기 모드 스프라이트를 찾을 수 없습니다.");
                break;
        }
    }

    private void OnPlayerBulletChanged(int bulletCount, int maxBulletCount)
    {
        _bulletCountText.text = $"{bulletCount} / {maxBulletCount}";
    }

    private void OnPlayerBombFired(int bombCount)
    {
        _bombCountText.text = $"폭탄 {bombCount}";
    }
}