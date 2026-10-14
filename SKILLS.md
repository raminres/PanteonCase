# Panteon Case Working Skills

This file is a project playbook, not a list of installed tools. Use it to keep manual and assisted work consistent.

## Layer extraction and masking

Use a non-destructive image editor workflow. Export visible layers as transparent PNGs with a shared canvas size and origin. Make dedicated grayscale/alpha masks for gold, water, foam/ripples, and selected wind-animated foliage. Verify a static reassembly before importing into Unity.

## uGUI composition

Use a Screen Space Overlay Canvas unless a specific requirement needs another mode. Configure Canvas Scaler for `Scale With Screen Size`, a 1080 x 1920 reference resolution, and a documented match setting. Keep visual layers as clearly named `Image` children of one prefab root, ordered explicitly in the hierarchy.

## UI shader implementation

Start from a minimal URP-compatible UI shader and retain support for `Image` tint/alpha. Keep texture samples sparse and use `_Time` or one controller-provided time value for animation. Use masks to constrain movement and highlight effects. Check clipping/masking behavior if `RectMask2D` is used.

For this case, make "refraction" a gentle, local UV distortion of the already supplied art; do not reach for a screen-color texture or full-screen blur. That is both more reliable in Canvas UI and substantially friendlier to mobile GPUs.

## Effects

- **Water:** slow UV flow, very small normal/noise offset, a moving reflection band, and separate foam/ripple motion. Keep it readable rather than physically realistic.
- **Sky/clouds:** barely perceptible drift or parallax; use long loops and low contrast.
- **Foliage:** separate only a handful of leaves/vines and animate from convincing pivots. Vary phase and amplitude slightly.
- **Gold shine:** mask a soft diagonal gradient sweep to the gold layer. Expose color, intensity, width, angle, speed, softness, and interval.

## Controller design

Use one `StylizedSceneController` on the prefab root. It coordinates enable flags and shared timing, owns references explicitly, and exposes grouped parameters. Cache references in `Awake`/`OnEnable`; do not perform lookup work every frame. Avoid global state so multiple prefab instances can coexist.

## Profiling and validation

After every major effect, check:

1. Game view at 1080 x 1920 and 1536 x 2048.
2. Unity Stats and Frame Debugger for added draw calls/material splits.
3. Alpha overdraw risk from large transparent UI layers.
4. Whether pausing/disabling an effect returns the art to its intended static state.

Record the evidence and decisions in the project README. Avoid claims about mobile performance until they are backed by the project’s own measurements.

## Submission readiness

Before packaging, test the prefab in a clean Unity 6.3.23f1 URP project. Include the demo scene, prefab, layers/masks required at runtime, materials, shaders, scripts, and concise documentation. Produce a short capture of the animated scene and preserve Frame Debugger screenshots/notes for the technical explanation.
