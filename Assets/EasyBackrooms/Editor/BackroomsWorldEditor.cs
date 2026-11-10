using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    [CustomEditor(typeof(BackroomsWorld))]
    public sealed class BackroomsWorldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            var world = (BackroomsWorld)target;

            if (world.mode == WorldMode.Infinite)
                EditorGUILayout.HelpBox("Chunks stream in around the target when you press Play.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(world.mode != WorldMode.Fixed))
                {
                    if (GUILayout.Button("Build", GUILayout.Height(28)))
                        Run(w => w.BuildFixed());
                }

                if (GUILayout.Button("Clear", GUILayout.Height(28)))
                    Run(w => w.Clear());
            }
        }

        void Run(System.Action<BackroomsWorld> action)
        {
            foreach (var t in targets)
            {
                var world = (BackroomsWorld)t;
                action(world);
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            }
        }
    }
}
