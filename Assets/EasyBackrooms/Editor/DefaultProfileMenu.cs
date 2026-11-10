using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class DefaultProfileMenu
    {
        [MenuItem("Assets/Create/EasyBackrooms/Default Profile", priority = 0)]
        static void CreateDefaultProfile()
        {
            string path = AssetDatabase.GenerateUniqueAssetPath(EditorAssetUtil.SelectedFolder() + "/BackroomsProfile.asset");
            var profile = ScriptableObject.CreateInstance<BackroomsProfile>();
            AssetDatabase.CreateAsset(profile, path);

            // Stages live inside the profile file
            profile.border = EditorAssetUtil.AddSubAsset<DoorwayBorderStage>(profile, "Doorway Border");
            profile.stages.Add(EditorAssetUtil.AddSubAsset<RandomWallsStage>(profile, "Random Walls"));
            profile.stages.Add(EditorAssetUtil.AddSubAsset<OpenRoomStage>(profile, "Open Rooms"));
            profile.stages.Add(EditorAssetUtil.AddSubAsset<PillarStage>(profile, "Pillars"));
            profile.stages.Add(EditorAssetUtil.AddSubAsset<LightPanelStage>(profile, "Light Panels"));
            profile.stages.Add(EditorAssetUtil.AddSubAsset<ConnectivityStage>(profile, "Connectivity"));

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
        }
    }
}
