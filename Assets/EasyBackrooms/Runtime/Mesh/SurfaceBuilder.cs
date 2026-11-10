using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBackrooms
{
    // Inherit this to make a new surface
    public abstract class SurfaceBuilder : ScriptableObject
    {
        [SerializeField, Tooltip("Uncheck to skip this surface without removing it.")]
        bool active = true;

        [Tooltip("Drop your material here.")]
        public Material material;

        [Min(0.01f), Tooltip("How many meters one copy of the texture covers.")]
        public float tileSize = 1f;

        [Tooltip("Give this surface a collider so you cant walk through it.")]
        public bool addCollider = true;

        [Tooltip("Whether this surface casts shadows.")]
        public ShadowCastingMode shadows = ShadowCastingMode.On;

        public bool Active => active;

        public abstract void Build(in BuildContext context, MeshData mesh);

        // Add spots where real lights go
        public virtual void CollectLightPoints(in BuildContext context, List<Vector3> points) { }
    }
}
