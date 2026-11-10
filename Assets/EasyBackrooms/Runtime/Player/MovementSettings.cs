using UnityEngine;

namespace EasyBackrooms
{
    // Make copies for different walk feels
    [CreateAssetMenu(menuName = "EasyBackrooms/Movement Settings", fileName = "MovementSettings", order = 30)]
    public sealed class MovementSettings : ScriptableObject
    {
        [Header("Speed")]
        [Min(0f)] public float walkSpeed = 2.6f;
        [Min(0f)] public float sprintSpeed = 5f;
        [Min(0f)] public float crouchSpeed = 1.2f;

        [Min(0.1f), Tooltip("How fast you reach full speed. Lower feels heavier.")]
        public float acceleration = 12f;

        [Min(0f)] public float gravity = 18f;

        [Header("Body")]
        [Min(0.5f)] public float standHeight = 1.75f;
        [Min(0.5f)] public float crouchHeight = 1.1f;
        [Min(0.1f)] public float radius = 0.3f;

        [Min(0f), Tooltip("Eyes sit this far below the top of the head.")]
        public float eyeOffset = 0.12f;

        [Min(0.1f), Tooltip("How fast you crouch and stand.")]
        public float crouchTransition = 6f;

        [Header("Look")]
        [Min(0f)] public float mouseSensitivity = 0.08f;

        [Min(0f), Tooltip("Gamepad look speed in degrees per second.")]
        public float stickSensitivity = 140f;

        [Range(10f, 89f)] public float maxPitch = 85f;

        [Header("Steps")]
        [Min(0.1f), Tooltip("Meters per step while walking.")]
        public float walkStride = 0.75f;

        [Min(0.1f), Tooltip("Meters per step while sprinting.")]
        public float sprintStride = 1.05f;

        [Header("Head Bob")]
        [Tooltip("Turn this off if bobbing makes you sick.")]
        public bool headBob = true;

        [Min(0f)] public float bobHeight = 0.035f;
        [Min(0f)] public float bobSway = 0.02f;

        [Min(0f), Tooltip("Camera roll in degrees when strafing.")]
        public float strafeTilt = 1.5f;
    }
}
