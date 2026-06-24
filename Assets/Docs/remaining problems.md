You are right: **8 cm was far too conservative for this stylized “Strong” profile.** I’m interpreting your “80” as **80 cm / 0.8 m**. That is a far more useful stress target.

The static-pressure debug image also gives us a decisive answer: the data reaches the texture, but the **baked pressure shape itself is wrong**. We should fix only that.

## What the debug view proves

Those isolated white patches are not a continuous pressure band. The current shader:

* finds the nearest segment of the 16-point footprint contour;
* uses that segment’s fixed outward normal;
* keeps only segments judged sufficiently upstream-facing;
* draws a very narrow Gaussian line beside those segments.

On an irregular, faceted footprint, neighbouring edges have noticeably different normals. A few edges pass the upstream-facing test and others fail it, producing separate blobs.

So the immediate failure is not the wake field or final lighting. It is:

```text
cached contour → static pressure texture
```

## Concrete next patch: static pressure only

I recommend changing only the static-pressure path.

### 1. Build a continuous upstream front

Instead of classifying individual contour edges by their normals, the compute shader should derive the object’s upstream silhouette.

For every lateral position across the rock:

1. Intersect that lateral row with the cached convex contour.
2. Find the upstream-most intersection.
3. Treat those intersections together as one continuous front curve.
4. Generate a raised shelf immediately upstream of that curve.

This naturally handles different objects:

* rounded rock → curved pressure front;
* broad slab → flatter pressure front;
* narrow object → narrow front;
* irregular generated rock → front follows its actual footprint.

There will be no disconnected edge-normal spots.

### 2. Replace the thin Gaussian line with a pressure shelf

The shape should have meaningful thickness:

```text
rock boundary
████████ strongest pressure
██████
████
██
ordinary water upstream
```

For the displayed rock, I would initially use a crest depth around **0.45–0.55 m**. The exact calculation should be resolution- and object-aware:

```text
crest depth =
clamp(
    max(0.35 m, object along-length × 0.20),
    0.35 m,
    0.75 m
)
```

Your rock’s along length is about `2.31 m`, giving roughly `0.46 m`. That spans several water-mesh intervals and texture samples, rather than disappearing between them.

The crest remains strongest directly against the upstream face and falls off sharply over that distance.

### 3. Stop limiting static pressure to the ripple ceiling

The diagnostic says:

```text
Effective amplitude: 0.0799 m
Maximum allowed:     0.09 m
```

That ceiling comes from the general disturbance/ripple height and geometry-strength controls. It is inappropriate for this feature.

A localized **positive static-pressure rise** is not the same safety problem as downward ripple displacement. Raising it does not expose the riverbed. Static pressure should therefore receive its own positive-height budget.

I recommend calculating the requested height directly in metres:

```text
base profile height
× object Strength Multiplier
× object Geometry Amplitude
× blockage factor
× flow factor
```

Starting profile bases:

```text
Subtle:    0.10 m
Standard:  0.25 m
Strong:    0.45 m
```

With the values shown for your Strong rock:

```text
0.45 m
× 1.35 Strength
× 1.40 Geometry
× approximately 0.9–1.0 size/flow adjustment
≈ 0.75–0.85 m
```

That puts this specific test near your requested **80 cm**.

I would then apply a source-size safety cap such as:

```text
maximum static pressure =
min(
    1.25 m,
    smaller footprint dimension × 0.5
)
```

For this rock, that permits around `0.88 m`. Small stones cannot create metre-high walls, while large obstacles can produce substantial stylized pressure.

### 4. Do not multiply it down again in the water shader

The static texture should store the final requested height in metres.

The shader should combine:

```text
stable ripple height using ripple limits
+
positive static-pressure height using its separate limit
```

The existing general geometry-strength multiplier should not silently reduce the already-authored static height a second time. The generated object’s profile and Geometry Amplitude already provide artistic control.

### 5. Leave everything else alone

For this pass, I would not alter:

* wake generation;
* wake advection;
* ripple simulation;
* impacts;
* dynamic emitters;
* registry behaviour;
* footprint resolution;
* object controls;
* temporal pressure animation.

They are separate questions. The wake field does not feed the static-pressure debug view, so it does not prevent this test.

## Acceptance condition

Before doing anything else, this one test must pass:

**Static Pressure Target debug view**

* one connected bright shelf across the upstream face;
* no disconnected blobs;
* no downstream trail;
* shape follows the object’s width.

**Final rendering**

* unmistakable steep rise from a low camera;
* still readable from the gameplay camera;
* Strong reaches roughly `0.8 m` for the shown rock;
* the rest of the river remains unchanged.

I do not need any additional files or information. The current V5 sources, screenshot, and resolved diagnostics are enough to implement this isolated correction.



- shore wave motion is too static, static shapes travel downstream instead of them fluctuating over time.

- river current is not stronger/faster in the middle compared to sides, this should change.