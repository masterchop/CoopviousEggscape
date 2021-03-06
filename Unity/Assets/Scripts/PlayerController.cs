﻿using UnityEngine;

public class PlayerController : SoundPlayerBase
{
    public float maxPlayerSpeed = 10f;
    public float minPlayerSpeed = 1f;
    public float playerAcceleration = 2f;
    public int playerNumber = 1;
    public float dragFactor = 2f;
    public float playerRadius = 4f;
    public float maxAttacksPerSecond = 2f;
    public float cameraEdgeFactor = 10f;
    public float disabledControlsTimeOnAttack = 0.4f;
    public int playerStartLives = 3;
    public float chickenHandVisibleTime = 0.4f;
    public float screenShakeChanceOnHit = 0.05f;
    public Texture2D playerLifeIcon;
    public float playerLifeIconWidth = 100f;
    public float playerLifeIconHeight = 50f;
    public TurtleManager _turtleManager;

    public GameObject chickenHand;
    public GameObject eggPrefab;
    public GameObject otherPlayer;

    public AudioClip[] attackSoundsImpact;
    public AudioClip[] attackSoundsScream;
    public AudioClip[] attackSoundsMiss;
    public AudioClip[] lifeLostSounds;

    private ParticleSystem[] _particleSystems;

    private Vector3 _velocity;
    private float _lastAttack;
    private Animator _animator;
    private GameController _gameController;
    private Camera _camera;
    private ScreenShaker _screenShaker;

    private float _lastDisabledControls;
    private float _lastAttemptedAttack;
    private float _lastHit;

    private int _playerLives;

    public int playerLives
    {
        get
        {
            return _playerLives;
        }
        set
        {
            if (value < playerLives)
            {
                PlayRandomSound(lifeLostSounds);
            }

            _playerLives = value;
            if (_playerLives < 0)
            {
                _gameController.LoseGame();
            }

            Debug.Log(this.gameObject.name + " lives left: " + _playerLives);
        }
    }

    public bool disabledControls
    {
        get;
        set;
    }

    public Vector3 velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        Debug.Log("Player " + playerNumber + " ready!");

        if (eggPrefab == null)
        {
            // check for and alert if missing egg prefab
            Debug.LogError(this.gameObject.name + " is missing its eggPrefab!");
        }

        if (otherPlayer == null)
        {
            // check for and alert if missing other player reference
            Debug.LogError(this.gameObject.name + " is missing its otherPlayer reference!");
        }

        if (chickenHand == null)
        {
            Debug.LogError(this.gameObject.name + " is missing its chicken hand prefab");
        }

        chickenHand.SetActive(false);

        _animator = this.GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError(this.gameObject.name + " is missing its Animator component");
        }

        _particleSystems = this.GetComponentsInChildren<ParticleSystem>();
        if (_particleSystems == null || _particleSystems.Length == 0)
        {
            Debug.LogWarning(this.gameObject.name + " has no registered Particle Systems attached");
        }

        _gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        if (_gameController == null)
        {
            Debug.LogError(this.gameObject.name + " could not find the GameController game object with its GameController component");
        }

        _camera = Camera.main ?? Camera.current;
        if (_camera == null)
        {
            Debug.LogError(this.gameObject.name + " could not identify the main/current camera");
        }

        //        _screenShaker = _camera.GetComponent<ScreenShaker>();
        //        if (_screenShaker == null)
        //        {
        //            Debug.LogWarning(this.gameObject.name + " could not find the ScreenShaker component on the camera");
        //        }

        if (playerLifeIcon == null)
        {
            Debug.LogError(this.gameObject.name + " is missing its playerLifeIcon");
        }

        if (_turtleManager == null)
        {
            Debug.LogError(this.gameObject.name + " is missing a reference to the TurtleManager (TurtleArea)");
        }

        _playerLives = playerStartLives;
    }

    private void Update()
    {
        if (_gameController.gameState == GameController.GameState.MENU)
        {
            return;
        }

        _animator.SetBool("Walking", _velocity.sqrMagnitude > minPlayerSpeed);

        float currentTime = Time.time;
        if (currentTime - _lastDisabledControls > disabledControlsTimeOnAttack)
        {
            disabledControls = false;
        }
    }

    private void FixedUpdate()
    {
        if (_gameController.gameState == GameController.GameState.MENU)
        {
            return;
        }

        if (!this.rigidbody.isKinematic)
        {
            this.rigidbody.velocity = this.rigidbody.angularVelocity = Vector3.zero;
        }

        if (_velocity.sqrMagnitude > minPlayerSpeed)
        {
            // move forward in velocity direction as long as there is a velocity
            Vector3 selfPos = this.transform.position;
            Vector3 speed = _velocity * playerAcceleration * Time.fixedDeltaTime;

            Vector3 projectedPos = Camera.main.WorldToScreenPoint(selfPos + speed);
            if (projectedPos.x < cameraEdgeFactor || projectedPos.x > Screen.width - cameraEdgeFactor ||
                projectedPos.y < cameraEdgeFactor || projectedPos.y > Screen.height - cameraEdgeFactor)
            {
                return;
            }

            _velocity -= speed * dragFactor;
            this.rigidbody.MovePosition(selfPos + speed);
            this.transform.LookAt(selfPos + speed);
        }
    }

    private void OnGUI()
    {
        if (!_turtleManager.activated)
        {
            return;
        }

        float iconWidth = playerLifeIconWidth;
        float iconHeight = playerLifeIconHeight;

        if (this.playerNumber == 0)
        {
            GUI.BeginGroup(new Rect(5f, 5f, (Screen.width / 2f) - 5f, iconHeight + 5f));
            for (int i = 0; i < _playerLives; i++)
            {
                float x = i * (iconWidth + 5f);
                GUI.DrawTexture(new Rect(x, 0f, iconWidth, iconHeight), playerLifeIcon);
            }
            GUI.EndGroup();
        }
        else
        {
            GUI.BeginGroup(new Rect((Screen.width / 2f) + 5f, 5f, (Screen.width / 2f) - 10f, iconHeight + 5f));
            for (int i = 0; i < _playerLives; i++)
            {
                float x = (Screen.width / 2f) - ((i + 1) * (iconWidth + 5f));
                GUI.DrawTexture(new Rect(x, 0f, iconWidth, iconHeight), playerLifeIcon);
            }
            GUI.EndGroup();
        }

        float boxWidth = 100f, boxHeight = 25f;
        GUI.Box(new Rect(Screen.width/2f - boxWidth/2f, 5f, boxWidth, boxHeight), "Killed Turtles: " + GameCounter.killedTurtles);
    }

    public void Move(float deltaX, float deltaY)
    {
        if (disabledControls)
        {
            return;
        }

        // add up velocity gradually
        _velocity += new Vector3(deltaX, 0f, deltaY);

        // make sure velocity stays below max speed
        _velocity = Vector3.ClampMagnitude(_velocity, maxPlayerSpeed);
    }

    public void Attack()
    {
        // check if attacking is allowed
        float currentTime = Time.time;
        if ((currentTime - _lastAttack) < (1f / (float)maxAttacksPerSecond))
        {
            return;
        }

        _lastAttack = currentTime;

        chickenHand.SetActive(true);
        Invoke("HideChickenHand", chickenHandVisibleTime);

        Miss();
    }

    public void Hit()
    {
        float currentTime = Time.time;
        if ((currentTime - _lastHit) < (1f / (float)maxAttacksPerSecond))
        {
            return;
        }

        _lastHit = currentTime;

        Vector3 otherPlayerPos = otherPlayer.transform.position;
        Vector3 selfPos = this.transform.position;
        Vector3 eggDirection = (otherPlayerPos - selfPos).normalized;

        var otherPlayerController = otherPlayer.GetComponent<PlayerController>();
        otherPlayerController.MakeEgg(otherPlayerPos, eggDirection);
        PlayRandomSound(attackSoundsImpact);
    }

    public void Miss()
    {        
        PlayRandomSound(attackSoundsMiss);
    }

    private void HideChickenHand()
    {
        chickenHand.SetActive(false);
    }

    public void MakeEgg(Vector3 position, Vector3 direction)
    {
        PlayRandomSound(attackSoundsScream);

        // create new egg
        var newEgg = Instantiate(eggPrefab, position, this.transform.rotation) as GameObject;

        // set egg move direction
        var eggController = newEgg.GetComponent<EggController>();
        eggController.direction = direction;

        this.transform.LookAt(otherPlayer.transform.position);
        _velocity = Vector3.zero;
        disabledControls = true;
        _lastDisabledControls = Time.time;

        _animator.SetTrigger("Hit");

        foreach (var ps in _particleSystems)
        {
            ps.Play();
        }

        if (_screenShaker != null && Random.value < screenShakeChanceOnHit)
        {
            _screenShaker.ShakeScreen(2);
        }
    }

    public void FadeToBlack()
    {
        _gameController.FadeToBlack(0.6f, 0.6f);
    }
}