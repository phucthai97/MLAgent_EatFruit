using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerData : MonoBehaviour
{
    [Space(3)]
    [Header("HERO DATA]")]
    [SerializeField] private bool isBot = false;
    [SerializeField] private int _heroType;

    [Space(5)]
    [Header("HERO ACTION]")]
    [SerializeField] private bool _isKill = false;
    [SerializeField] private bool _isJumping;
    [SerializeField] private bool _isAttacking;
    [SerializeField] private bool _isRolling;
    [SerializeField] private bool _isTakingDamage;
    [SerializeField] private bool _flipX = false;

    [Space(5)]
    [Header("[EFFECT SUPPLY]")]
    [SerializeField] private bool _gloveState = false;
    [SerializeField] private bool _bInvincibility = false;
    [SerializeField] private float _moveSpeed = 3.7f;
    public readonly float moveSpeedNormal = 3.7f;
    public readonly float moveSpeedBooter = 5.0f;

    [Space(5)]
    [Header("[HEALTH PLAYER]")]
    [SerializeField] private float _currentHealth = 100f;
    [SerializeField] private float _totalHealth = 999f;

    [Space(7)]
    [Header("[HEART DATA]")]
    [SerializeField] private int _playerHearts = 0;
    private int _kills = 0;

    [Space(5)]
    [Header("[WEAPON DATA]")]
    [SerializeField] private int _numAttack = 0;
    [SerializeField] private float _damage = 0f;
    [SerializeField] private int _currentIndexWea = 0;
    [SerializeField] private WeaponType _weaponType = WeaponType.SWORD;
    [SerializeField] private List<EquipWeapon> _equipWeaponList;
    [SerializeField] private List<WeaponType> _weaponsTypeList = new();
    [SerializeField] private List<GlobalInfo.WeaponBalanceClass> _weaponDataList;
    [Space(1)]
    [Header("[RELOAD BULLETS]")]
    [SerializeField] private List<int> _bulletCountData;
    private int _reloadTime = 7;
    private List<float> _intervalLoadWeapon;
    private Coroutine[] _coroutineReloadBullet;

    [Space(5)]
    [Header("[REFERENCES]")]
    [SerializeField] private Hero _hero;
    [SerializeField] private WeaponSlots _weaponSlots;

    public List<EquipWeapon> EquipWeaponList
    {
        get { return _equipWeaponList; }
        set => _equipWeaponList = value;
    }

    public WeaponType WeaponType { get => _weaponType; set => _weaponType = value; }
    public List<WeaponType> Weapons { get => _weaponsTypeList; set => _weaponsTypeList = value; }
    public List<int> BulletCountData { get => _bulletCountData; set => _bulletCountData = value; }
    public bool IsBot { get => isBot; set => isBot = value; }
    public Hero Hero { get => _hero; }
    public int HeroType { get => _heroType; set => _heroType = value; }
    public bool FlipX
    {
        get => _flipX;
        set
        {
            _flipX = value;
            _hero.FlipChildObjects();
        }
    }
    public int PlayerHearts
    {
        get => _playerHearts;
        set
        {
            _playerHearts = value;
            if (isBot && !_hero.IsTraining)
                GameManager.Inst.BotDisplayDebug.TxtmpHearts.text = _playerHearts.ToString();
        }
    }
    public int Kills
    {
        get => _kills;
        set
        {
            _kills = value;
            if (IsBot & !_hero.IsTraining)
                GameManager.Inst.BotDisplayDebug.TxtmpKills.text = _kills.ToString();
        }
    }
    public bool GloveState { get => _gloveState; set => _gloveState = value; }
    public bool BInvincibility { get => _bInvincibility; set => _bInvincibility = value; }
    public float Damage { get => _damage; set => _damage = value; }
    public int ReloadTime { get => _reloadTime; set => _reloadTime = value; }
    public List<float> IntervalLoadWeapon { get => _intervalLoadWeapon; }
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            //Debug.Log($"Set currentHealth {value} for {_hero.PlayerName}");
            _currentHealth = value;
        }
    }
    public float TotalHealth { get => _totalHealth; set => _totalHealth = value; }
    public bool IsKill { get => _isKill; set => _isKill = value; }
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }
    public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }
    public bool IsRolling { get => _isRolling; set => _isRolling = value; }
    public bool IsTakingDamage { get => _isTakingDamage; set => _isTakingDamage = value; }
    public int NumAttack
    {
        get => _numAttack;
        set
        {
            _numAttack = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _coroutineReloadBullet = new Coroutine[4];
        StartCoroutine(WaitSetIsLocal());
    }

    IEnumerator WaitSetIsLocal()
    {
        yield return new WaitForSeconds(0.1f);
        if (_hero.IsLocal)
            ForLocalPlayer();
    }

    /// <summary>
    /// Set the value for players on the player's local device
    /// Players on other clients will have their values set via Hero.SetPropertiesEntryGame()
    /// </summary>
    private void ForLocalPlayer()
    {
        //Set player hearts
        var userSelectedCharacterLevel = GlobalInfo.instance.userUpgradeStatData.Single(t => t.Key == $"charac{_heroType + 1}").Value;
        var characterData = GlobalInfo.instance.CharacterDatas.Single(t => t.id == _heroType + 1).CharacterBalanceClasses
            .Single(t => t.level == userSelectedCharacterLevel);

        if (MainManager.instance != null)
        {
            if (MainManager.instance.adsFinished && !isBot && _hero.IsPlayer)
                PlayerHearts = (int)characterData.strength + 1;
            else
                PlayerHearts = (int)characterData.strength;

            if (MainManager.instance.buy_life && !isBot && _hero.IsPlayer)
                _totalHealth = characterData.life + (characterData.life * 0.2f);
            else
                _totalHealth = characterData.life;
        }
        //For training MLAgent
        else
        {
            //PlayerHearts = (int)characterData.strength;
            //_totalHealth = characterData.life;
            PlayerHearts = 9999999;
            _totalHealth = 80;
        }

        CurrentHealth = TotalHealth;
        _kills = 0;

        if (isBot)
        {
            UpdateOrReloadCurrentWeapon(0);
            _hero.SetWeapon(_equipWeaponList[_currentIndexWea].indexSlot - 1, 1);
        }
        else if (_hero.IsPlayer)
        {
            StatusManager.Instance.SetHeartObj(_playerHearts);
            StatusManager.Instance.SetKill(_kills);

            _weaponSlots = FindFirstObjectByType<WeaponSlots>();
            _weaponSlots.UpdateSpriteWeaponOnSlots(MainManager.instance.EquipWeaponList);
        }

        SetIntervalLoadWeapon(_reloadTime);
    }

    private void SetupBulletCountData()
    {
        Debug.Log($"SetupBulletCountData {_equipWeaponList.Count} weapons");
        for (int i = 0; i < _equipWeaponList.Count; i++)
        {
            string spriteName = _equipWeaponList[i].SpriteName;
            if (!spriteName.Equals("none"))
            {
                var userSelectedWeaponData = GlobalInfo.instance.userUpgradeStatData.Single(t => t.Key == spriteName).Value;
                var weaponData = GlobalInfo.instance.WeaponDatas.Single(t => t.id == _equipWeaponList[i].IndexSlot).WeaponBalanceClasses
                                                                .Single(t => t.level == userSelectedWeaponData);

                _weaponDataList.Add(weaponData);
                _weaponsTypeList.Add(MatchData.Inst.GetWeaponType(spriteName));

                if (weaponData.bulletCount != 0)
                    _bulletCountData.Add(weaponData.bulletCount);
                else
                    _bulletCountData.Add(int.MaxValue);
            }
        }
    }

    public void SetIntervalLoadWeapon(float totalTime)
    {
        _intervalLoadWeapon = new List<float>();
        for (int i = 0; i < _equipWeaponList.Count; i++)
        {
            float roundNum = Mathf.Floor(totalTime / (float)MatchData.Inst.SpriteCountdown.Count * 100) / 100;
            _intervalLoadWeapon.Add(roundNum);
        }
    }

    /// <summary>
    /// 1. If you select the current weapon slot -> Reload is for that weapon
    /// 2. If you choose another weapon slot, the weapon will changes
    /// </summary>
    public bool UpdateOrReloadCurrentWeapon(int index)
    {

        WeaponType choosingWeapon = MatchData.Inst.GetWeaponType(EquipWeaponList[index].spriteName);
        if (_weaponType == choosingWeapon)
        {
            //If is not a melee weapon &&  && no coroutine is running
            if (_bulletCountData[index] != int.MaxValue && _bulletCountData[index] < _weaponDataList[index].bulletCount && _coroutineReloadBullet[index] == null)
            {
                if (_weaponSlots != null)
                    _weaponSlots.ReloadBulletsSprite(index);

                _coroutineReloadBullet[index] = StartCoroutine(ReloadBulletsCoroutine(index));
            }
            return false;
        }

        _weaponType = choosingWeapon;
        _currentIndexWea = index;

        if (_hero.IsPlayer)
        {
            //Debug.Log($"Change Weapon main player -> _weaponType: {_weaponType}");
            GameManager.Inst.ChangeFieldOfViewCam(_weaponDataList[_currentIndexWea].attackRange);
            //Sound change weapon
            SoundManager.instance.AS[1].clip = SoundManager.instance.AC[11];
            SoundManager.instance.AS[1].Play();
        }
        return true;

    }

    public string GetWeaponName()
    {
        return _weaponDataList[_currentIndexWea].weaponName;
    }

    public bool CheckBullets()
    {
        if (_bulletCountData[_currentIndexWea] == int.MaxValue)
            return true;

        if (_bulletCountData[_currentIndexWea] > 0 && _coroutineReloadBullet[_currentIndexWea] == null)
            return true;
        else
            return false;
    }

    public void HandleBullet()
    {
        if (!_hero.IsLocal)
            return;
        if (_bulletCountData[_currentIndexWea] == int.MaxValue)
            return;

        if (_bulletCountData[_currentIndexWea] > 0 && _coroutineReloadBullet[_currentIndexWea] == null)
        {
            _bulletCountData[_currentIndexWea]--;

            //Update number bullets on UI weapon slots 
            if (_weaponSlots != null)
                _weaponSlots.NumBullets[_currentIndexWea].text = _bulletCountData[_currentIndexWea].ToString();

            if (_bulletCountData[_currentIndexWea] == 0 && _coroutineReloadBullet[_currentIndexWea] == null)
            {
                if (_weaponSlots != null)
                    _weaponSlots.ReloadBulletsSprite(_currentIndexWea);

                _coroutineReloadBullet[_currentIndexWea] = StartCoroutine(ReloadBulletsCoroutine(_currentIndexWea));
            }
        }
    }


    public IEnumerator ReloadBulletsCoroutine(int index)
    {
        yield return new WaitForSecondsRealtime(ReloadTime);
        _bulletCountData[index] = _weaponDataList[index].bulletCount;
        _coroutineReloadBullet[index] = null;
    }


    public void ChangeWeapon(int index)
    {
        string spriteName = _equipWeaponList[index].spriteName;
        _weaponType = MatchData.Inst.GetWeaponType(spriteName);
        // level reference
        var userSelectedWeaponData = GlobalInfo.instance.userUpgradeStatData.Single(t => t.Key == $"weapon{_hero.GetIndexWeapon() + 1}").Value;

        if (!_hero.IsTraining)
        {
            SoundManager.instance.AS[1].Play();
            SoundManager.instance.AS[1].clip = SoundManager.instance.AC[11];
            _hero.gameObject.GetPhotonView().RPC("SetWeapon", PhotonTargets.AllBuffered, _hero.GetIndexWeapon(), userSelectedWeaponData);
        }
        else
            _hero.SetWeapon(_hero.GetIndexWeapon(), userSelectedWeaponData);
    }

    /// <summary>
    /// Add weapon list database && create weaponData list
    /// </summary>
    public void AddWeaponList(List<EquipWeapon> weaponList)
    {
        _equipWeaponList.AddRange(weaponList);
        SetupBulletCountData();
    }

    public GlobalInfo.WeaponBalanceClass CurrentWeaData()
    {
        return _weaponDataList[_currentIndexWea];
    }
}
