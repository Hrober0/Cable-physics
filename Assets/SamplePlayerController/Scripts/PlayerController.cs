using System;
using UnityEngine;
using NaughtyAttributes;

namespace HPlayer
{
    public class PlayerController : MonoBehaviour
    {
        public enum Mods { None, Default, Sprint }

        #region -camera-

        [Header("Camera controll")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] public float mouseSensitivity = 8f;
        [SerializeField, Range(0, -90)] private float minPitch = -90f;
        [SerializeField, Range(0, 90)] private float maxPitch = 90f;

        private float yaw;
        private float pitch;

        [field: SerializeField, ReadOnly] public bool FreezCamera { get; set; } = false;

        #endregion

        #region -movment-

        [Header("Default")]
        [SerializeField, Min(1f)]
        private float defaultSpeed = 5f;

        [SerializeField, Min(0.5f)]
        private float defaultHeight = 1.9f;

        [SerializeField, Min(0f)]
        private float smoothMoveTime = 0.1f;

        [SerializeField]
        private LayerMask groundMask;

        [SerializeField]
        private float gravity = -18f;

        [SerializeField]
        private bool canJump = true;

        [SerializeField, ShowIf(nameof(canJump)), Min(1f)]
        private float defaultJumpHeight = 1.2f;

        [Header("Sprint")]
        [SerializeField]
        private bool canSprint = true;

        [SerializeField, ShowIf(nameof(canSprint)), Min(1f)]
        private float sprintSpeed = 9f;

        [SerializeField, ShowIf(EConditionOperator.And, nameof(canSprint), nameof(canJump)), Min(1f)]
        private float sprintJumpHeight = 1.4f;


        [Header("Inputs")]
        [SerializeField, ReadOnly] private bool inputJump = false;
        [SerializeField, ReadOnly] private bool inputSprint = false;
        [SerializeField, ReadOnly] private bool inputCrouch = false;
        [SerializeField, ReadOnly] private Vector2 inputMove = Vector2.zero;
        [SerializeField, ReadOnly] private Vector2 inputMouse = Vector2.zero;

        [Header("States")]
        [SerializeField, ReadOnly] private bool isGrounded = false;
        [SerializeField, ReadOnly] private Mods currentMod = Mods.None;
        [SerializeField, ReadOnly] private float currentSpeed;
        [SerializeField, ReadOnly] private float currentJumpHeight;

        private Vector3 smoothV;
        [SerializeField] private Vector3 velocity;
        [SerializeField] private float verticalVelocity;

        [field: SerializeField, ReadOnly] public bool FreezMovement { get; set; } = false;

        #endregion


        private CharacterController controller;

        public static Action OnPlayerEnterPortal;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            SetMode(Mods.Default);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw = transform.eulerAngles.y;
            pitch = playerCamera.transform.localEulerAngles.x;
        }

        private void Update()
        {
            float radius = controller.radius * 0.9f;
            Vector3 groundCheck = controller.bounds.center - (controller.bounds.extents.y - radius + 0.2f) * Vector3.up;
            isGrounded = Physics.CheckSphere(groundCheck, radius, groundMask);

            GetInput();

            MoveCamera();

            ChoseMode();
            MovePlayer();
        }

        private void SetMode(Mods mod)
        {
            if (currentMod == mod)
                return;

            switch (mod)
            {
                case Mods.Default:
                    currentSpeed = defaultSpeed;
                    currentJumpHeight = defaultJumpHeight;
                    break;
                case Mods.Sprint:
                    currentSpeed = sprintSpeed;
                    currentJumpHeight = sprintJumpHeight;
                    break;
                default:
                    currentSpeed = 0;
                    currentJumpHeight = 0;
                    break;
            }

            currentMod = mod;
        }

        private void ChoseMode()
        {
            if (canSprint && inputSprint)
                SetMode(Mods.Sprint);
            else
                SetMode(Mods.Default);
        }


        private void GetInput()
        {
            // movement
            inputMove.x = Input.GetAxisRaw("Horizontal");
            inputMove.y = Input.GetAxisRaw("Vertical");

            // mouse
            inputMouse.x = Input.GetAxisRaw("Mouse X");
            inputMouse.y = Input.GetAxisRaw("Mouse Y");

            //sprint on/off
            inputSprint = Input.GetKey(KeyCode.LeftShift);

            //Crouching controller
            inputCrouch = Input.GetKey(KeyCode.LeftControl);

            //jumping
            inputJump = Input.GetKey(KeyCode.Space);
        }

        private void MoveCamera()
        {
            // Verrrrrry gross hack to stop camera swinging down at start
            if (Time.unscaledTime < 6f && inputMouse.x * inputMouse.x + inputMouse.y * inputMouse.y > 50)
            {
                inputMouse.x = 0;
                inputMouse.y = 0;
            }

            if (FreezCamera)
                return;

            yaw += inputMouse.x * mouseSensitivity;
            pitch -= inputMouse.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.eulerAngles = Vector3.up * yaw;
            playerCamera.transform.localEulerAngles = Vector3.right * pitch;
        }

        private void MovePlayer()
        {
            if (FreezMovement)
                return;

            Vector3 inputDir = new Vector3(inputMove.x, 0, inputMove.y).normalized;
            Vector3 worldInputDir = transform.TransformDirection(inputDir);

            Vector3 targetVelocity = worldInputDir * currentSpeed;
            velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothV, smoothMoveTime);

            if (isGrounded)
            {
                if (verticalVelocity < 0)
                    verticalVelocity = -2f;

                if (canJump && inputJump && verticalVelocity < 1f)
                    verticalVelocity = Mathf.Sqrt(currentJumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
            velocity = new Vector3(velocity.x, verticalVelocity, velocity.z);

            controller.Move(velocity * Time.deltaTime);
        }


        public void SetPosition(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            velocity = Vector3.zero;
            smoothV = Vector3.zero;
            verticalVelocity = 0;
            controller.enabled = true;
        }
        public void SetRotation(float rotX, float rotY)
        {
            if (rotX > 180)
                rotX -= 360;

            inputMouse.y = 0;
            pitch = rotX;
            playerCamera.transform.localEulerAngles = Vector3.right * rotX;

            inputMouse.x = 0;
            yaw = rotY;
            transform.eulerAngles = Vector3.up * rotY;
        }

        public void SetCameraBackgroundOnSkyBox(float viewRange)
        {
            playerCamera.clearFlags = CameraClearFlags.Skybox;
            playerCamera.farClipPlane = viewRange;
        }
        public void SetCameraBackgroundOnSolidColor(Color color, float viewRange)
        {
            playerCamera.farClipPlane = viewRange;
            playerCamera.clearFlags = CameraClearFlags.SolidColor;
            playerCamera.backgroundColor = color;
        }

        private void SetEnable() => SetEnable(true);
        private void SetDisable() => SetEnable(false);
        public void SetEnable(bool enableState)
        {
            if (enableState)
            {
                FreezCamera = true;
                enabled = false;
            }
            else
            {
                FreezCamera = false;
                enabled = true;
            }
        }
    }
}