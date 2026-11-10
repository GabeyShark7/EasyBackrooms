# Changelog

## 0.1.0

First release.

- Seeded layout generator built from swappable stages: random walls, open rooms, corridors, pillars, light panels, dark zones, biome switching and connectivity
- Chunk edges seeded by world position so neighbors always line up
- Theme asset of swappable surface builders: floor, ceiling, walls, pillars, baseboard, lit and dead light panels
- Infinite streaming with pooling and a per frame budget, or a Fixed area with sealed walls
- Real spot lights on every visible panel, distance fading, soft shadows on the nearest lights, flicker patterns and positional buzz
- Atmosphere preset with fog, fake bounce light, bloom, grain, vignette and color grading, plus a one click URP lighting setup
- Procedural light panel texture generator
- First person player with sprint, crouch, head bob and gamepad support
- Footstep, flicker crackle and ambient loop audio slots
- Prop spawner and runtime NavMesh around the player
- Complete Backrooms menu, profile presets, inline stage editing with live rebuilds and a demo scene maker
