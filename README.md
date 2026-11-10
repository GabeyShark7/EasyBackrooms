# EasyBackrooms

An open source, drop-in backrooms generator for Unity.

Endless seeded yellow hallways in one click. Every part of the generator is a swappable module, so you can replace any piece without touching the rest.

## Requirements

- Unity 6000.3 or newer
- Universal Render Pipeline (URP)
- Input System package (set Active Input Handling to the new Input System)

## Install

**Package Manager:** Window > Package Manager > + > Add package from git URL:

```
https://github.com/GabeyShark7/EasyBackrooms.git?path=/Assets/EasyBackrooms
```

**Or copy** the `Assets/EasyBackrooms` folder into your project.

## Quick start

1. Open an empty scene.
2. In the Hierarchy, right click > **EasyBackrooms > Complete Backrooms**.
3. Press Play.

That creates a profile, a theme, the world, the lights, the atmosphere and a first person player. Click to grab the mouse, Esc to let go.

| Key | Action |
|---|---|
| WASD | Move |
| Mouse | Look |
| Shift | Sprint |
| Ctrl or C | Crouch |
| Esc | Free the mouse |

Gamepads work too.

## How it is built

**Profile** decides the layout. It holds the seed, the chunk size and a list of **stages** that run in order on every chunk:

| Stage | What it does |
|---|---|
| Doorway Border | Walls on chunk edges, always with gaps, seeded by world position so neighbors match |
| Random Walls | Straight wall runs |
| Open Rooms | Big empty areas |
| Corridors | Long one cell hallways |
| Pillars | Pillar halls inside rooms |
| Light Panels | Where ceiling lights go, some burnt out |
| Dark Zones | Rare areas where every light is dead |
| Biome Switch | Runs different stage lists in different parts of the world |
| Connectivity | Knocks out just enough walls so every spot can be reached. Keep it last |

**Theme** decides the look. It holds a list of **surface builders**, each with its own material: Floor, Ceiling, Walls, Pillars, Baseboard, Light Panels and Dead Panels.

**Backrooms World** puts it in the scene. Mode **Infinite** streams chunks around the player forever. Mode **Fixed** builds one area with walls around it, in the editor or on play.

Profiles and themes show every stage and surface inline. Tweak a value and the world rebuilds.

### Presets

Project window > Create > EasyBackrooms > Presets: Classic Halls, Pillar Rooms, Tight Maze, Open Halls, Lights Out.

## Bring your own assets

EasyBackrooms finds textures by name. Put a folder anywhere with:

```
carpet_color.png         carpet_normal.png         carpet_rough.png
ceiling_tiles_color.png  ceiling_tiles_normal.png  ceiling_tiles_rough.png
wallpaper_color.png      wallpaper_normal.png      wallpaper_rough.png
```

Then Create > EasyBackrooms > Default Theme. Normal maps and roughness maps are set up automatically, and roughness is converted to the smoothness URP wants.

Light panel textures are generated. Create > EasyBackrooms > Light Panel Textures, tweak, press Generate.

Sounds are found by name too:

| Name contains | Used for |
|---|---|
| `backrooms_buzz` | Fluorescent hum on nearby panels |
| `step` | Footsteps |
| `flicker` | Crackle when a light flickers |
| `ambient` | Room tone everywhere |

## Lighting

Every lit panel inside the fog gets a real downward spot light that fades with distance, so you never see lights switch on. The nearest ones cast soft shadows. Tools > EasyBackrooms > Setup Realistic Lighting tunes your URP asset for soft shadows and ambient occlusion (the Atmosphere menu runs it for you).

## Extending

**Your own stage:**

```csharp
using EasyBackrooms;
using UnityEngine;

[CreateAssetMenu(menuName = "EasyBackrooms/Stages/Puddles")]
public class PuddleStage : LayoutStage
{
    [Range(0, 1)] public float chance = 0.05f;

    public override void Apply(ChunkLayout layout, ref DetRandom rng)
    {
        for (int y = 0; y < layout.Size; y++)
        for (int x = 0; x < layout.Size; x++)
            if (rng.Chance(chance)) layout.SetFlag(x, y, CellFlags.User1, true);
    }
}
```

Always use the `rng` you are given so the same seed gives the same world.

**Your own surface:** inherit `SurfaceBuilder` and add quads or boxes to the `MeshData` you are given. Override `CollectLightPoints` if your surface should get real lights.

**Spawning things:** add a **Prop Spawner** for prefabs, or listen to the world's `chunkLoaded` and `chunkUnloaded` events.

**AI:** add **EasyBackrooms > NavMesh** to keep a NavMesh baked around the player.

## License

MIT. See [LICENSE](LICENSE).
