using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace EasyBackrooms
{
    // WASD Shift sprint Ctrl crouch
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("EasyBackrooms/First Person Player")]
    public sealed class FirstPersonPlayer : MonoBehaviour
    {
        [Tooltip("How the player moves. Leave empty for the defaults.")]
        public MovementSettings settings;

        [Tooltip("The camera, as a child of this object.")]
        public Transform cameraRoot;

        [Tooltip("Drop the player on open floor in this world when play starts.")]
        public BackroomsWorld spawnInWorld;

        [Tooltip("Grab the mouse on start. Esc lets go and clicking grabs it again.")]
        public bool lockCursor = true;

        [Tooltip("Fires on every footstep.")]
        public UnityEvent stepped = new UnityEvent();

        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public float Speed { get; private set; }

        CharacterController controller;
        MovementSettings fallback;
        Vector3 velocity;
        float yaw;
        float pitch;
        float roll;
        float height;
        float stepDistance;
        float bobWeight;
        float spawnTime;

        MovementSettings S => settings != null ? settings : fallback;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (settings == null) fallback = ScriptableObject.CreateInstance<MovementSettings>();
            if (cameraRoot == null && Camera.main != null && Camera.main.transform.IsChildOf(transform)) cameraRoot = Camera.main.transform;

            height = S.standHeight;
            yaw = transform.eulerAngles.y;
            ApplyHeight();
        }

        void Start()
        {
            if (spawnInWorld != null) Teleport(spawnInWorld.FindSpawnPoint());
            spawnTime = Time.time;
            if (lockCursor) SetCursorLocked(true);
        }

        void OnDestroy()
        {
            if (fallback != null) Destroy(fallback);
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;
            float dt = Time.deltaTime;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
            if (lockCursor && mouse != null && mouse.leftButton.wasPressedThisFrame) SetCursorLocked(true);

            var look = Vector2.zero;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked) look += mouse.delta.ReadValue() * S.mouseSensitivity;
            if (pad != null) look += pad.rightStick.ReadValue() * (S.stickSensitivity * dt);

            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, -S.maxPitch, S.maxPitch);

            var move = Vector2.zero;
            bool sprintHeld = false;
            bool crouchHeld = false;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed) move.y -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
                sprintHeld |= keyboard.leftShiftKey.isPressed;
                crouchHeld |= keyboard.leftCtrlKey.isPressed || keyboard.cKey.isPressed;
            }

            if (pad != null)
            {
                move += pad.leftStick.ReadValue();
                sprintHeld |= pad.leftStickButton.isPressed;
                crouchHeld |= pad.buttonEast.isPressed;
            }

            move = Vector2.ClampMagnitude(move, 1f);
            Move(move, sprintHeld, crouchHeld, dt);
            UpdateCamera(move, dt);
        }

        void Move(Vector2 input, bool sprintHeld, bool crouchHeld, float dt)
        {
            IsCrouching = crouchHeld;
            IsSprinting = !crouchHeld && sprintHeld && input.y > 0.1f;

            float target = IsCrouching ? S.crouchSpeed : IsSprinting ? S.sprintSpeed : S.walkSpeed;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            var wish = (transform.right * input.x + transform.forward * input.y) * target;

            var flat = new Vector3(velocity.x, 0f, velocity.z);
            flat = Vector3.MoveTowards(flat, wish, S.acceleration * dt);

            // Wait for the floor to stream in
            bool waiting = Time.time - spawnTime < 3f &&
                           !Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 5f);

            IsGrounded = controller.isGrounded;
            float vy = velocity.y;
            if (waiting) vy = 0f;
            else if (IsGrounded && vy < 0f) vy = -2f;
            else vy -= S.gravity * dt;

            velocity = new Vector3(flat.x, vy, flat.z);
            controller.Move(velocity * dt);

            height = Mathf.MoveTowards(height, IsCrouching ? S.crouchHeight : S.standHeight, S.crouchTransition * dt);
            ApplyHeight();

            Speed = flat.magnitude;
            if (IsGrounded && Speed > 0.3f)
            {
                float stride = IsSprinting ? S.sprintStride : S.walkStride;
                float before = Mathf.Floor(stepDistance / stride);
                stepDistance += Speed * dt;
                if (Mathf.Floor(stepDistance / stride) > before) stepped.Invoke();
            }

            float moving = target > 0f ? Mathf.Clamp01(Speed / target) : 0f;
            bobWeight = Mathf.MoveTowards(bobWeight, IsGrounded ? moving : 0f, dt * 4f);
        }

        void UpdateCamera(Vector2 input, float dt)
        {
            if (cameraRoot == null) return;

            roll = Mathf.Lerp(roll, -input.x * S.strafeTilt, 1f - Mathf.Exp(-8f * dt));

            var offset = Vector3.zero;
            if (S.headBob)
            {
                float stride = IsSprinting ? S.sprintStride : S.walkStride;
                float phase = stepDistance / stride * Mathf.PI;
                offset.y = -Mathf.Abs(Mathf.Sin(phase)) * S.bobHeight * bobWeight;
                offset.x = Mathf.Cos(phase) * S.bobSway * bobWeight;
            }

            cameraRoot.localPosition = new Vector3(0f, height - S.eyeOffset, 0f) + offset;
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, roll);
        }

        void ApplyHeight()
        {
            controller.height = height;
            controller.radius = S.radius;
            controller.center = new Vector3(0f, height * 0.5f, 0f);
        }
    }
}
