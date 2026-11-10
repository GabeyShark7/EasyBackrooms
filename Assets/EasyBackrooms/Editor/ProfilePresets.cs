using UnityEditor;

namespace EasyBackrooms.Editor
{
    // Ready made layouts to start from
    public static class ProfilePresets
    {
        const string Root = "Assets/Create/EasyBackrooms/Presets/";

        [MenuItem(Root + "Classic Halls", priority = 10)]
        static void Classic() => Make("ClassicHalls", p => { });

        [MenuItem(Root + "Pillar Rooms", priority = 11)]
        static void PillarRooms() => Make("PillarRooms", p =>
        {
            DefaultProfileMenu.Get<RandomWallsStage>(p).runs = 12;
            var rooms = DefaultProfileMenu.Get<OpenRoomStage>(p);
            rooms.maxRooms = 4;
            rooms.roomChance = 0.9f;
            rooms.minSize = 5;
            rooms.maxSize = 11;
        });

        [MenuItem(Root + "Tight Maze", priority = 12)]
        static void TightMaze() => Make("TightMaze", p =>
        {
            var walls = DefaultProfileMenu.Get<RandomWallsStage>(p);
            walls.runs = 60;
            walls.minLength = 2;
            walls.maxLength = 5;
            DefaultProfileMenu.Get<OpenRoomStage>(p).maxRooms = 0;
            DefaultProfileMenu.Get<ConnectivityStage>(p).extraOpenings = 0.02f;
            if (p.border is DoorwayBorderStage border) border.wallChance = 0.75f;
        });

        [MenuItem(Root + "Open Halls", priority = 13)]
        static void OpenHalls() => Make("OpenHalls", p =>
        {
            DefaultProfileMenu.Get<RandomWallsStage>(p).runs = 8;
            var rooms = DefaultProfileMenu.Get<OpenRoomStage>(p);
            rooms.maxRooms = 3;
            rooms.roomChance = 0.8f;
            rooms.minSize = 6;
            rooms.maxSize = 12;
            DefaultProfileMenu.Get<PillarStage>(p).Active = false;
            if (p.border is DoorwayBorderStage border) border.wallChance = 0.35f;
        });

        [MenuItem(Root + "Lights Out", priority = 14)]
        static void LightsOut() => Make("LightsOut", p =>
        {
            DefaultProfileMenu.Get<DarkZoneStage>(p).darkness = 0.55f;
            DefaultProfileMenu.Get<LightPanelStage>(p).deadChance = 0.3f;
        });

        static void Make(string name, System.Action<BackroomsProfile> tweak)
        {
            var profile = DefaultProfileMenu.CreateProfile(EditorAssetUtil.SelectedFolder() + "/" + name + ".asset");
            tweak(profile);

            foreach (var stage in profile.stages) EditorUtility.SetDirty(stage);
            if (profile.border != null) EditorUtility.SetDirty(profile.border);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            EditorAssetUtil.Select(profile);
        }
    }
}
