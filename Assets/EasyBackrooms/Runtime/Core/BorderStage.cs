using UnityEngine;

namespace EasyBackrooms
{
    // Inherit this to change chunk edges
    public abstract class BorderStage : ScriptableObject
    {
        // Use only rng or chunks wont match
        public abstract void BuildSide(bool[] walls, ref DetRandom rng);
    }
}
