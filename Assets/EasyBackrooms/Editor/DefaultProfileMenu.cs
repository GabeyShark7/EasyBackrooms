using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class DefaultProfileMenu
    {
        [MenuItem("Assets/Create/EasyBackrooms/Default Profile", priority = 0)]
        static void Menu()
        {
            EditorAssetUtil.Select(CreateProfile(EditorAssetUtil.SelectedFolder() + "/BackroomsProfile.asset"));
        }

        public static BackroomsProfile CreateProfile(string path)
        {
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            var profile = ScriptableObject.CreateInstance<BackroomsProfile>();
            AssetDatabase.CreateAsset(profile, path);

            // Stages live inside the profile file
            profile.border = EditorAssetUtil.AddSubAsset<DoorwayBorderStage>(profile, "Doorway Border");
            profile.stages.Add(EditorAssetUtil.AddSubAsset<RandomWallsStage>(profile, "Random Walls"));
            profile.stages.Add(EditorAssetUtil.AddSubAsset<OpenRoomStage>(profile, "Open Rooms"));

            var corridors = EditorAssetUtil.AddSubAsset<CorridorStage>(profile, "Corridors");
            corridors.maxCorridors = 1;
            corridors.chance = 0.3f;
            profile.stages.Add(corridors);

            profile.stages.Add(EditorAssetUtil.AddSubAsset<PillarStage>(profile, "Pillars"));
            profile.stages.Add(EditorAssetUtil.AddSubAsset<LightPanelStage>(profile, "Light Panels"));

            var dark = EditorAssetUtil.AddSubAsset<DarkZoneStage>(profile, "Dark Zones");
            dark.darkness = 0.22f;
            profile.stages.Add(dark);

            profile.stages.Add(EditorAssetUtil.AddSubAsset<ConnectivityStage>(profile, "Connectivity"));

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        public static T Get<T>(BackroomsProfile profile) where T : LayoutStage
        {
            foreach (var stage in profile.stages)
            {
                if (stage is T match) return match;
            }
            return null;
        }
    }
}
