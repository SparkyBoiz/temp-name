using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownPlayerController : Singleton<TopDownPlayerController>
    {
        public enum State
        {
            Idle,
            Moving
        }

    public float moveSpeed = 6f;
    public float batteryDrainRate;
    public Game.Core.InputSystem_Actions inputActions;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Animator _animator;
    private float nextMoveSoundTime;
    private bool controlsInverted;
    public State state { get; private set; }
    public float battery { get; private set; }
    public Action<float> BatteryChanged;

        public override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _animator = GetComponentInChildren<Animator>();
            nextMoveSoundTime = 0f;


            if (inputActions == null)
            {
                inputActions = new Game.Core.InputSystem_Actions();
            }
            battery = 1f;
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnCancelMove;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnCancelMove;
            inputActions.Disable();
            BatteryChanged = null;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            _moveInput = controlsInverted ? -input : input;
            EnterMoving();
        }

        public void InvertControls(bool inverted)
        {
            controlsInverted = inverted;
            if (state == State.Moving)
            {
                _moveInput = controlsInverted ? -_moveInput : _moveInput;
            }
        }

        private void OnCancelMove(InputAction.CallbackContext ctx)
        {
            _moveInput = Vector2.zero;
            EnterIdle();
        }

        void Update()
        {
            switch (state)
            {
                case State.Idle:
                    HandleIdle();
                    break;
                case State.Moving:
                    HandleMoving();
                    break;
            }
            BatteryDrain();
        }

        private void FixedUpdate()
        {
            switch (state)
            {
                case State.Idle:
                    HandleIdlePhysics();
                    break;
                case State.Moving:
                    HandleMovingPhysics();
                    break;
            }
        }

        void EnterIdle()
        {
            _rb.linearVelocity = Vector3.zero;
            state = State.Idle;
            _animator.SetBool("Moving", false);
        }

        void EnterMoving()
        {
            state = State.Moving;
            _animator.SetBool("Moving", true);
        }

        void HandleIdle()
        {

        }

        void HandleIdlePhysics()
        {

        }

        void HandleMoving()
        {
            if (Time.time >= nextMoveSoundTime && _rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                GameEvents.RequestSoundWord(SoundType.PlayerWalk, transform.position, Vector3.up, 0.7f);
                nextMoveSoundTime = Time.time + 0.3f;
            }
        }

        void HandleMovingPhysics()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;

            if (_moveInput.x > 0.01f)
            {
                transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            }
            else if (_moveInput.x < -0.01f)
            {
                transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            }
        }

        void BatteryDrain()
        {
            battery -= batteryDrainRate * Time.deltaTime;
            battery = Mathf.Max(0, battery);
            BatteryChanged?.Invoke(battery);

            if (battery == 0)
                InGameUIManager.Instance.ShowGameover();
        }

        public void ChargeBattery(float amount)
        {
            battery += amount;
            battery = Mathf.Min(1f, battery);
            BatteryChanged?.Invoke(battery);
        }
    }
}