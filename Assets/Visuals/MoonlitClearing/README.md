# Moonlit clearing — first environment study

This records the original clearing milestone. The active scene now includes the broader
presentation pass described in `../FirstPass/README.md`; its newer counts and tuning notes apply.

Open `Assets/Scenes/MoonwingForest.unity`. The scene already contains the authored assets;
no generation command is needed. Use **Game view** for the intended composition. In Scene
view, enable scene lighting and effects/fog to see the atmosphere.

The `Moonlit Clearing - Visual Study` hierarchy contains:

- Framing trees: four reusable curved-trunk/individual-leaf variants, arranged around an open clearing.
- Botanical borders: feathered ferns, six-petal moonflowers, and opaline mushroom families.
- Mossy stones: smooth irregular slate groups.
- Moonlight and motes: global URP Volume, sparse fireflies, and a low backdrop mist system.
- Clearing floor: a non-colliding visual surface just above the original ground.

The existing camera framing and gameplay roots, scripts, prefab connections, colliders,
UI, and one-assassin testing value are preserved. Existing directional-light appearance,
camera post-processing, and scene fog/ambient/sky settings provide the new atmosphere.
The original ground remains in place with its original collider and material. The two original
`Tree_1` mesh renderers are disabled, with a new silverbranch tree dressing that location; the
original tree objects, transforms, meshes, materials, and colliders are retained.

## Tuning

- `Settings/Moonwing Clearing Atmosphere.asset`: bloom, exposure, contrast, tonemapping, vignette.
- `Materials/Moonwing Starfield.mat`: moon direction; its shader supplies the night gradient and stars.
- `Materials/Lavender Canopy.mat`: canopy tint and wind amplitude.
- `Materials/Moonflower Petals.mat` and `Luminous Mushroom Caps.mat`: emission intensity.
- `Quiet Pixie Drift`: particle density and size, capped at 85 particles.
- `Low Mist - Back Tree Line`: sparse additive haze, capped at 18 particles.
- Lighting window / Environment: fog density and ambient colors.

## Scope and performance

All meshes are generated once in the editor and stored as assets. No runtime forest-generation
script, additional package, downloaded art, or new scenery collider is used. Leaves and small
plants do not cast shadows. Trunks and stones use URP Lit and cast shadows. Foliage uses a
small opaque, double-sided URP shader with vertex colors and gentle wind; it receives main-light
shadows. Glow comes from emission and bloom, not one real-time light per plant.

The visual layer intentionally does not enforce movement boundaries. Existing AI moves directly
and NPCs wander freely, so sufficiently long excursions can still intersect decorative scenery.
This sample does not add obstacle avoidance or change spawn rules.

PC and Mobile/Web quality assets are unchanged. The style still needs a real Web build performance
check on the intended device; a desktop preview is not a Web performance guarantee.

## Validation performed

Unity 6000.6.3f1 compiled the authoring code and rendered the clearing on the existing PC and Mobile
quality profiles in an isolated project copy. Asset validation checked shaders, required gameplay
references, and the absence of added colliders. A Play mode smoke check exercised shooting, camera
follow, NPC wandering/conversation, assassin spawn/chase/health, magic drain and meter updates, and
victory/capture panels with their time freezes. It did not simulate keyboard movement input or
measure browser frame rate.

The generated sample contains 31 tree instances, 8 reusable prefab assets, 16 stored meshes,
10 materials, 4 shaders, and one volume profile. Across all instances, the visual layer contains
202 renderers and approximately 649,000 mesh triangles before camera culling. Reused meshes keep
asset size down; this count is a scene inventory, not a performance measurement.

## Authoring source

`Editor/MoonlitClearingBuilder.cs` is an editor-only, deterministic mesh/layout authoring utility.
It never runs automatically. Its menu command refuses to run when this clearing already exists,
protecting hand edits. Edit the saved scene/prefabs normally for artistic iteration.

The sky and fog are scene-wide for a coherent view. The authored vegetation forms this single
clearing with a shallow scenic tree backdrop. The visual ground extension behind it has no collider
and does not extend the playable ground. Character redesign, the home, UI styling, and expansion
beyond this sample are deliberately deferred.
