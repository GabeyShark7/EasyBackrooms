using UnityEngine;

namespace EasyBackrooms
{
    // Inherit this to make your own stage
    public abstract class LayoutStage : ScriptableObject
    {
        [SerializeField, Tooltip("Uncheck to skip this stage without removing it.")]
        bool active = true;

        [SerializeField, Tooltip("Change this to reroll only this stage. Leave at 0 for the default.")]
        int salt;

        public bool Active => active;
        public int Salt => salt != 0 ? salt : Hash.FromString(GetType().FullName);

        public abstract void Apply(ChunkLayout layout, ref DetRandom rng);
    }
}
