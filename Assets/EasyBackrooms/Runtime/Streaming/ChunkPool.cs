using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    // Reuses chunks so nothing stutters
    public sealed class ChunkPool
    {
        readonly Transform parent;
        readonly Stack<ChunkView> free = new Stack<ChunkView>();

        public ChunkPool(Transform parent)
        {
            this.parent = parent;
        }

        public int FreeCount => free.Count;

        public ChunkView Get()
        {
            while (free.Count > 0)
            {
                var view = free.Pop();
                if (view == null) continue;
                view.gameObject.SetActive(true);
                return view;
            }

            return Create(parent);
        }

        public void Release(ChunkView view)
        {
            view.gameObject.SetActive(false);
            free.Push(view);
        }

        public void Forget()
        {
            free.Clear();
        }

        public static ChunkView Create(Transform parent)
        {
            var go = new GameObject("Chunk");
            go.transform.SetParent(parent, false);
            return go.AddComponent<ChunkView>();
        }
    }
}
