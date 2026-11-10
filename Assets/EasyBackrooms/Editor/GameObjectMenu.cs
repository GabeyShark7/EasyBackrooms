using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class GameObjectMenu
    {
        [MenuItem("GameObject/EasyBackrooms/Backrooms World", false, 10)]
        static void World(MenuCommand command)
        {
            // Grabs assets so you dont have to
            var world = BackroomsSetup.CreateWorld(command.context as GameObject,
                EditorAssetUtil.FindFirst<BackroomsProfile>(), EditorAssetUtil.FindFirst<BackroomsTheme>());

            var cam = Camera.main;
            if (cam != null && cam.GetComponent<FlyCamera>() == null && cam.GetComponentInParent<FirstPersonPlayer>() == null)
            {
                Undo.AddComponent<FlyCamera>(cam.gameObject);
                Undo.RecordObject(cam.transform, "Move Camera");
                cam.transform.position = world.FindSpawnPoint() + Vector3.up * 1.6f;
            }
        }

        [MenuItem("GameObject/EasyBackrooms/Backrooms Lights", false, 11)]
        static void Lights(MenuCommand command)
        {
            BackroomsSetup.CreateLights(command.context as GameObject, Object.FindFirstObjectByType<BackroomsWorld>(), FolderNearProfile());
        }

        [MenuItem("GameObject/EasyBackrooms/Player", false, 13)]
        static void Player(MenuCommand command)
        {
            BackroomsSetup.CreatePlayer(command.context as GameObject, Object.FindFirstObjectByType<BackroomsWorld>());
        }

        [MenuItem("GameObject/EasyBackrooms/Prop Spawner", false, 14)]
        static void Props(MenuCommand command)
        {
            var go = new GameObject("Backrooms Props");
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Prop Spawner");
            go.AddComponent<PropSpawner>().world = Object.FindFirstObjectByType<BackroomsWorld>();
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/EasyBackrooms/NavMesh", false, 15)]
        static void Nav(MenuCommand command)
        {
            var go = new GameObject("Backrooms NavMesh");
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create NavMesh");
            go.AddComponent<BackroomsNavMesh>().world = Object.FindFirstObjectByType<BackroomsWorld>();
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/EasyBackrooms/Layout Preview", false, 30)]
        static void Preview(MenuCommand command)
        {
            var go = new GameObject("Backrooms Layout Preview");
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Layout Preview");
            var preview = go.AddComponent<LayoutPreview>();
            preview.profile = EditorAssetUtil.FindFirst<BackroomsProfile>();
            Selection.activeObject = go;

            if (preview.profile == null)
                Debug.LogWarning("EasyBackrooms: no profile found. Right click in the Project window and pick Create > EasyBackrooms > Default Profile.");
        }

        static string FolderNearProfile()
        {
            var profile = EditorAssetUtil.FindFirst<BackroomsProfile>();
            return profile != null
                ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(profile)).Replace('\\', '/')
                : BackroomsSetup.DefaultFolder;
        }
    }
}
