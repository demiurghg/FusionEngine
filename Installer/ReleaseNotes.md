# Fusion Engine Version 0.2 Release Notes

## New features
 1. GPU particles.
 2. Sound system.
 3. Game loader for client-side async contehnt loading.
 4. Improved pipeline state.
 5. Now GUIDs identifies client on server side.
 6. Client activation and deactivation server notification
 7. RenderSystem is initialized with created RenderWorld instance.

## Fixes
 1. Fixed enable and disable of keyboard scanning.
 2. Fixed mesh instance visibility.
 3. Fixed mouse clipping.

## Renamings 
 1. GraphicsEngine renamed to RenderSystem
 2. ViewLayerHdr renamed to RenderWorld.
 
## Notes
Changes are breaking.
 1. Add class inherited from GameLoader
 2. Re-override methods in GameClient and GameServer
 3. Note that RenderWorld already exists.
 
 