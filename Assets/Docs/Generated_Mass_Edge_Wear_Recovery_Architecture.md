# Generated Mass Edge-Wear Recovery Conclusions

Status: historical conclusions retained for maintenance

This document no longer owns current production routing. Current production geometry and surface response are defined by `Generated_Mass_Surface_Response_Architecture.md`. Current selection is defined by `Generated_Mass_Incremental_Selection_Architecture.md`.

## Retained conclusions

The recovery program established that:

- valid corner chips must not be rolled back to preserve ordinary bevels;
- ordinary bevel incompatibility is resolved by deterministic ranking and one-loser-per-retry reduction;
- zero ordinary bevels is a valid certified result;
- complete geometry/topology/render certification remains mandatory after every reduction;
- construction deadlines must begin after baseline generation where cold baseline work would otherwise consume the integration budget;
- combinatorial conflict-frontier search was unnecessary and was removed;
- diagnostic approximations must not duplicate or redefine production selection;
- exact accepted geometry, not diagnostic candidate intention, owns final status and identity evidence;
- failures must return a deterministic certified fallback rather than a partial or status-inconsistent mesh.

## Superseded historical state

Earlier recovery patches deliberately kept ordinary generation base-only while geometry was validated through editor previews. That state is historical. GM-SURFACE.2 now promotes the certified builder to ordinary generation; `BaseGeometryOnly` remains only for disabled features and safe fallback.

Historical terms such as `geometryCommit=disabled`, preview-only production ownership, or an unchanged `EdgeWearEvaluationMode.None` must not be interpreted as current architecture.

## Maintenance constraints

- Do not restore the retired frontier solver without new evidence.
- Do not make surface-response work alter frozen selection.
- Do not add blocking diagnostics.
- Do not add per-rock feature textures.
- Do not use editor preview state as production ownership.
