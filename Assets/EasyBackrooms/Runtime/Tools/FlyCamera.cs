using UnityEngine;
using UnityEngine.InputSystem;

namespace EasyBackrooms
{
    // Click to look and Esc to let go
    [AddComponentMenu("EasyBackrooms/Fly Camera")]
    public sealed class FlyCamera : MonoBehaviour
    {
        [Min(0f), Tooltip("Meters per second.")]
        public float speed = 6f;

        [Min(1f), Tooltip("Speed multiplier while holding Shift.")]
        public float fastMultiplier = 3f;

        [Min(0f), Tooltip("Mouse look sensitivity.")]
        public float lookSensitivity = 0.12f;

        float yaw;
        float pitch;

        void OnEnable()
        {
            var euler = transform.eulerAngles;
            yaw = euler.y;
            pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                var delta = mouse.delta.ReadValue() * lookSensitivity;
                yaw += delta.x;
                pitch = Mathf.Clamp(pitch - delta.y, -89f, 89f);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            var move = Vector3.zero;
            if (keyboard.wKey.isPressed) move += Vector3.forward;
            if (keyboard.sKey.isPressed) move += Vector3.back;
            if (keyboard.dKey.isPressed) move += Vector3.right;
            if (keyboard.aKey.isPressed) move += Vector3.left;
            if (keyboard.eKey.isPressed) move += Vector3.up;
            if (keyboard.qKey.isPressed) move += Vector3.down;

            float velocity = speed * (keyboard.leftShiftKey.isPressed ? fastMultiplier : 1f);
            transform.Translate(move.normalized * velocity * Time.deltaTime, Space.Self);
        }
    }
}
