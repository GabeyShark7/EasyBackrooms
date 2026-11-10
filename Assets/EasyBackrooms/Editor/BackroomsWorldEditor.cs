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
                EditorGUILayout.HelpBox("Chunks stream in around the target when you press Play. Switch Mode to Fixed to build in the editor.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(world.mode != WorldMode.Fixed && !Application.isPlaying))
                {
                    if (GUILayout.Button("Build", GUILayout.Height(28)))
                        Run(w =>
                        {
                            if (w.mode == WorldMode.Fixed) w.BuildFixed();
                            else w.Rebuild();
                        });
                }

                if (GUILayout.Button("Clear", GUILayout.Height(28)))
                    Run(w => w.Clear());
            }

            using (new EditorGUI.DisabledScope(world.profile == null))
            {
                if (GUILayout.Button("Randomize Seed", GUILayout.Height(22)))
                {
                    Undo.RecordObject(world.profile, "Randomize Seed");
                    world.profile.seed = Random.Range(int.MinValue, int.MaxValue);
                    EditorUtility.SetDirty(world.profile);
                    WorldRefresher.Queue(world.profile);
                }
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
