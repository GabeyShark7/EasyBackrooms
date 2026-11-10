using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class DefaultProfileMenu
    {
        [MenuItem("Assets/Create/EasyBackrooms/Default Profile", priority = 0)]
        static void CreateDefaultProfile()
        {
            string folder = "Assets";
            if (Selection.activeObject != null)
            {
                string selected = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(selected))
                    folder = AssetDatabase.IsValidFolder(selected) ? selected : Path.GetDirectoryName(selected).Replace('\\', '/');
            }

            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/BackroomsProfile.asset");
            var profile = ScriptableObject.CreateInstance<BackroomsProfile>();
            AssetDatabase.CreateAsset(profile, path);

            // Stages live inside the profile file
            profile.border = AddStage<DoorwayBorderStage>(profile, "Doorway Border");
            profile.stages.Add(AddStage<RandomWallsStage>(profile, "Random Walls"));
            profile.stages.Add(AddStage<OpenRoomStage>(profile, "Open Rooms"));
            profile.stages.Add(AddStage<PillarStage>(profile, "Pillars"));
            profile.stages.Add(AddStage<ConnectivityStage>(profile, "Connectivity"));

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
        }

        static T AddStage<T>(BackroomsProfile profile, string name) where T : ScriptableObject
        {
            var stage = ScriptableObject.CreateInstance<T>();
            stage.name = name;
            AssetDatabase.AddObjectToAsset(stage, profile);
            return stage;
        }
    }
}
