# Rock And River Refactor Handoff

> **River Foam supersession notice — 2026-07-07**
>
> This handoff is historical for earlier rock/river refactor context. Any river Foam architecture, static-foam, morphing, lateral motion, or rendering responsibility statement in this document is superseded by `Docs/River_Foam_Stage6_Architecture.md` and `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`. Use those documents as the active Foam source of truth.

## Current River Foam continuation — `4.11C.5.16C.1` source implementation

This section supersedes later historical River Foam movement/history statements wherever they conflict.

`4.11C.5.16A.1`, `4.11C.5.16B`, and `4.11C.5.16B.1` are accepted for progression. The active source now contains `4.11C.5.16C — Advected Layer D Temporal Occupancy` plus `4.11C.5.16C.1 — Debug Footprint Consistency`. Unity screenshots provide provisional runtime evidence, but the stationary Temporal Difference convergence test and final acceptance remain pending.

Current Layer C movement truth remains:

```text
canonical local velocity moves the complete packed FoamState;
shared donor-cell face fluxes provide longitudinal and lateral movement;
shore, obstacle, invalid, and lateral exterior faces are no-flux;
endpoint outflow follows physical flow direction;
CFL substeps target 0.90 with a 64-substep safety ceiling;
lifecycle aging is distributed across those substeps;
births are applied after completed transport;
open-water render smoothing is local residual-time backtracing.
```

Current Layer D temporal-sheet truth:

```text
Film Source and Film Support remain half-resolution material-derived helpers;
two additional half-resolution RHalf textures ping-pong temporal occupancy;
occupancy uses the same canonical velocity and Layer C substep count;
occupancy uses closed shore/obstacle/invalid faces and flow-aware endpoint outflow;
Build Time defaults to 0.20 s and Release Time defaults to 0.80 s;
_FoamShapeMask combines committed Presence with temporal occupancy;
new Target, Occupancy, and Difference views expose the field;
history clears when Layer D diagnostics are re-entered;
Final Foam remains disconnected and unchanged.
```

The temporal field is visual-only. It does not create or move durable material, change Remaining Life, alter Material Pattern, feed Layer B/C, or extend source lifetime. Its purpose is to provide the persistent moving visual sheet that `5.16D` can pinch, tear, split, rejoin, and fracture.

Current debug-footprint truth:

```text
all relevant views use the same foam.fieldUV;
Material Presence remains raw stored amplitude;
Motion Field ownership uses smoothstep(0.02, 0.16, Presence) at partial opacity;
Remaining Life is normalized life multiplied by the same meaningful-Presence gate;
Evaluated Shape and Temporal Occupancy are intentionally broader Layer D products;
5.16C.1 changes diagnostics only and does not alter simulation or Final Foam.
```

Primary `5.16C` files:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

The canonical velocity files remain unchanged:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
```

Required next action is combined Unity validation of `5.16C/5.16C.1`, including stationary Temporal Difference convergence and footprint comparison. Do not begin `5.16D` damage/fracture or `5.16E` Final Foam integration until temporal occupancy is proven stable, correctly advected, clipped, reversible, convergent, visually useful, and diagnostically comparable.


## Purpose

This document is a refactor-grade handoff for the current generated-rock and river files requested from the workspace.

It is intentionally much more detailed than the earlier handoff. The goal is to give a future refactor owner enough context to:

- identify the correct files quickly;
- understand current responsibilities and coupling;
- see the important serialized data and execution paths;
- understand where colliders are created or assigned;
- understand the current river and static-foam behavior boundaries;
- work from substantial inlined file content without constantly tabbing back into the IDE.

This handoff reflects the current files on disk in the workspace.

## Requested Assets And Current Locations

### Rock generation

- Generated rock component and collider assignment:
  [GeneratedMass.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Masses/GeneratedMass.cs)
- Rock generation data/settings:
  [GeneratedMass.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Masses/GeneratedMass.cs)
  `MassRecipe` and its related enums live in the same file.
- Rock mesh generator:
  [MassGenerator.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Masses/MassGenerator.cs)

### River files requested

- River body/current controller:
  [StylizedRiver.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Rivers/StylizedRiver.cs)
- Static foam generator:
  [StylizedRiverStaticFoam.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Rivers/StylizedRiverStaticFoam.cs)

## High-Level System Map

### Rock side

The "rock" system currently uses the `Masses` naming rather than `Rocks`.

The main runtime/editor-facing component is `GeneratedMass`. It owns the recipe, invokes procedural mesh generation, stores the generated mesh, and assigns both render mesh and collider mesh.

The geometry builder is `MassGenerator`. It accepts a `MassRecipe` and returns `MeshData`. `GeneratedMass` then passes that `MeshData` into `MeshBuilder.ApplyToMesh(...)`.

### River side

`StylizedRiver` owns:

- spline sampling;
- surface mesh generation;
- current-accent mesh generation;
- shader/material property application;
- animation clocks;
- coordination with ground generation;
- coordination with attached static foam.

`StylizedRiverStaticFoam` owns:

- collider detection;
- projecting colliders onto river space;
- generating contact arcs and wake ribbons;
- managing a separate foam mesh child object;
- applying current-style shader properties for foam rendering.

## Important Current Constraints

### River Body Flow constraint

The current tiled Body Flow path is intentionally provisional. The in-code comments currently state:

- do not replace it with one independently generated mask per chunk;
- future replacement must depend on global connected-river distance;
- Body Detail and white Current Accents remain approved and active.

That guidance is present in [StylizedRiver.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Rivers/StylizedRiver.cs:78) and in the shader-side comments already added elsewhere.

### Static foam constraint

The current static foam pass is intentionally driven only by the explicit `RiverFoamStatic` layer-mask setup. The current in-code note in [StylizedRiverStaticFoam.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Rivers/StylizedRiverStaticFoam.cs:18) says not to broaden detection to tags, names, or generalized heuristics.

## Rock System: File Responsibilities

### `GeneratedMass.cs`

Primary responsibilities:

- defines the rock-related enums;
- defines the serialized `MassRecipe` data class;
- applies archetype defaults;
- exposes editor context-menu actions;
- holds the generated mesh instance;
- assigns the render mesh;
- assigns the collider mesh;
- cleans up temporary generated mesh ownership on destroy.

This is both:

- the rock-generation component the user attaches to a GameObject;
- the place where the rock collider is required and assigned.

### `MassGenerator.cs`

Primary responsibilities:

- procedural rock mesh construction;
- archetype-dependent builder selection;
- deterministic randomization from seeds;
- macro profile selection;
- major and secondary cuts;
- triangulation and cleanup;
- scale, lean, grounding, and recentering;
- final conversion to `MeshData`.

It is long and algorithm-heavy. For a refactor, it is best thought of as:

- public entry point layer;
- plane-cut builder path;
- polished/radial builder path;
- cleanup/sanitization utilities;
- tuning tables and mappings near the end of the file.

## River System: File Responsibilities

### `StylizedRiver.cs`

Primary responsibilities:

- caches Unity components and spline container;
- manages generated river surface mesh;
- manages generated current-accent mesh;
- controls live regeneration and delayed editor rebuild;
- applies body and current material properties;
- advances animation over time;
- exposes surface and accent counts;
- refreshes attached static foam after river rebuild;
- provides shared spline samples to foam and ground systems.

### `StylizedRiverStaticFoam.cs`

Primary responsibilities:

- detects candidate colliders within a river-shaped search volume;
- filters candidates by explicit layer mask and surface crossing;
- projects candidates into along-river/across-river coordinates;
- produces foam interactions;
- generates contact arcs and wake ribbons into a new mesh;
- owns the foam child mesh output object;
- applies rendering properties derived partly from the river's visual seed.

## Rock Generation Data Model

The current rock settings live in `GeneratedMass.cs`.

### Enums

The following enums are part of the live shape description:

- `MassArchetype`
- `MassScaleStep`
- `FormComplexity`
- `SurfaceFacetDensity`
- `EdgeCharacter`
- `ShapeDiversity`
- `GroundingStyle`
- `LeanStyle`

### Current supported archetypes

```csharp
public enum MassArchetype
{
    TerrainBoulder,
    SquatBoulder,
    StandingStone,
    FlatSlab,
    BrokenChunk,
    PolishedStone
}
```

### `MassRecipe` serialized fields

Identity:

- `archetype`
- `shapeSeed`
- `surfaceSeed`

Primary shape:

- `size`
- `formComplexity`
- `surfaceFacetDensity`
- `edgeCharacter`
- `shapeDiversity`
- `grounding`
- `lean`

Advanced proportions:

- `fineScale`
- `widthBias`
- `heightBias`
- `depthBias`

Surface data:

- `surfaceVariation`

### Current `MassRecipe` source excerpt

```csharp
[Serializable]
public sealed class MassRecipe
{
    public const int MinimumSeed = 1;
    public const int MaximumSeed = 9999;

    [Header("Identity")]
    [SerializeField]
    private MassArchetype archetype = MassArchetype.TerrainBoulder;

    [Tooltip("Controls proportions, major planes, cuts, lean and silhouette.")]
    [Range(MinimumSeed, MaximumSeed)]
    [SerializeField]
    private int shapeSeed = 1234;

    [Tooltip("Controls surface triangulation, subtle facet relief and vertex-colour variation.")]
    [Range(MinimumSeed, MaximumSeed)]
    [SerializeField]
    private int surfaceSeed = 5678;

    [Header("Primary Shape")]
    [SerializeField]
    private MassScaleStep size = MassScaleStep.M;

    [Tooltip("Controls the number of major cuts and dominant planes.")]
    [SerializeField]
    private FormComplexity formComplexity = FormComplexity.Moderate;

    [Tooltip("Controls triangulation across the major planes.")]
    [SerializeField]
    private SurfaceFacetDensity surfaceFacetDensity = SurfaceFacetDensity.Medium;

    [Tooltip("Controls how strongly major edges are preserved or worn down.")]
    [SerializeField]
    private EdgeCharacter edgeCharacter = EdgeCharacter.Natural;

    [Tooltip("Controls how far different shape seeds may depart from the archetype's base form.")]
    [SerializeField]
    private ShapeDiversity shapeDiversity = ShapeDiversity.Broad;

    [SerializeField]
    private GroundingStyle grounding = GroundingStyle.Stable;

    [SerializeField]
    private LeanStyle lean = LeanStyle.Subtle;

    [Header("Advanced Proportions")]
    [Tooltip("Fine adjustment within the selected size step.")]
    [Range(0.85f, 1.15f)]
    [SerializeField]
    private float fineScale = 1f;

    [Range(0.7f, 1.3f)]
    [SerializeField]
    private float widthBias = 1f;

    [Range(0.7f, 1.3f)]
    [SerializeField]
    private float heightBias = 1f;

    [Range(0.7f, 1.3f)]
    [SerializeField]
    private float depthBias = 1f;

    [Header("Surface Data")]
    [Tooltip("Amount of deterministic variation written into vertex colour red.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float surfaceVariation = 0.35f;
}
```

### Archetype defaults

The current defaulting logic is built into `MassRecipe.ApplyArchetypeDefaults()`. This means:

- archetype selection is not just descriptive;
- changing archetype can implicitly rewrite several other settings;
- `GeneratedMass` tracks `lastAppliedArchetype` and `recipeInitialized` to manage that behavior.

Current behavior by archetype:

- `TerrainBoulder`: moderate complexity, medium facets, natural edges, broad diversity, stable grounding, subtle lean.
- `SquatBoulder`: moderate complexity, medium facets, natural edges, broad diversity, embedded grounding, no lean.
- `StandingStone`: simple complexity, low facets, sharp edges, broad diversity, stable grounding, subtle lean.
- `FlatSlab`: simple complexity, low facets, sharp edges, restrained diversity, embedded grounding, no lean.
- `BrokenChunk`: complex complexity, medium facets, chipped edges, wild diversity, stable grounding, subtle lean.
- `PolishedStone`: simple complexity, medium facets, polished edges, broad diversity, stable grounding, no lean.

## Rock Component Execution Flow

### Lifecycle summary

`GeneratedMass` calls `Regenerate()` from:

- `OnEnable()`
- `OnValidate()` when `regenerateOnValidate` is true
- explicit context-menu methods indirectly after changing seeds
- `ResetRecipeToArchetype()`

### Current core flow

```csharp
private void OnEnable()
{
    EnsureRecipeState();
    CacheComponents();
    Regenerate();
}

private void OnValidate()
{
    EnsureRecipeState();

    if (!regenerateOnValidate)
    {
        return;
    }

    CacheComponents();
    Regenerate();
}
```

### Regeneration and collider assignment

This is the most important path for a refactor because it spans generation ownership, mesh naming, render assignment, and collider assignment:

```csharp
[ContextMenu("Regenerate Mass")]
public void Regenerate()
{
    CacheComponents();

    if (recipe == null)
    {
        ClearGeneratedAssignments();
        return;
    }

    EnsureGeneratedMesh();

    MeshData meshData = MassGenerator.Generate(recipe);

    string meshName =
        $"GeneratedMass_{recipe.Archetype}_Shape{recipe.ShapeSeed}_Surface{recipe.SurfaceSeed}";

    MeshBuilder.ApplyToMesh(
        meshData,
        generatedMesh,
        meshName);

    meshFilter.sharedMesh = generatedMesh;

    meshCollider.sharedMesh = null;
    meshCollider.sharedMesh = generatedMesh;
    meshCollider.convex = false;
}
```

### Collider-specific notes

The rock collider is not created in a separate helper script. The current collider behavior is embedded directly into `GeneratedMass`.

Important lines:

- `[RequireComponent(typeof(MeshCollider))]`
- cached field `private MeshCollider meshCollider;`
- assignment:
  `meshCollider.sharedMesh = null;`
  `meshCollider.sharedMesh = generatedMesh;`
  `meshCollider.convex = false;`

That makes `GeneratedMass.cs` both:

- the rock spawner/generator component;
- the rock collider assignment script.

## MassGenerator: Entry Points And Core Structure

### Public entry point

```csharp
public static MeshData Generate(MassRecipe recipe)
{
    if (recipe == null)
    {
        throw new ArgumentNullException(nameof(recipe));
    }

    Vector3 dimensions = ResolveDimensions(recipe);

    TriangleSoup soup = UsesRadialBuilder(recipe.Archetype)
        ? BuildRadialMass(recipe)
        : BuildPlaneCutMass(recipe);

    ApplyDimensions(soup.Positions, dimensions);
    ApplyLean(soup.Positions, recipe.Lean, recipe.ShapeSeed);
    ApplyGrounding(soup.Positions, recipe.Grounding);
    RecenterOnGround(soup.Positions);

    return BuildMeshData(soup, recipe);
}
```

### Builder split

The archetype split is currently very simple:

```csharp
private static bool UsesRadialBuilder(MassArchetype archetype)
{
    return archetype == MassArchetype.PolishedStone;
}
```

Implication:

- all archetypes except `PolishedStone` use the plane-cut path;
- `PolishedStone` is the only radial-builder archetype in the current implementation.

### Important current tuning constants

At the top of the file:

```csharp
private const float PlaneEpsilon = 0.0001f;
private const float PointMergeDistance = 0.00001f;
private const float PointMergeDistanceSqr =
    PointMergeDistance * PointMergeDistance;

private const float RelativeCollinearEpsilon = 0.0000000001f;
private const float RelativeTriangleAreaEpsilon = 0.000000000001f;
private const float MinimumEdgeLengthSqr = 0.000000000001f;
private const float TinyFaceAreaEpsilon = 0.0000000001f;
```

These are strong refactor hotspots because they directly affect:

- clipping stability;
- polygon sanitization;
- welding behavior;
- tiny-face removal;
- degeneracy thresholds.

### Plane-cut path summary

The current plane-cut generation pipeline is:

1. Create a deterministic random stream from `shapeSeed`.
2. Create asymmetric box extents.
3. Create the initial box faces.
4. Select a macro profile.
5. Apply profile cuts.
6. Compute major cut count from `FormComplexity`.
7. Compute cut depth range from `ShapeDiversity`.
8. Apply archetype and edge-character multipliers.
9. Apply major cuts.
10. Apply secondary chips.
11. Triangulate the resulting polyhedron with the chosen surface density and edge character.

Key excerpt:

```csharp
private static TriangleSoup BuildPlaneCutMass(MassRecipe recipe)
{
    System.Random shapeRandom =
        CreateRandom(recipe.ShapeSeed, 0x27101987);

    BoxExtents box = CreateBoxExtents(
        shapeRandom,
        recipe.ShapeDiversity,
        recipe.Archetype);

    List<PolygonFace> faces = CreateBoxFaces(box);

    MacroProfile profile = SelectMacroProfile(
        shapeRandom,
        recipe.Archetype,
        recipe.ShapeDiversity);

    ApplyProfileCuts(
        faces,
        box,
        profile,
        shapeRandom,
        recipe);

    GetMajorCutCountRange(
        recipe.FormComplexity,
        out int minimumCuts,
        out int maximumCuts);

    int majorCutCount = shapeRandom.Next(
        minimumCuts,
        maximumCuts + 1);

    GetCutDepthRange(
        recipe.ShapeDiversity,
        out float minimumDepth,
        out float maximumDepth);

    float archetypeDepthMultiplier =
        GetArchetypeCutDepthMultiplier(recipe.Archetype);

    float edgeDepthMultiplier =
        GetEdgeCutDepthMultiplier(recipe.EdgeCharacter);

    for (int i = 0; i < majorCutCount; i++)
    {
        CutRegion region = SelectCutRegion(i, shapeRandom);
        Vector3 normal = RandomCutNormal(shapeRandom, region);

        float depth = RandomRange(
            shapeRandom,
            minimumDepth,
            maximumDepth);

        depth *= archetypeDepthMultiplier;
        depth *= edgeDepthMultiplier;
        depth = Mathf.Clamp(depth, 0.04f, 0.46f);

        ApplyCut(faces, normal, depth);
    }

    int chipCount = GetSecondaryChipCount(
        recipe.EdgeCharacter,
        recipe.FormComplexity);

    for (int i = 0; i < chipCount; i++)
    {
        Vector3 normal = RandomCutNormal(
            shapeRandom,
            CutRegion.Any);

        float depth = RandomRange(
            shapeRandom,
            0.035f,
            0.09f);

        ApplyCut(faces, normal, depth);
    }

    return TriangulatePolyhedron(
        faces,
        recipe.SurfaceFacetDensity,
        recipe.EdgeCharacter,
        recipe.SurfaceSeed);
}
```

### Important tuning mappings near end of file

These functions are useful to keep in mind during refactor because they encode much of the artistic behavior in compact lookup form:

- `GetBaseDimensions(...)`
- `GetSizeMultiplier(...)`
- `GetMajorCutCountRange(...)`
- `GetCutDepthRange(...)`
- `GetArchetypeCutDepthMultiplier(...)`
- `GetEdgeCutDepthMultiplier(...)`
- `GetSecondaryChipCount(...)`
- `GetBoundarySegments(...)`
- `GetSurfaceRelief(...)`
- `GetReliefMultiplier(...)`
- `GetSurfaceFrequency(...)`
- `GetRadialRegularization(...)`
- `GetGroundingSettings(...)`

Example excerpt:

```csharp
private static Vector3 GetBaseDimensions(MassArchetype archetype)
{
    return archetype switch
    {
        MassArchetype.TerrainBoulder => new Vector3(2f, 1.45f, 1.7f),
        MassArchetype.SquatBoulder => new Vector3(2.35f, 0.95f, 2f),
        MassArchetype.StandingStone => new Vector3(1.1f, 2.8f, 0.95f),
        MassArchetype.FlatSlab => new Vector3(2.4f, 0.62f, 1.7f),
        MassArchetype.BrokenChunk => new Vector3(1.75f, 1.55f, 1.45f),
        MassArchetype.PolishedStone => new Vector3(2f, 1.35f, 1.7f),
        _ => Vector3.one
    };
}
```

```csharp
private static float GetSizeMultiplier(MassScaleStep size)
{
    return size switch
    {
        MassScaleStep.XS => 0.25f,
        MassScaleStep.S => 0.45f,
        MassScaleStep.M => 0.70f,
        MassScaleStep.L => 1f,
        MassScaleStep.XL => 1.45f,
        MassScaleStep.XXL => 2.10f,
        MassScaleStep.Monumental => 3.20f,
        _ => 1f
    };
}
```

## Current River Body Controller

### Current serialized surface/body/current fields

The serialized river state currently includes:

Setup:

- `splineContainer`
- `liveRegeneration`

Channel:

- `width`
- `bankBlend`
- `depth`
- `bedFlatness`
- `bankProfile`
- `bankOverlap`
- `carvingStrength`

Surface mesh:

- `quality`
- `surfaceOffset`

Water body:

- `shallowColor`
- `deepColor`
- `flowTint`
- `opacity`
- `flowSpeed`
- `flowScale`
- `flowStrength`
- `detailScale`
- `detailStrength`
- `waveHeight`
- `bankLight`
- `lightingSteps`

Current accents:

- `enableCurrentAccents`
- `currentColor`
- `currentIntensity`
- `currentOpacity`
- `currentSpeed`
- `currentDensity`
- `currentLength`
- `currentWidth`
- `currentCurvature`
- `currentSoftness`

Advanced:

- `bodyMaterial`
- `currentMaterial`
- `flowTexture`
- `detailTexture`
- `currentVerticalOffset`
- `visualSeed`

### Current header and important guardrail excerpt

```csharp
[Header("Water Body")]
[SerializeField] private Color shallowColor = new Color(0.42f, 0.73f, 0.73f, 1f);
[SerializeField] private Color deepColor = new Color(0.12f, 0.42f, 0.48f, 1f);
[SerializeField] private Color flowTint = new Color(0.72f, 0.92f, 0.88f, 1f);

[Range(0.15f, 1f)]
[SerializeField] private float opacity = 0.72f;

// Provisional local-slice Body Flow controls only. This tiled implementation is deferred and must not
// be replaced with one independently generated mask per map chunk. Its future replacement depends on
// global connected-river distance supplied by the procedural map assembler. Body Detail and the
// separate white Current Accents remain approved and active.
[Range(0f, 4f)]
[SerializeField] private float flowSpeed = 0.75f;
```

### Core rebuild flow

```csharp
[ContextMenu("Regenerate River and Ground")]
public void RegenerateAll()
{
    ValidateSettings();
    CacheComponents();
    ResolveSplineContainer();
    EnsureGeneratedObjects();
    ResolveDefaultTextures();
    BuildSplineSamples();
    BuildSurface();
    BuildCurrentAccents();
    RefreshAttachedStaticFoam();
    NotifyParentGround();
    ApplyVisualSettings();
}
```

This ordering matters:

- spline samples must exist before surface/current/foam;
- static foam is refreshed after surface/current geometry updates;
- ground notification is separate from foam refresh;
- visual settings are applied after geometry generation.

### Shared spline sample API

This is the current bridge into foam and other downstream systems:

```csharp
public float BuildSharedSplineSamples(
    List<StylizedRiverSplineSample> targetSamples)
{
    if (targetSamples == null)
    {
        throw new ArgumentNullException(nameof(targetSamples));
    }

    return StylizedRiverGeometry.BuildSplineSamples(
        ResolveSplineContainer(),
        ResolveSurfaceSampleSpacing(),
        targetSamples);
}
```

For refactor planning, this is already a useful seam because it externalizes the same sample set that the river uses internally.

### Surface mesh creation

```csharp
private void BuildSurface()
{
    StylizedRiverGeometry.BuildSurfaceMesh(
        transform,
        splineSamples,
        width,
        bankOverlap,
        ResolveCrossSegments(),
        surfaceOffset,
        surfaceMesh);
}
```

### Current-accent generation

```csharp
private void BuildCurrentAccents()
{
    if (!enableCurrentAccents)
    {
        generatedAccentCount = 0;

        if (currentMesh != null)
        {
            currentMesh.Clear();
        }

        return;
    }

    StylizedRiverGeometry.BuildCurrentAccentMesh(
        transform,
        splineSamples,
        riverLength,
        width,
        surfaceOffset + currentVerticalOffset,
        currentDensity,
        currentLength,
        currentWidth,
        currentCurvature,
        1f,
        currentTravelDistance,
        visualSeed,
        ResolveCurrentRows(),
        currentMesh);

    generatedAccentCount =
        currentMesh != null
            ? Mathf.Max(0, currentMesh.vertexCount / (ResolveCurrentRows() + 1) / 2)
            : 0;
}
```

### Animation clocks

```csharp
private void AdvanceAnimation(float deltaTime)
{
    if (deltaTime <= 0f)
    {
        return;
    }

    riverTime = Mathf.Repeat(riverTime + deltaTime, 4096f);
    flowDistance = Mathf.Repeat(flowDistance + flowSpeed * deltaTime, Mathf.Max(64f, flowScale * 256f));
    currentTravelDistance = Mathf.Repeat(currentTravelDistance + currentSpeed * deltaTime, Mathf.Max(64f, riverLength + currentLength * 4f));

    if (enableCurrentAccents && splineSamples.Count >= 2)
    {
        BuildCurrentAccents();
    }

    ApplyAnimationClock();
}
```

Important implication:

- current accents are actually rebuilt as animated geometry over time;
- body flow animation is shader-property based;
- current animation is split across geometry motion and shader properties.

### Body shader-property path

```csharp
private void ApplyBodyProperties()
{
    if (meshRenderer == null)
    {
        return;
    }

    bodyProperties ??= new MaterialPropertyBlock();
    bodyProperties.Clear();
    bodyProperties.SetColor(ShallowColorId, shallowColor);
    bodyProperties.SetColor(DeepColorId, deepColor);
    bodyProperties.SetColor(FlowTintId, flowTint);
    bodyProperties.SetFloat(OpacityId, opacity);
    bodyProperties.SetFloat(FlowScaleId, flowScale);
    bodyProperties.SetFloat(FlowStrengthId, flowStrength);
    bodyProperties.SetFloat(DetailScaleId, detailScale);
    bodyProperties.SetFloat(DetailStrengthId, detailStrength);
    bodyProperties.SetFloat(WaveHeightId, waveHeight);
    bodyProperties.SetFloat(BankLightId, bankLight);
    bodyProperties.SetFloat(LightingStepsId, lightingSteps);
    // Retain the current Body Flow shader inputs only as a provisional local-slice implementation.
    // The future replacement must be driven by global connected-river distance from the procedural
    // map assembler, while Body Detail and Current Accents remain part of the accepted baseline.
    bodyProperties.SetFloat(FlowDistanceId, flowDistance);
    bodyProperties.SetFloat(RiverTimeId, riverTime);
    bodyProperties.SetFloat(VisualSeedId, visualSeed);

    if (flowTexture != null)
    {
        bodyProperties.SetTexture(FlowTextureId, flowTexture);
    }

    if (detailTexture != null)
    {
        bodyProperties.SetTexture(DetailTextureId, detailTexture);
    }

    meshRenderer.SetPropertyBlock(bodyProperties);
}
```

## Current Static Foam Generator

### Detection model

The foam system is attached to a river and depends on:

- the river component;
- shared river spline samples;
- explicit layer-mask filtering;
- collider bounds crossing the water surface;
- collider projection into river-aligned space.

### Serialized settings

Detection:

- `staticFoamMask`

Foam look:

- `foamColor`
- `foamIntensity`
- `foamOpacity`
- `contactAmount`
- `wakeAmount`
- `foamScale`
- `wakeLength`
- `edgeSoftness`

Advanced:

- `foamMaterial`
- `surfaceTolerance`
- `verticalOffset`

### Current detection and rebuild entry point

```csharp
[ContextMenu("Regenerate Static Foam")]
public void RegenerateStaticFoam()
{
    ResolveRiver();
    EnsureOutput();
    ApplyVisualSettings();

    detectedColliderCount = 0;
    queryBufferFull = false;
    splineSamples.Clear();

    if (river == null ||
        staticFoamMask.value == 0)
    {
        ClearGeneratedFoam();
        return;
    }

    float riverLength =
        river.BuildSharedSplineSamples(splineSamples);

    if (riverLength <= 0.01f ||
        splineSamples.Count < 2)
    {
        ClearGeneratedFoam();
        return;
    }

    Bounds searchBounds =
        BuildSearchBounds();

    int count =
        Physics.OverlapBoxNonAlloc(
            searchBounds.center,
            searchBounds.extents,
            overlapBuffer,
            Quaternion.identity,
            staticFoamMask,
            QueryTriggerInteraction.Ignore);

    queryBufferFull =
        count >= overlapBuffer.Length;

    HashSet<EntityId> seen =
        new HashSet<EntityId>();

    List<StaticFoamInteraction> interactions =
        new List<StaticFoamInteraction>();

    for (int index = 0; index < count; index++)
    {
        Collider candidate = overlapBuffer[index];
        overlapBuffer[index] = null;

        if (!IsValidCandidate(candidate))
        {
            continue;
        }

        EntityId entityId =
            candidate.GetEntityId();

        if (!seen.Add(entityId))
        {
            continue;
        }

        if (TryCreateInteraction(
                candidate,
                out StaticFoamInteraction interaction))
        {
            interactions.Add(interaction);
        }
    }

    detectedColliderCount =
        interactions.Count;

    BuildFoamMesh(interactions);
}
```

### Candidate filtering rules

Current rejection logic:

- null collider;
- disabled collider;
- trigger collider;
- inactive GameObject;
- collider layer not in `staticFoamMask`;
- collider belonging to the river object's own child hierarchy;
- projection onto river fails;
- collider bounds do not cross the water surface tolerance band;
- projected across-distance exceeds visible river width plus projected extent.

### Interaction creation

```csharp
private bool TryCreateInteraction(
    Collider candidate,
    out StaticFoamInteraction interaction)
{
    interaction = default;

    Bounds bounds = candidate.bounds;

    if (!StylizedRiverGeometry.TryProjectPoint(
            splineSamples,
            bounds.center,
            out StylizedRiverProjection projection))
    {
        return false;
    }

    Vector3 extents = bounds.extents;

    float acrossExtent =
        ProjectAabbExtent(extents, projection.Side);

    float alongExtent =
        ProjectAabbExtent(extents, projection.Tangent);

    float waterY =
        projection.Centre.y +
        river.SurfaceOffset;

    bool crossesSurface =
        bounds.min.y <= waterY + surfaceTolerance &&
        bounds.max.y >= waterY - surfaceTolerance;

    if (!crossesSurface)
    {
        return false;
    }

    if (Mathf.Abs(projection.AcrossDistance) >
        river.VisibleHalfWidth + acrossExtent)
    {
        return false;
    }

    acrossExtent =
        Mathf.Clamp(
            acrossExtent,
            0.10f,
            Mathf.Max(
                0.10f,
                river.VisibleHalfWidth * 1.10f));

    alongExtent =
        Mathf.Clamp(
            alongExtent,
            0.10f,
            6f);

    Vector3 centre = bounds.center;
    centre.y =
        waterY +
        verticalOffset;

    interaction =
        new StaticFoamInteraction(
            centre,
            projection.DistanceAlong,
            projection.Tangent,
            projection.Side,
            acrossExtent,
            alongExtent,
            Hash01(candidate.GetEntityId()));

    return true;
}
```

### Foam mesh generation split

Current foam mesh generation uses two shape types per interaction:

- contact arc;
- wake ribbons.

The main branch point:

```csharp
for (int index = 0; index < interactions.Count; index++)
{
    StaticFoamInteraction interaction =
        interactions[index];

    if (contactAmount > 0.001f)
    {
        AddContactArc(
            interaction,
            contactSegments,
            builder);
    }

    if (wakeAmount > 0.001f)
    {
        AddWakeRibbons(
            interaction,
            wakeRows,
            builder);
    }
}
```

### Contact arc excerpt

```csharp
private void AddContactArc(
    StaticFoamInteraction interaction,
    int segmentCount,
    FoamMeshBuilder builder)
{
    const float startAngle = -102f;
    const float endAngle = 102f;

    Vector3 upstream =
        -interaction.Tangent;

    Vector3 side =
        interaction.Side;

    float obstacleRadius =
        Mathf.Max(
            interaction.AcrossRadius,
            interaction.AlongRadius);

    float bandWidth =
        Mathf.Clamp(
            (0.11f + obstacleRadius * 0.075f) *
            foamScale *
            Mathf.Lerp(0.55f, 1f, contactAmount),
            0.08f,
            0.42f * foamScale);
    ...
}
```

### Wake ribbon excerpt

```csharp
private void AddWakeRibbons(
    StaticFoamInteraction interaction,
    int rowCount,
    FoamMeshBuilder builder)
{
    float obstacleRadius =
        Mathf.Max(
            interaction.AcrossRadius,
            interaction.AlongRadius);

    float resolvedLength =
        Mathf.Clamp(
            (0.55f + obstacleRadius * 1.35f) *
            wakeLength *
            foamScale,
            0.35f,
            5f);

    float startingWidth =
        Mathf.Clamp(
            (0.07f + interaction.AcrossRadius * 0.055f) *
            foamScale *
            Mathf.Lerp(0.55f, 1f, wakeAmount),
            0.05f,
            0.28f);
    ...
}
```

## Refactor Seams And Coupling Notes

### Rock system seams

Good seam candidates:

- split enums and `MassRecipe` into a dedicated data file;
- split `GeneratedMass` lifecycle from mesh/collider assignment;
- separate `MassGenerator` tuning tables from geometry operations;
- isolate the polished/radial path from the plane-cut path;
- isolate cleanup thresholds and geometric tolerances into a dedicated config layer.

Current coupling points:

- `GeneratedMass` knows both generation and collider assignment;
- `MassRecipe` and `GeneratedMass` are co-located in one file;
- `MassGenerator` contains both algorithmic logic and art-direction tuning tables.

### River system seams

Good seam candidates:

- split river rebuild orchestration from material/shader application;
- split current-accent animation geometry from river body animation clocks;
- split static-foam detection from static-foam mesh building;
- extract shared river coordinate utilities into a narrower boundary object;
- formalize the current `BuildSharedSplineSamples(...)` bridge into a more explicit sampling service.

Current coupling points:

- `StylizedRiver` directly triggers `StylizedRiverStaticFoam.RegenerateStaticFoam()`;
- foam detection depends on several river-derived quantities:
  `SurfaceOffset`, `VisibleHalfWidth`, `Quality`, `VisualSeed`, and spline sampling;
- body-flow guardrail comments establish future design constraints that a refactor should preserve.

## Practical File Content Summary

If someone needs the shortest "what actually matters" list for the requested files:

- `GeneratedMass.cs`
  This is the generated rock component, the recipe owner, the mesh owner, and the collider assignment point.
- `MassRecipe` in `GeneratedMass.cs`
  This is the rock-generation settings/data class.
- `MassGenerator.cs`
  This is the procedural rock mesh builder.
- `GeneratedMass.cs`
  This is also the current rock collider assignment script.
- `StylizedRiver.cs`
  This is the current river controller including deferred Body Flow guardrails.
- `StylizedRiverStaticFoam.cs`
  This is the current static-foam generator using explicit layer-mask-only detection.

## Files To Keep Open During Refactor

- [GeneratedMass.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Masses/GeneratedMass.cs)
- [MassGenerator.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Masses/MassGenerator.cs)
- [StylizedRiver.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Rivers/StylizedRiver.cs)
- [StylizedRiverStaticFoam.cs](/F:/Unity/Projects/Norse%20Stylized%203D%20PoC/Assets/Game/Procedural/Rivers/StylizedRiverStaticFoam.cs)

## Note On Workspace State

I inspected the files directly from disk. Earlier, `git status` hit an unrelated Git LFS filter error on a river texture, so this handoff is based on the current workspace files rather than a clean git-state confirmation.

---

# Latest River Foam Addendum — Canonical Architecture Lock

## Supersession note

This handoff remains useful for older generated-rock and river refactor context, but the active Foam architecture is now owned by:

```text
Docs/River_Foam_Stage6_Architecture.md
Docs/River_Foam_Active_Blockers_and_Next_Patches.md
```

Any older statement in this handoff about static foam, Stage 1.5, coherent deformation as the next primary solution, persistent morphing, lateral row commits, or shader-side macro foam shaping is historical only if it contradicts those docs.

## Current foam state

The river Foam system is in Stage 6 / `4.11C` manually-born persistent material recovery. The current code has:

```text
Persistent Foam State / FoamState
_FoamShapeMask
Foam Evaluated Shape debug
Foam Shape Difference debug
Motion Field debug
Motion Field + Cell Grid debug
```

5.9z added a coherent coordinate-warp prototype for `_FoamShapeMask`. Validation with `Foam Shape Difference` showed the warp was numerically active, but normal `Material Presence` and `Foam Evaluated Shape` still looked essentially identical. In 4.11C.5.10B, the warp was retired and `EvaluateFoamShape` was reset to pass-through clipped Persistent Presence so future Layer D probes start from a clean baseline.

## Canonical layer graph

The active architecture is no longer described as a loose `Stage 1 / Stage 1.5 / Stage 2 / Stage 3` sequence. Use the acyclic layer graph:

```text
Layer A — River Domain
Layer B — External Influence Fields
Layer C — Persistent Foam Material
Layer D — Visual Foam / Film Evaluation
Layer E — Shader Composition
Layer F — Scheduling, Quality, Debug
```

Hard dependency rule:

```text
A → B → C → D → E
A may also feed C/D/E directly.
B may feed C/D/E.
C may feed D/E.
D may feed E.
No downstream layer may feed an upstream layer.
```

## Layer ownership summary

- `Layer A — River Domain` owns river-space coordinates, valid fluid, boundary/shore mapping, and material UV conventions.
- `Layer B — External Influence Fields` owns foam-agnostic support/contact/motion/exclusion/wake/pressure influence fields. It may feed Layer C and Layer D, but it must not read `FoamState`, `_FoamShapeMask`, or Layer D helper fields.
- `Layer C — Persistent Foam Material` owns durable `Presence`, `Remaining Life`, `Material Pattern`, birth, death, and real material movement.
- `Layer D — Visual Foam / Film Evaluation` owns `_FoamShapeMask` and future foam-derived visual helper fields such as film source/support. It may visually widen, bridge, pinch, bend, and fragment foam, but it must not write persistent material.
- `Layer E — Shader Composition` owns final color, opacity, soft edges, local procedural chipping/fray, thin streaks, reflection/refraction integration, and debug pixels. It must not own broad structural foam connectivity or feed back into compute.
- `Layer F — Scheduling, Quality, Debug` owns update cadence, allocation, binding, quality tiers, debug view selection, and Inspector labels. It must not own foam behavior math.

## Active implementation direction

Do not tune 5.9z coordinate warp as the primary solution. The compliance/debug audit and `Foam Shape Difference` debug view were completed in `4.11C.5.10`; the failed coordinate warp was retired in `4.11C.5.10B`. `4.11C.5.11` then tested local procedural breakup inside Layer D, but validation showed cell/ribbon-shaped artifacts because `_FoamShapeMask` is too coarse for atomic detail. `4.11C.5.11B` retires that probe and restores the clean pass-through Layer D baseline. The next work should be:

```text
1. Test Layer E shader-side local detail for sub-cell chipping/fray/thin streaks.
2. Add low-res Layer D Film Source / Film Support for broad sheet/contact/bridge behavior.
3. Integrate accepted macro support into full-res _FoamShapeMask.
4. Switch Final Foam to _FoamShapeMask only after the evaluated shape is visibly better than current final foam.
```

Do not introduce pocket IDs, connected-component tracking, or per-pocket state without explicit approval. Do not use naive full-res radius 1/3/5 classifiers as the default, because they cost `179` samples per cell, or about `2.93M` samples for one 128×128 field evaluation. Do not let shader-side wide-neighbour sampling become the source of broad foam structure.

## Code locations to inspect for future Foam work

Core runtime and owner/scheduler code:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs
```

Core compute files:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Support.hlsl
```

Render files:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Important symbols:

```text
StylizedRiverFoamDebugView
FoamEvaluatedShape
EvaluateFoamShape
DispatchEvaluateShape()
shapeMaskTexture
_FoamShapeMask / _FoamShapeMaskWrite
_FoamStateRead / _FoamStateWrite
_FoamMotionLaneRead
_FoamObstacleRoutingRead
FoamDecodeMaterialState(...)
RiverWaterFoamResult.materialUV
```

## Addendum — River Foam 4.11C.5.11 / 5.11B Local Breakup Probe Retired

After the failed 5.9z coordinate warp was retired in `4.11C.5.10B`, `4.11C.5.11` tested a cheap local-only Layer D breakup probe. The probe obeyed the dependency rules: it wrote only `_FoamShapeMask`, read only current-cell material data plus river physical position/time/seed, did not sample neighbouring FoamState cells, did not mutate persistent material, and kept Final Foam disconnected.

Validation showed the probe was active, but unsuitable: `Foam Shape Difference` showed mostly magenta/removal, and the removals appeared as long simulation-cell/ribbon-shaped holes. The result exposed `_FoamShapeMask` cell scale instead of producing granular, almost atomic breakup.

`4.11C.5.11B` retires the probe as active code. `EvaluateFoamShape` is reset to pass-through clipped Persistent Presence, the local helper functions are removed from `CS_RiverFoam.compute`, and `DispatchEvaluateShape()` no longer binds the physical-position/time/seed data required only by that rejected probe.

Current rule: Layer D owns macro film structure, broad sheet/contact/bridge/pinch behavior, and a clean `_FoamShapeMask` foundation. Fine fragmentation, tiny cuts, edge granularity, and thin streaks belong in Layer E shader composition.

## Addendum — River Foam 4.11C.5.12 Layer E Shader-Side Local Detail Probe

After `4.11C.5.11B` restored a clean Layer D pass-through baseline, `4.11C.5.12` adds a debug-only Layer E shader-side local-detail probe. The purpose is to test fine chipping/fray/cuts at rendered-pixel scale, because the previous Layer D local-breakup probe proved `_FoamShapeMask` cells are too coarse for atomic detail.

Changed responsibility remains strict:

```text
Layer C Persistent Foam Material remains material truth.
Layer D _FoamShapeMask remains a clean macro-shape product and is not mutated by this probe.
Layer E Shader Composition applies only debug-local pixel detail and writes only screen pixels.
Final Foam remains unchanged.
```

New debug views:

```text
Foam Shader Detail Probe
Foam Shader Detail Difference
```

Important files:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Next decision after validation: accept, tune once, or reject Layer E local detail. Regardless of that result, broad inspiration-river sheet/contact/bridge behavior still requires the future low-res Layer D Film Source / Film Support system.



## Addendum — River Foam 4.11C.5.13 Low-Resolution Layer D Film Source / Film Support

After the Layer E shader-detail proof, `4.11C.5.13` adds the first real structural Layer D helper fields. `_FoamFilmSource` is a half-resolution visual-film source/permission field built from persistent material and external support/contact fields. `_FoamFilmSupport` is a half-resolution directional spread field intended to create broad sheet/contact/bridge support without pocket IDs, connected components, or wide full-resolution neighbourhood classifiers.

`EvaluateFoamShape` now combines Persistent Presence with Film Source/Support into `_FoamShapeMask`. This remains a visual product only. Final Foam is still disconnected, and no durable material state is modified by Layer D.

When continuing work, validate the four relevant debug views first: `Material Presence`, `Foam Film Source`, `Foam Film Support`, and `Foam Evaluated Shape`. Use `Foam Shape Difference` to confirm where Layer D adds or removes coverage compared with raw Material Presence.

## Addendum — River Foam 4.11C.5.13B Layer D Domain-Space Sampling Fix

If continuing river foam work from this point, keep this coordinate rule in mind:

```text
FoamState = material-space persistent truth.
Film Source / Film Support / _FoamShapeMask = domain-space visual products.
Layer D reads FoamState through domainUV - phaseTransport / fieldLength, but writes/samples its own products in domainUV / fieldUV.
```

This was introduced because the first 5.13 validation showed Film Source, Film Support, Evaluated Shape, and Shape Difference stuttering with the same rhythm as the material cell grid. The material cell grid may still move/snap in its own debug view; that is expected. Layer D visual products should not inherit that movement.


### River Foam 4.11C.5.13C note

`4.11C.5.13C` corrects Layer D Film Source semantics. Generic Layer B support/contact/topology must not become visual film by itself. Film Source is now material-derived, with support used only as bias/suppression. The first path toward shore/rock/contact-looking foam should be Layer C source population: birth real persistent material where environmental source candidates justify it, then let support/lifetime capture decide how long it survives. Do not add a separate visual-only environmental film product before validating that source-population route.


## Addendum — River Foam 4.11C.5.13C validation and next-chat continuation target

Use this section as the current continuation point if opening a new chat after `4.11C.5.13C`.

### Validated current state

```text
4.11C.5.13B fixed Layer D coordinate-space stutter.
4.11C.5.13C fixed support-topology contamination.
Foam Film Source now follows material-derived foam, not generic support topology.
Foam Film Support now spreads material-derived source.
Foam Shape Difference now reports material-derived visual additions/removals.
Final Foam remains unchanged and still does not consume _FoamShapeMask.
```

### Current debug-view meanings

```text
Material Presence:
  Layer C material truth.

Foam Film Source:
  Half-resolution material-derived visual source.
  It should not show support topology without material.

Foam Film Support:
  Half-resolution broadened support/spread field fed by Film Source.
  It can be broader than source but cannot come from support alone.

Foam Evaluated Shape:
  Full-resolution domain-space _FoamShapeMask.
  Visual interpretation only; not persistent material.

Foam Shape Difference:
  Signed difference between _FoamShapeMask and raw material presence.
  Green = material-derived visual addition after 5.13C.
  Magenta = visual removal.

Foam And Aging Topology:
  The explicit support/topology view. If support topology is visible here, that is intentional.
```

### Current 5.13D patch status

`4.11C.5.13D — Layer D Film Spread Shape Tune` has now been implemented as a narrow compute-only tuning pass and is pending Unity validation. The patch keeps the material-gated Film Source contract from 5.13C, does not switch Final Foam to `_FoamShapeMask`, and does not add environmental contact film or Inspector controls.

The primary code file changed is:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Changed functions:

```text
FoamResolveVisualFilmInfluenceAtDomainUV(...)
BuildFoamFilmSupport
EvaluateFoamShape
```

Implemented tuning:

```text
Support bias range reduced to 0.94-1.08.
Cross-flow Film Support spread reduced and gated by source/evidence.
Along-flow continuity remains dominant.
Diagonal spread reduced.
Bridge thresholds tightened and bridge contribution lowered.
Final support contribution to _FoamShapeMask made more conservative.
Negative suppression remains active.
```

Do not touch:

```text
Final Foam integration.
Layer E shader-detail tuning.
Environmental contact film.
Entity/pocket/connected-component systems.
Persistent FoamState writes.
Layer B support-source seeding.
Inspector controls for this still-unstable formula.
```

Validation for 5.13D should compare:

```text
Foam Film Source
Foam Film Support
Foam Evaluated Shape
Foam Shape Difference
Final Foam
```

Expected validation:

```text
Film Source remains material-derived.
Film Support is still broader than source but less uniformly inflated.
Shape Difference green additions are smaller/more selective.
No topology-support contamination returns.
No phase/cell-grid stutter returns.
Final Foam remains unchanged.
```

### Next continuation target after 5.13D validation

5.13D validation showed that material-derived spread from one central manual ribbon remains visually limited. The corrected next step is not a new environmental/contact-film layer. The correct next step is automatic Layer C source population: birth real persistent material near supported environmental source candidates, then use existing support/lifetime capture plus Layer D spread.

## Addendum — River Foam 4.11C.5.14A Automatic Shore/Contact Source Population

Use this as the current continuation point after `4.11C.5.14A`.

### Audit result

```text
Manual/progressive birth exists.
Support/lifetime capture exists.
Layer B shore/pressure/lee topology sources exist.
Layer D material-derived Film Source / Film Support exists.
Automatic birth near specific environmental places was missing.
```

Therefore the intended architecture remains:

```text
Layer B support/contact/topology
  -> Layer C real material birth and Remaining Life capture
  -> Layer D material-derived visual spread
  -> Layer E local shader polish
```

### Implemented first source class

`4.11C.5.14A` adds a disabled-by-default Automatic Source Population foldout and the first conservative source class: sparse shore/contact material birth. This creates real persistent FoamState material through the existing `PendingInjection` / `QueueMaterialBirth` / `InjectFoam` path.

Changed code files:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
```

### Hard constraints preserved

```text
Automatic birth is off by default.
Support/topology still cannot render as foam from zero material.
No Final Foam integration changed.
No new Environmental Contact Film product was added.
No entity/pocket/connected-component system was added.
Layer D still does not write FoamState.
```

### Validation target

```text
With Automatic Birth disabled, behavior should match 5.13D.
When enabled, Material Presence should receive sparse shore/contact births.
Material Remaining Life should show support-preserved material near shore/contact zones.
Foam Film Source and Foam Film Support should derive from those real births.
Final Foam should remain unchanged.
```

### Next continuation target after 5.14A

First validate whether sparse shore/contact source population creates useful real material in the intended places. If it does, the next source classes should be obstacle/pressure contact birth, lee/wake birth, and connector/major-support birth. If it overfills the river, reduce birth amount, acceptance, radius, or per-tick budget before adding any new source classes.


## Addendum — River Foam 4.11C.5.14B / 5.14C Source Population Controls

Use this as the current continuation point after `4.11C.5.14C`.

### Validation result that caused 5.14B

`4.11C.5.14A` successfully proved automatic Layer C birth plumbing, but the first shore source controls were too crude. With Automatic Birth enabled and the old `Shore Contact Birth Amount` around `0.35`, Material Remaining Life showed some shore survival, but the births appeared as large blocky chunks, sometimes river-wide or wider.

The conclusion was:

```text
Architecture validated: automatic material birth + support capture works.
Source-scale tuning failed: one amount slider produced oversized compound chunks.
```

The correct fix was not a new visual film layer and not another hard-coded constant tweak. The correct fix was source-class-specific spawning.

### 5.14B result

`4.11C.5.14B` introduced source-population presets and a Shore Contact Birth profile. The source-class idea was correct, but the Inspector exposed too many low-level controls:

```text
Density
Budget Per Tick
Minimum Support
Inward Offset Metres
Band Width Metres
Seed Radius Metres
Seed Elongation
Stroke Length Metres
Initial Amount
Initial Life
Jitter
Shape Mode
```

This was rejected as authoring/control bloat. It forced the user to tune implementation details rather than the intended shore-foam behavior.

### Implemented 5.14C controls

`4.11C.5.14C` keeps source-class-specific spawning but simplifies Shore Contact Birth to one deterministic hidden recipe with four English-facing controls:

```text
Coverage      how much shoreline receives foam over time
Size          how large each shore seed/stroke is
Strength      how visible new shore foam is at birth
Persistence   how much initial life new shore foam receives
```

The Source Population foldout now exposes:

```text
Automatic Foam Birth
Spawn Preset
Shore Foam
  Coverage
  Size
  Strength
  Persistence
Runtime Status / Queued counts
```

Hidden shore recipe rules:

```text
Shore foam always uses small deterministic strokes.
Compound shore blobs are not used.
Coverage maps to candidate spacing/acceptance and internal budget.
Size maps to conservative radius/stroke length.
Strength maps to initial persistent material presence.
Persistence maps to initial Remaining Life.
Candidate variation is deterministic from river seed, candidate identity, and repeat cycle, not wall-clock randomness.
Support capture still controls long-term survival.
```

### Changed code files in 5.14C

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
```

### Hard constraints preserved

```text
Automatic birth remains globally gated.
Only Shore Contact Birth is implemented.
No Final Foam integration changed.
No support-only Film Source was reintroduced.
No Environmental Contact Film visual product was added.
No entity/pocket/connected-component system was added.
Layer D still does not write FoamState.
```

### Validation target

Use:

```text
Material Presence
Material Remaining Life
Foam Film Source
Foam Film Support
Final Foam
```

Expected result:

```text
With Automatic Foam Birth disabled, behavior matches the previous baseline.
With Spawn Preset = Shore Contact Test, shore births appear as small deterministic edge strokes/flecks.
At Coverage around 0.35, births should not become river-wide chunks.
Changing Coverage should change how much shoreline receives births over time, not footprint or life.
Changing Size should change footprint while keeping births near-shore.
Changing Strength should affect new material visibility.
Changing Persistence should affect initial survival before support capture.
Foam Film Source and Film Support should derive from real material births.
Final Foam remains unchanged.
```

### Next continuation target after 5.14C

First validate the simplified Shore Contact Test preset. If it still overfills, tune the hidden shore recipe constants and deterministic acceptance. If shore birth validates as controllable small strokes/flecks, the next source class should be either Obstacle Contact Birth or Lee/Wake Birth, implemented one class at a time through the same Layer C material-birth path and exposed through similarly small intent-level controls.

## Addendum — River Foam 4.11C.5.14D Deterministic Shore Source Events

Superseded: `4.11C.5.14D` visually failed and was replaced by `4.11C.5.14E`.

### Why 5.14D exists

`4.11C.5.14C` simplified the Source Population controls, but runtime validation showed that the hidden shore recipe was too starved and too same-shaped. Even with controls maxed, barely any foam spawned. The few spawned shapes read as isolated one-shot strokes, not as a coherent shore/contact source system.

The rejected direction is many faint deposits that accumulate into visible foam. The target reference shows crisp high-value foam: thin streamlines, shore/contact ribbons, broad but sharp sheets, rock/bank-attached arcs, and branching connectors. The source system therefore needs deterministic placement of normal-strength source events, not many weak material deposits.

### Implemented contract

`4.11C.5.14D` remains architecturally compliant:

```text
Layer B/domain shore context
  -> deterministic shore source slots
  -> Layer C progressive source events
  -> real persistent FoamState material
  -> support/lifetime capture
  -> Layer D material-derived spread
  -> Final Foam unchanged
```

No new visual-only environmental/contact-film texture was added. Support topology still does not become foam. Remaining Life rules are unchanged.

### Current controls

The Source Population UI should show only:

```text
Automatic Foam Birth
Spawn Preset
Shore Foam
  Coverage
  Activity
  Patch Size
  Pattern
```

`Pattern` options are:

```text
Mixed
Shore Ribbons
Inward Wash
```

### Current implementation

The shore source class now uses deterministic source slots across both banks. The scheduler starts bounded source events from eligible slots. Events are full-strength material births revealed progressively through the existing composition-event path.

Implemented recipes:

```text
Shore Ribbon:
  bank-parallel opaque source event near the shore contact band.

Inward Wash:
  shore-attached source event that drifts inward/downstream from the bank.
```

### Validation instructions

Validate in this order:

```text
1. Material Presence
2. Material Remaining Life
3. Foam Film Source
4. Foam Film Support
5. Final Foam
```

Expected behavior:

```text
Coverage ~0.45 / Activity ~0.45 / Patch Size ~0.35 / Pattern Mixed should start visible, opaque shore events over time.
Shore Ribbons should produce bank-parallel events.
Inward Wash should produce inward/downstream tongues.
Events should distribute across the river chunk over time.
Events should not be faint deposits, random one-shot specks, giant river-wide blobs, or support-only topology.
Final Foam should remain unchanged.
```

### Next likely work

If 5.14D validates, tune only the four high-level shore controls or hidden recipe constants. Do not add river-body, obstacle-contact, or lee/wake source classes until shore source events are acceptable. If 5.14D still looks wrong, diagnose whether the issue is slot eligibility, recipe geometry, progressive reveal envelope, or support/lifetime capture before adding new systems.


## Addendum — River Foam 4.11C.5.14E Automatic Source Event Rasterizer

Use this as the current continuation point after `4.11C.5.14E`.

### Why 5.14E exists

`4.11C.5.14D` kept the right architecture but failed the visual target. Even at max Coverage/Activity, it produced predictable near-shore rectangles/bars; `Shore Ribbons` and `Inward Wash` were not visually distinct. The root cause was that both recipes still emitted generic progressive composition segments and therefore reached the GPU as `PendingInjection` / `InjectFoam` segment capsules.

### Implemented contract

`4.11C.5.14E` keeps automatic shore birth as Layer C material, but changes the automatic output path:

```text
Layer B/domain shore context
  -> deterministic shore source slots
  -> typed automatic FoamSourceEvent records
  -> RasterizeFoamSourceEvent compute kernel
  -> real persistent FoamState material through FoamMergeBornPresence
  -> support/lifetime capture
  -> Layer D material-derived spread
  -> Final Foam unchanged
```

Manual/debug injections still use the old `PendingInjection` / `InjectFoam` path. Automatic shore foam no longer relies on generic segment capsules.

### Current source types

```text
ShoreRibbon
  Analytic bank-following ribbon using live current-shore edges, tapered ends, and deterministic edge breakup.

InwardWash
  Analytic shore-attached tongue revealed inward from the live shore edge with downstream curvature and taper.
```

The current UI remains only Coverage, Activity, Patch Size, and Pattern. Do not add low-level source controls unless validation proves a genuinely necessary authoring parameter.

### Validation instructions

Validate in this order:

```text
1. Material Presence
2. Material Remaining Life
3. Foam Film Source
4. Foam Film Support
5. Final Foam
```

Expected behavior:

```text
Coverage = 1 / Activity = 1 / Patch Size = 1 / Pattern Shore Ribbons should show obvious thin bank-following ribbons, not rectangular bars.
Coverage = 1 / Activity = 1 / Patch Size = 1 / Pattern Inward Wash should show shore-attached inward/downstream tongues, visibly distinct from ribbons.
Pattern Mixed should deterministically show both source classes across both banks.
Material should be normal-strength; do not expect faint deposits to accumulate into visibility.
Foam Film Source and Foam Film Support should follow the new material.
Final Foam should remain unchanged.
```

### Next likely work

If 5.14E validates, the next source vocabulary needed by the inspiration river is open-water streamlines/sheet borders, followed by rock/contact arcs. Keep using the typed source-event rasterizer for those classes. Do not switch Final Foam to `_FoamShapeMask` until source material and Layer D macro spread are accepted.

## River Foam handoff note — after 4.11C.5.14F

Current automatic shore-source direction:

- The dedicated Layer C automatic source-event rasterizer is still the right foundation.
- 5.14E fixed the primitive problem by replacing generic capsule-like automatic shore births.
- 5.14F fixes the next observed problem: source events formed too quickly and Inward Wash accumulated into broad blobs.

Important current implementation details:

- Shore Foam now has `Formation Speed` in metres per second. This controls how fast a single event forms along its path and is independent from `Activity`.
- Source duration is derived from source path distance / formation speed.
- Inward Wash is a moving curved stroke-head. It writes only a short head/trail segment per update; persistent FoamState preserves the already drawn path.
- Final Foam remains disconnected from the Layer D products until source material and macro spread are accepted.

Next validation should compare `Shore Ribbons`, `Inward Wash`, and `Mixed` in `Material Remaining Life`, then inspect `Foam Film Source` and `Foam Film Support`.

## River Foam handoff note — after 4.11C.5.14G

Current shore-spawning status:

- The automatic source-event rasterizer remains the correct Layer C foundation.
- Formation Speed is good enough to park for now.
- Shore Ribbons are improved enough to keep as the primary shore source.
- 5.14G specifically refines Inward Wash, which was still producing chunky slabs/cards after 5.14F.

5.14G changes:

- Inward Wash uses smaller dimensions and lower persistence.
- Inward Wash uses shorter wash-specific head trails.
- The wash curve follows shore first, then peels inward.
- Wash source-fill and feather/stroke inflation are reduced.
- `Mixed` is mostly Shore Ribbon and only occasionally Inward Wash.

Next validation should stay strictly on shore spawning in `Material Remaining Life`: compare `Shore Ribbons`, `Inward Wash`, and `Mixed`. Do not move to object foam, free-water foam, Layer D tuning, or Final Foam until shore spawning is acceptable.


## River Foam handoff note — after 4.11C.5.14H

Current shore-spawning status:

- The Layer C automatic source-event rasterizer remains the foundation.
- Formation Speed remains accepted enough to park.
- Shore Ribbons are the primary accepted shore source.
- Inward Wash remains crude, but now has direct authoring controls rather than another hardcoded tuning pass.

5.14H changes:

- Inspector source population is now organized as `Foam Birth Sources`.
- `Shore Foam` is implemented; `Object Foam` and `Free Water Foam` are present as disabled framework sections for future source classes.
- Shore Foam `Mixed` composition uses normalized Shore Ribbon / Inward Wash weights. Changing one recalculates the other so total source rate is not affected.
- Each shore pattern exposes Formation Speed, dimension ranges, Initial Life, and Breakup Strength.
- Runtime event sampling uses correlated scale and aspect guards to avoid incoherent length/width/reach combinations.

Next recommended validation: stay in `Material Remaining Life`. Validate the control framework, especially normalized weights, per-pattern Formation Speed, and Initial Life. If shore spawning is authorable enough, the next source class should be Object Foam spawning. Do not move to free-water foam, Layer D tuning, or Final Foam before shore/object birth are acceptable.

## 4.11C.5.15A handoff note

Object Foam spawning is now implemented as a Layer C birth source category. Validate with Shore Foam disabled or low and Object Foam enabled in Material Remaining Life. Expected result: object-contact arcs/flecks near static generated obstacles, no material inside obstacle masks, no free-water or wake-tail behavior yet.

## 4.11C.5.15A.1 handoff note

If Object Foam is enabled and automatic birth is on, object source population should now run for any non-Off source preset. The previous 5.15A build left Object Foam behind a hidden preset gate, causing `Object source population disabled` and zero events. The inspector now reports `Source Anchors` for Object Foam. If anchors are zero, inspect disturbance static source registration/export; if anchors are greater than zero but events remain zero, inspect scheduling/rejection.

## 4.11C.5.15A.2 handoff note

Object Foam now has a GPU Object Contact Edge Field. The 5.15A/5.15A.1 object event scheduler remains unchanged and CPU-bounded; only the object-contact source shape authority changed. The contact field is built from the exact obstacle exclusion field and static pressure/contact evidence. Contact Arc/Fleck masks sample the field and shape in contact normal/tangent space, so they should be less box-like than the earlier object half-extent bands.

Validate in Material Remaining Life with Shore Foam off/low and Object Foam on. If contact foam is still poor, inspect the contact field construction/gating before changing scheduling.

## 4.11C.5.15A.4 handoff note

Object Foam now includes `Contact Semi-Arcs` in addition to full Contact Arcs and Contact Flecks. This was added because the stable full-arc evaluator is symmetric in contact tangent space, while the visual target needs object-contact foam that sometimes appears on one shoulder/side only. Semi-Arcs reuse the existing object-contact field and source-event rasterizer; deterministic signed lopsidedness is carried through `Curvature` / GPU `variation.w`, so no new texture/buffer/resource binding was introduced.

Validation should stay in `Material Remaining Life`: test Debug Pattern Mode `Contact Arcs`, `Contact Semi-Arcs`, `Contact Flecks`, then `Mixed`. If Semi-Arcs still look too symmetric, inspect the signed tangent-window evaluator before changing object-contact resource semantics. Do not restart the failed edge-distance contact-field correction in the same patch as pattern tuning.

## River Foam State After 4.11C.5.15B

Free Water Foam birth is now implemented. The active free-water source patterns are Lace Connectors and Torn Fragments. Lace Connectors use a moving head+stroke path; Torn Fragments use a timed sweep reveal over an asymmetric local patch. Both write persistent FoamState material and remain subject to valid-fluid/obstacle clipping. Rind strokes and shader glints are not implemented in this patch. Validate with Material Remaining Life and isolate Free Water Foam by disabling Shore/Object birth first.

### River Foam State After 4.11C.5.15B.2

Free Water Foam now has three source patterns: Lace Connectors, Cross-Lace Connectors, and Torn Fragments. Cross-Lace Connectors are the newest addition and are intended to address the missing horizontal/cross-current ribbons. They use a moving head+stroke across the river, pack lateral half-length/width/sign into existing source event object data, and use the existing rasterizer/valid-fluid clipping path. No density, Coverage, Activity, or glint-rendering changes were made in this patch.



## River Foam Handoff — after `4.11C.5.16A.1` implementation

### Current strategic state

Spawning is parked as provisionally sufficient. The river can create persistent material at shores, static objects, and open water using multiple source grammars. Cross-Lace remains visibly blocky because the persistent grid has anisotropic physical cells; the expensive high-longitudinal-resolution redesign is deferred and Cross-Lace should remain a minority pattern.

The active sequence now begins at motion authority and proceeds in layer order.

### Implemented velocity foundation

Raw Layer B inputs remain:

```text
Motion Lane RHalf — signed lateral intent, scrolling sample coordinate.
Obstacle Routing RGHalf — signed route direction + influence, fixed to obstacles.
```

They are resolved through one shared physical contract in:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl
```

Output:

```text
velocityMetresPerSecond.x = nonnegative downstream speed magnitude
velocityMetresPerSecond.y = signed lateral speed
lateralIntent
downstreamSpeedFactor
obstacleInfluence
laneIntent
obstacleIntent
```

Current Inspector meanings:

```text
Downstream Speed Ratio
Maximum Lateral Speed Ratio
Lane Advection Ratio
Direction Change Frequency
Across-River Coherence
Low Lateral Motion Coverage
Obstacle Slowdown Strength
Obstacle Minimum Downstream Factor
```

Important: old serialized field names remain internally to avoid scene/prefab churn. A one-time tuning migration converts the accepted legacy defaults to the new physical units.

### Current movement truth

```text
The resolved velocity contract exists and is debug-visible.
The old global downstream phase commit still moves FoamState.
No local slowdown or lateral velocity changes FoamState yet.
```

`5.16A` validation confirmed the shared velocity contract, signed lateral response, flow reversal, physical lane advection, and no premature persistent-material movement. It exposed three issues that must be corrected before transport:

```text
runtime warning HelpBoxes repeatedly inserted/removed rows and shifted the Inspector;
obstacle yellow was blended after speed darkening and could obscure stagnation proof;
lateral route sign regions were too broad downstream, while the same scale also changed across-river variation.
```

`5.16A.1` implementation:

```text
Inspector runtime transport diagnostics now use fixed-height labels only;
Motion Field composes route/obstacle hue first and applies downstream-speed brightness once;
Direction Change Frequency controls downstream sign changes independently;
Across-River Coherence controls lateral row grouping independently;
all main, breaker, cross-cut, and warp frequencies use the split controls;
two-pass across-width smoothing remains unchanged;
Motion Lane signature is version 3 and hashes both controls;
Obstacle Routing signature remains independent.
```

The existing Motion Field debug views should now read:

```text
bright neutral gray = straight full-speed downstream;
bright red/blue = full-speed signed lateral velocity;
dark red/blue = signed lateral route with downstream slowdown;
dark yellow = obstacle-influenced and slow;
near black = near-stagnation;
white overlay = raw persistent Presence.
```

### Active validation

```text
leave Foam Velocity open in Play Mode for 20–30 seconds and confirm no vertical movement;
set obstacle slowdown to 1 / minimum downstream factor to 0 and confirm strong regions darken;
compare Direction Change Frequency 1 against 2.0–2.5;
compare Across-River Coherence 1 against 1.5–2.0;
reject checkerboard or regular stripe results;
confirm Material Presence is still unchanged.
```

### Exact next major patch after validation

`4.11C.5.16B — Conservative Unified 2D Material Advection`.

The patch must replace the final global-only transport authority with a conservative finite-volume/TVD path for packed material:

```text
R = Presence
G = Presence × Remaining Life
B = Presence × Material Pattern
```

Required invariants:

```text
no upstream downstream velocity;
no per-cell stochastic lateral row choice;
no neighbour-resampling morphology;
material-weighted life/pattern transport;
valid-fluid and obstacle face-flux rejection;
bounded CFL/substeps;
measured material conservation error;
coherent broad left/right movement.
```

Do not begin Layer D visual history or fracture until this transport path is accepted. Layer D must later consume the same velocity contract.
