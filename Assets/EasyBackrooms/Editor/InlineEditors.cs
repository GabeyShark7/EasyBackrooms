using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    // Draws sub asset settings in place
    internal static class InlineEditors
    {
        public static bool Draw(Object asset, Dictionary<Object, UnityEditor.Editor> cache)
        {
            if (asset == null) return false;

            string key = "EasyBackrooms.Fold." + asset.GetInstanceID();
            bool open = SessionState.GetBool(key, false);
            string label = $"{asset.name}   ({ObjectNames.NicifyVariableName(asset.GetType().Name)})";

            bool now = EditorGUILayout.Foldout(open, label, true);
            if (now != open) SessionState.SetBool(key, now);
            if (!now) return false;

            if (!cache.TryGetValue(asset, out var editor) || editor == null)
            {
                editor = UnityEditor.Editor.CreateEditor(asset);
                cache[asset] = editor;
            }

            bool changed;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                editor.OnInspectorGUI();
                changed = EditorGUI.EndChangeCheck();
            }

            return changed;
        }

        public static void Dispose(Dictionary<Object, UnityEditor.Editor> cache)
        {
            foreach (var editor in cache.Values)
            {
                if (editor != null) Object.DestroyImmediate(editor);
            }
            cache.Clear();
        }
    }

    // Rebuilds worlds a moment after edits
    [InitializeOnLoad]
    internal static class WorldRefresher
    {
        static readonly HashSet<Object> Pending = new HashSet<Object>();
        static double due;

        static WorldRefresher()
        {
            EditorApplication.update += Tick;
        }

        public static void Queue(Object asset)
        {
            if (asset == null) return;
            Pending.Add(asset);
            due = EditorApplication.timeSinceStartup + 0.35;
        }

        static void Tick()
        {
            if (Pending.Count == 0 || EditorApplication.timeSinceStartup < due) return;

            foreach (var world in Object.FindObjectsByType<BackroomsWorld>(FindObjectsSortMode.None))
            {
                if (!Pending.Contains(world.profile) && !Pending.Contains(world.theme)) continue;

                if (Application.isPlaying)
                {
                    world.Rebuild();
                }
                else if (world.mode == WorldMode.Fixed && world.GetComponentInChildren<ChunkView>(true) != null)
                {
                    world.BuildFixed();
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
                }
            }

            Pending.Clear();
        }
    }

    [CustomEditor(typeof(BackroomsProfile))]
    public sealed class BackroomsProfileEditor : UnityEditor.Editor
    {
        readonly Dictionary<Object, UnityEditor.Editor> cache = new Dictionary<Object, UnityEditor.Editor>();

        public override void OnInspectorGUI()
        {
            var profile = (BackroomsProfile)target;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Randomize Seed", GUILayout.Height(24)))
            {
                Undo.RecordObject(profile, "Randomize Seed");
                profile.seed = Random.Range(int.MinValue, int.MaxValue);
                EditorUtility.SetDirty(profile);
                changed = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stage Settings", EditorStyles.boldLabel);
            changed |= InlineEditors.Draw(profile.border, cache);
            foreach (var stage in profile.stages) changed |= InlineEditors.Draw(stage, cache);

            if (changed) WorldRefresher.Queue(profile);
        }

        void OnDisable() => InlineEditors.Dispose(cache);
    }

    [CustomEditor(typeof(BackroomsTheme))]
    public sealed class BackroomsThemeEditor : UnityEditor.Editor
    {
        readonly Dictionary<Object, UnityEditor.Editor> cache = new Dictionary<Object, UnityEditor.Editor>();

        public override void OnInspectorGUI()
        {
            var theme = (BackroomsTheme)target;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Surface Settings", EditorStyles.boldLabel);
            foreach (var builder in theme.builders) changed |= InlineEditors.Draw(builder, cache);

            if (changed) WorldRefresher.Queue(theme);
        }

        void OnDisable() => InlineEditors.Dispose(cache);
    }
}
