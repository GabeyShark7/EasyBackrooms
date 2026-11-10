using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    [CustomEditor(typeof(ChunkBuilder))]
    public sealed class ChunkBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Build", GUILayout.Height(28)))
                    Run(builder => builder.Build());

                if (GUILayout.Button("Clear", GUILayout.Height(28)))
                    Run(builder => builder.Clear());
            }
        }

        void Run(System.Action<ChunkBuilder> action)
        {
            foreach (var t in targets)
            {
                var builder = (ChunkBuilder)t;
                action(builder);
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
            }
        }
    }
}
