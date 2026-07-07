# Rock And River Refactor Handoff

> **River Foam supersession notice — 2026-07-07**
>
> This handoff is historical for earlier rock/river refactor context. Any river Foam architecture, static-foam, morphing, lateral motion, or rendering responsibility statement in this document is superseded by `Docs/River_Foam_Stage6_Architecture.md` and `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`. Use those documents as the active Foam source of truth.

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
