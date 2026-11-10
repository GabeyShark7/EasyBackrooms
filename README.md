# EasyBackrooms

An open source, drop-in backrooms generator for Unity.

Endless seeded yellow hallways in a few clicks. Every part of the generator is a swappable module, so you can replace any piece without touching the rest.

> Work in progress. Not ready for use yet.

## Requirements

- Unity 6000.3 or newer
- Universal Render Pipeline (URP)
- Input System package

## How it is built

The generator is a pipeline of small modules that run in order for each chunk:

1. **Layout**: decides which cells are floor, wall, pillar or room
2. **Rooms**: carves room types into the layout
3. **Mesh**: builds floors, walls and ceilings
4. **Decor**: places lights and props
5. **Hooks**: events you can use to spawn your own stuff

Each module is a ScriptableObject. Swap one out, reorder them, or write your own.

## Bring your own assets

EasyBackrooms ships with no textures or sounds of its own. Plug your materials and audio into a theme asset and the generator uses them.

## Folder layout

```
Assets/EasyBackrooms/
  Art/       default textures and sounds
  Runtime/   generator code
  Editor/    inspectors and menu items
```

## License

MIT. See [LICENSE](LICENSE).
