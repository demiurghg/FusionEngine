# Fusion Engine Version 0.05 Release Notes

## New features
 1. Particles now uses only premultiplied blend state.
 2. Added new types of particles: Particle Beams.
 3. Added client state change event.
 4. Sprite clipping.
 5. Frames content clipping
 5. Added local sounds in SoundWorld.
 
## Fixes
 1. Fixed choppy sound through XAudio operation sets.
 2. Fixed file share error while simultaneous asset loading.

## Notes
Changes are breaking.
 1. Some SpriteLayer's functions are renamed.
 2. Some SpriteFont's functions are renamed.
 3. Some Frame's functions are renamed and have new parameters.



# Fusion Engine Version 0.04 Release Notes

## New features
 1. net.ShowLatency now enables and disables printing if latency updated.
 2. SSAO and HBAO
 3. Network Atoms
 
## Fixes
 1. Fixed odd crash in TextureAtlas (#24).
 2. Frames now check double click width and height
 3. SoundWorld now updates world state.

## Notes
There are no breaking changes.

 
 
# Fusion Engine Version 0.03 Release Notes

## New features
 1. Double click in Frames.
 2. Server side lag calculation
 3. GameClient now receives server GameTime
 4. GameClient 
 5. Fixed timestep on server side with TargetFrameRate
 6. Debug render with traces.
 
## Fixes
 1. Console would not open if non-english keybaord layout is active.
 2. FrameProcessor.TargetFrame was never assigned.

## Notes
Changes are breaking.
 1. Set server TargetFrameRate

 
 
# Fusion Engine Version 0.02 Release Notes

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
 
 