using UnityEngine;

namespace EasyBackrooms
{
    // Inherit this for your own flicker
    public abstract class FlickerPattern : ScriptableObject
    {
        // Return 1 for full and 0 for off
        public abstract float Evaluate(Vector3 position, float time);
    }
}
