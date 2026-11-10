using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Empty Theme", fileName = "BackroomsTheme", order = 3)]
    public sealed class BackroomsTheme : ScriptableObject
    {
        [Tooltip("Each builder makes one surface with its own material. Add, remove or swap freely.")]
        public List<SurfaceBuilder> builders = new List<SurfaceBuilder>();
    }
}
