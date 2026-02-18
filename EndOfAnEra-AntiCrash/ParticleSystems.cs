using UnityEngine;

namespace EndOfAnEra_AntiCrash
{
    // Im Sure Im Even Missing Checks Here But This Is A Concept, Not A Full Proof Fix For All Cases. In Majority Of Cases This Should Be Able To Prevent Crashing
    public class ParticleSystems
    {
        internal static void SanitizeParticleSystem(ParticleSystem ps, ref Limits Limits, bool shouldLog, bool NukeAnyways = false)
        {
            if (ps == null || ps.gameObject == null) return;

            if (++Limits.particleSystemCount > Limits.MaxParticleSystems)
            {
                if (Helpers.Nuke(ps, ref Limits, shouldLog, $"exceeding global particle system limit ({Limits.particleSystemCount} > {Limits.MaxParticleSystems})"))
                    return;
            }

            if (Limits.recursionDepth > Limits.MaxParticleNestingDepth)
            {
                if (Helpers.Nuke(ps, ref Limits, shouldLog, $"excessive nesting depth ({Limits.recursionDepth} > {Limits.MaxParticleNestingDepth})"))
                    return;
            }

            var emission = ps.emission;
            var shape = ps.shape;
            var velocityOverLifetime = ps.velocityOverLifetime;
            var limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
            var forceOverLifetime = ps.forceOverLifetime;
            var sizeOverLifetime = ps.sizeOverLifetime;
            var sizeBySpeed = ps.sizeBySpeed;
            var rotationOverLifetime = ps.rotationOverLifetime;
            var externalForces = ps.externalForces;
            var inheritvelocity = ps.inheritVelocity;
            var noise = ps.noise;
            var collision = ps.collision;
            var trigger = ps.trigger;
            var subEmitters = ps.subEmitters;
            var textureSheetAnimation = ps.textureSheetAnimation;
            var lights = ps.lights;
            var trails = ps.trails;

            int _maxParticles = ps.main.maxParticles;
            if (_maxParticles > Limits.MaxParticleCount)
            {
                if (Helpers.Nuke(ps, ref Limits, shouldLog, $"exceeding max particle count ({_maxParticles} > {Limits.MaxParticleCount})"))
                    return;
            }

            // -------------------------------
            // Limit maxParticles by mesh if the particle system is using mesh rendering, to prevent extreme cases of very high vertex/triangle count meshes with high maxParticles
            // -------------------------------
            var particlemesh = ps.gameObject.GetComponent<ParticleSystemRenderer>();
            if (particlemesh != null && particlemesh.renderMode == ParticleSystemRenderMode.Mesh)
            {
                var mesh = particlemesh.mesh;
                if (mesh != null)
                {
                    int vertexCount = mesh.vertexCount;
                    int triangleCount = (int)Renderers.GetTrianglesCountImpl(mesh);

                    if (vertexCount >= 65000) // good to hard limit here
                    {
                        if (Helpers.Nuke(ps, ref Limits, shouldLog, $"mesh too complex (vertices: {vertexCount} > 65000)"))
                            return;
                    }

                    if (triangleCount > 0 && vertexCount > 0)
                    {
                        long maxParticlesByVertices = (long)Limits.MaxParticleMeshVertices / vertexCount;
                        long maxParticlesByTriangles = (long)Limits.MaxMeshTriangles / triangleCount;
                        long maxParticlesSafe = Math.Min(maxParticlesByVertices, maxParticlesByTriangles);

                        if (maxParticlesSafe <= 0)
                        {
                            if (Helpers.Nuke(ps, ref Limits, shouldLog, $"mesh too complex to safely limit particles"))
                                return;
                        }

                        if (ps.main.maxParticles > maxParticlesSafe)
                            ps.main.maxParticles = (int)maxParticlesSafe;
                    }
                }
            }

            int maxParticles = ps.main.maxParticles;

            // -------------------------------
            // Nuke if still over limit
            // -------------------------------
            if (maxParticles > Limits.MaxParticleCount)
            {
                if (Helpers.Nuke(ps, ref Limits, shouldLog, $"exceeding max particle count ({maxParticles} > {Limits.MaxParticleCount})"))
                    return;
            }

            long projectedParticleCount = (long)Limits.currentParticleCount + maxParticles;
            if (projectedParticleCount > Limits.MaxParticleCount)
            {
                if (Helpers.Nuke(ps, ref Limits, shouldLog, $"exceeding total particle count ({projectedParticleCount} > {Limits.MaxParticleCount})"))
                    return;
            }

            Limits.currentParticleCount += maxParticles;

            if (subEmitters.enabled)
            {
                if (subEmitters.subEmittersCount > Limits.MaxChildParticleSystems)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"too many sub-emitters", maxParticles))
                        return;
                }

                Limits.recursionDepth++;

                long estimatedChildParticles = Helpers.EstimateSubEmitterParticles(subEmitters);

                if ((long)Limits.currentParticleCount + estimatedChildParticles > Limits.MaxParticleCount)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"sub-emitters exceeding total particle count ({Limits.currentParticleCount + estimatedChildParticles} > {Limits.MaxParticleCount})", maxParticles))
                    {
                        Limits.recursionDepth--;
                        return;
                    }
                }

                for (int i = 0; i < subEmitters.subEmittersCount; i++)
                {
                    var sub = subEmitters.GetSubEmitterSystem(i);
                    if (sub != null)
                    {
                        SanitizeParticleSystem(sub, ref Limits, shouldLog, NukeAnyways);
                    }
                }

                Limits.recursionDepth--;
            }

            if (ps.playbackSpeed >= 125)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"playbackSpeed >= 125 (Playback Speed: {ps?.playbackSpeed})", maxParticles))
                        return;
                }
                else
                {
                    ps.playbackSpeed = Helpers.Limit(ps.playbackSpeed, 20);
                }
            }

            if (textureSheetAnimation.cycleCount >= Limits.MaxParticleAnimationCycle || textureSheetAnimation.fps > 90 || textureSheetAnimation.startFrameMultiplier > 5f)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "texture sheet animation limits", maxParticles))
                        return;
                }
                else
                {
                    textureSheetAnimation.cycleCount = (int)Helpers.Limit(textureSheetAnimation.cycleCount, Limits.MaxParticleAnimationCycle);
                    textureSheetAnimation.fps = Helpers.Limit(textureSheetAnimation.fps, 60);
                    textureSheetAnimation.startFrameMultiplier = (int)Helpers.Limit(textureSheetAnimation.startFrameMultiplier, 1f);
                }
            }

            if (externalForces.multiplier > 100)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "external forces", maxParticles))
                        return;
                }
                else
                {
                    externalForces.multiplier = Helpers.Limit(externalForces.multiplier, 10f);
                }
            }

            var limit = limitVelocityOverLifetime.limit;
            float limitcurve = Helpers.GetMaxCurveValue(limit);
            if (limitVelocityOverLifetime.dragMultiplier > 5f || limitcurve >= Limits.MaxParticleSimulationSpeed * 5 || limitVelocityOverLifetime.limitMultiplier > Limits.MaxParticleSimulationSpeed * 3)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "LimitVelocityOverLifetime limits", maxParticles))
                        return;
                }
                else
                {
                    limitVelocityOverLifetime.dragMultiplier = Helpers.Limit(limitVelocityOverLifetime.dragMultiplier, 3f);
                    limitVelocityOverLifetime.limitMultiplier = Helpers.Limit(limitVelocityOverLifetime.limitMultiplier, Limits.MaxParticleSimulationSpeed * 3);
                    Helpers.Limit(ref limit, Limits.MaxParticleSimulationSpeed * 5, ps.name + ".limitVelocityOverLifetime.limit", shouldLog);
                    limitVelocityOverLifetime.limit = limit;
                }
            }

            var velocityCurveX = velocityOverLifetime.x;
            var velocityCurveY = velocityOverLifetime.y;
            var velocityCurveZ = velocityOverLifetime.z;
            var speedModifierCurve = velocityOverLifetime.speedModifier;
            float velocityXValue = Helpers.GetMaxCurveValue(velocityCurveX);
            float velocityYValue = Helpers.GetMaxCurveValue(velocityCurveY);
            float velocityZValue = Helpers.GetMaxCurveValue(velocityCurveZ);
            float speedModifierValue = Helpers.GetMaxCurveValue(speedModifierCurve);
            if (velocityXValue > Limits.MaxParticleSimulationSpeed || velocityYValue > Limits.MaxParticleSimulationSpeed || velocityZValue > Limits.MaxParticleSimulationSpeed || speedModifierValue > Limits.MaxParticleSimulationSpeed || velocityOverLifetime.speedModifierMultiplier > Limits.MaxParticleSimulationSpeed)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "velocity over lifetime curves", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref velocityCurveX, Limits.MaxParticleSimulationSpeed, ps.name + ".velocityOverLifetime.x", shouldLog);
                    Helpers.Limit(ref velocityCurveY, Limits.MaxParticleSimulationSpeed, ps.name + ".velocityOverLifetime.y", shouldLog);
                    Helpers.Limit(ref velocityCurveZ, Limits.MaxParticleSimulationSpeed, ps.name + ".velocityOverLifetime.z", shouldLog);
                    velocityOverLifetime.x = velocityCurveX;
                    velocityOverLifetime.y = velocityCurveY;
                    velocityOverLifetime.z = velocityCurveZ;
                    Helpers.Limit(ref speedModifierCurve, Limits.MaxParticleSimulationSpeed, ps.name + ".velocityOverLifetime.speedModifier", shouldLog);
                    velocityOverLifetime.speedModifier = speedModifierCurve;
                    velocityOverLifetime.speedModifierMultiplier = Helpers.Limit(velocityOverLifetime.speedModifierMultiplier, Limits.MaxParticleSimulationSpeed, ps.name + ".velocityOverLifetime.speedModifierMultiplier", shouldLog);
                }
            }

            if (sizeBySpeed.sizeMultiplier >= Limits.MaxParticleSize / 2)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "SizeBySpeed sizeMultiplier", maxParticles))
                        return;
                }
                else
                {
                    sizeBySpeed.sizeMultiplier = Helpers.Limit(sizeBySpeed.sizeMultiplier, Limits.MaxParticleSize / 3);
                }
            }

            if (sizeOverLifetime.sizeMultiplier >= Limits.MaxParticleSize / 2 || (rotationOverLifetime.zMultiplier > 5 || rotationOverLifetime.xMultiplier > 5 || rotationOverLifetime.yMultiplier > 5))
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "size/rotation multipliers", maxParticles))
                        return;
                }
                else
                {
                    sizeOverLifetime.sizeMultiplier = Helpers.Limit(sizeOverLifetime.sizeMultiplier, Limits.MaxParticleSize / 3);
                    rotationOverLifetime.zMultiplier = Helpers.Limit(rotationOverLifetime.zMultiplier, 2f);
                    rotationOverLifetime.xMultiplier = Helpers.Limit(rotationOverLifetime.xMultiplier, 2f);
                    rotationOverLifetime.yMultiplier = Helpers.Limit(rotationOverLifetime.yMultiplier, 2f);
                }
            }

            var scrollCurve = noise.scrollSpeed;
            var positionCurve = noise.positionAmount;
            var scrollMax = Helpers.GetMaxCurveValue(scrollCurve);
            var positionMax = Helpers.GetMaxCurveValue(positionCurve);
            if (scrollMax >= Limits.MaxScrollSpeed || positionMax >= Limits.MaxScrollSpeed / 1.25f)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "noise module", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref scrollCurve, Limits.MaxScrollSpeed / 2, ps.name + ".noise.scrollSpeed", shouldLog);
                    noise.scrollSpeed = scrollCurve;
                    Helpers.Limit(ref positionCurve, Limits.MaxScrollSpeed / 2, ps.name + ".noise.positionAmount", shouldLog);
                    noise.positionAmount = positionCurve;
                }
            }

            if (ps.main.startSpeedMultiplier >= 200)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "startSpeedMultiplier", maxParticles))
                        return;
                }
                else
                {
                    ps.main.startSpeedMultiplier = Helpers.Limit(ps.main.startSpeedMultiplier, 100f);
                }
            }

            var startspeedCurve = ps.main.startSpeed;
            var startspeedvalue = Helpers.GetMaxCurveValue(startspeedCurve);
            if (startspeedvalue >= Limits.MaxParticleSimulationSpeed * 100 / 4) // abt 500
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "startSpeed curve", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref startspeedCurve, Limits.MaxParticleSimulationSpeed * 50, ps.name + ".main.startSpeed", shouldLog);
                    ps.main.startSpeed = startspeedCurve;
                }
            }

            if (ps.time > Limits.MaxParticleTime)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"particle time being above limit ({ps?.time} > {Limits.MaxParticleTime})", maxParticles))
                        return;
                }
                else
                {
                    ps.time = Helpers.Limit(ps.time, Limits.MaxParticleTime);
                }
            }

            var startLifetimeCurve = ps.main.startLifetime;
            float startLifetimeValue = Helpers.GetMaxCurveValue(startLifetimeCurve);
            if (startLifetimeValue > Limits.MaxParticleTime || ps.main.startLifetimeMultiplier > Limits.MaxParticleTime / 20)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"duration/lifetime being above limit ({ps?.duration} > {Limits.MaxParticleDuration} | {ps.main.startLifetime} > {Limits.MaxParticleTime} | {ps.main.startLifetimeMultiplier} > {Limits.MaxParticleTime / 20})", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref startLifetimeCurve, Limits.MaxParticleTime, ps.name + ".main.startLifetime", shouldLog);
                    ps.main.startLifetime = startLifetimeCurve;

                    ps.main.startLifetimeMultiplier = Helpers.Limit(ps.main.startLifetimeMultiplier, Limits.MaxParticleTime / 20, ps.name + ".main.startLifetimeMultiplier", shouldLog);
                }
            }

            var inheritvelocityCurve = inheritvelocity.curve;
            float inheritvelocityValue = Helpers.GetMaxCurveValue(inheritvelocityCurve);
            if (inheritvelocityValue >= Limits.MaxParticleSimulationSpeed * 100)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "inheritive velocity curve", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref inheritvelocityCurve, Limits.MaxParticleSimulationSpeed * 50, ps.name + ".inheritVelocity.curve", shouldLog);
                    inheritvelocity.curve = inheritvelocityCurve;
                }
            }

            var inheritvelocityMultiplier = inheritvelocity.curveMultiplier;
            float inheritvelocityMultiplierValue = Mathf.Abs(inheritvelocityMultiplier);
            if (inheritvelocityMultiplierValue >= Limits.MaxParticleSimulationSpeed * 50)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "inherit velocity curveMultiplier", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(inheritvelocityMultiplier, Limits.MaxParticleSimulationSpeed * 25, ps.name + ".inheritVelocity.curveMultiplier", shouldLog);
                    inheritvelocity.curveMultiplier = inheritvelocityMultiplier;
                }
            }

            var forcex = forceOverLifetime.x;
            var forcey = forceOverLifetime.y;
            var forcez = forceOverLifetime.z;
            float forceXValue = Helpers.GetMaxCurveValue(forcex);
            float forceYValue = Helpers.GetMaxCurveValue(forcey);
            float forceZValue = Helpers.GetMaxCurveValue(forcez);
            if (forceXValue > Limits.MaxParticleSimulationSpeed || forceYValue > Limits.MaxParticleSimulationSpeed || forceZValue > Limits.MaxParticleSimulationSpeed)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "force over lifetime", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref forcex, Limits.MaxParticleSimulationSpeed, ps.name + ".forceOverLifetime.x", shouldLog);
                    Helpers.Limit(ref forcey, Limits.MaxParticleSimulationSpeed, ps.name + ".forceOverLifetime.y", shouldLog);
                    Helpers.Limit(ref forcez, Limits.MaxParticleSimulationSpeed, ps.name + ".forceOverLifetime.z", shouldLog);
                    forceOverLifetime.x = forcex;
                    forceOverLifetime.y = forcey;
                    forceOverLifetime.z = forcez;
                }
            }


            if (emission.rateOverDistanceMultiplier > Limits.MaxParticleSimulationSpeed * 5)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "emission rate over distance multiplier", maxParticles))
                        return;
                }
                else
                {
                    emission.rateOverDistanceMultiplier = Helpers.Limit(emission.rateOverDistanceMultiplier, Limits.MaxParticleSimulationSpeed * 5, ps.name + ".emission.rateOverDistanceMultiplier", shouldLog);
                }
            }

            if (ps.main.simulationSpeed > Limits.MaxParticleSimulationSpeed)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"simulation speed being above limit ({ps.main.simulationSpeed} > {Limits.MaxParticleSimulationSpeed})", maxParticles))
                        return;
                }
                else
                {
                    ps.main.simulationSpeed = Helpers.Limit(ps.main.simulationSpeed, Limits.MaxParticleSimulationSpeed, ps.name + ".main.simulationSpeed", shouldLog);
                }
            }

            var trailslifetime = trails.lifetime;
            float trailslifetimevalue = Helpers.GetMaxCurveValue(trailslifetime);
            if (trails.ribbonCount > Limits.MaxParticleRibbons || trailslifetimevalue > Limits.MaxParticleTime || trails.lifetimeMultiplier > 50f || trails.widthOverTrailMultiplier > 50f)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "trails limits", maxParticles))
                        return;
                }
                else
                {
                    trails.ribbonCount = (int)Helpers.Limit(trails.ribbonCount, Limits.MaxParticleRibbons);
                    trails.lifetimeMultiplier = Helpers.Limit(trails.lifetimeMultiplier, 10f);
                    trails.widthOverTrailMultiplier = Helpers.Limit(trails.widthOverTrailMultiplier, 10f);
                    Helpers.Limit(ref trailslifetime, Limits.MaxParticleTime, ps.name + ".trails.lifetime", shouldLog);
                    trails.lifetime = trailslifetime;
                }
            }

            var lightintensity = lights.intensity;
            float lightintensityvalue = Helpers.GetMaxCurveValue(lightintensity);
            if (lights.ratio > 0.1f || lights.maxLights > 10 || lightintensityvalue > Limits.MaxLightIntensity / 2)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "lights limits", maxParticles))
                        return;
                }
                else
                {
                    lights.ratio = Helpers.Limit(lights.ratio, 0.1f, ps.name + ".lights.ratio", shouldLog);
                    lights.maxLights = (int)Helpers.Limit(lights.maxLights, 10, ps.name + ".lights.maxLights", shouldLog);
                    Helpers.Limit(ref lightintensity, Limits.MaxLightIntensity / 2, ps.name + ".lights.intensity", shouldLog);
                    lights.intensity = lightintensity;
                }
            }

            var collisionbounce = collision.bounce;
            float collisionbouncevalue = Helpers.GetMaxCurveValue(collisionbounce);
            if (collision.maxCollisionShapes > Limits.MaxParticleCollisionShapes || collisionbouncevalue > Limits.MaxParticleBounceCount || collision.bounceMultiplier > 3f || collision.maxKillSpeed > Limits.MaxParticleKillSpeed || collision.minKillSpeed > Limits.MaxParticleKillSpeed)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "collision limits", maxParticles))
                        return;
                }
                else
                {
                    collision.maxCollisionShapes = (int)Helpers.Limit(collision.maxCollisionShapes, Limits.MaxParticleCollisionShapes, ps.name + ".collision.maxCollisionShapes", shouldLog);
                    collision.bounceMultiplier = Helpers.Limit(collision.bounceMultiplier, 3f, ps.name + ".collision.bounceMultiplier", shouldLog);
                    collision.maxKillSpeed = Helpers.Limit(collision.maxKillSpeed, Limits.MaxParticleKillSpeed, ps.name + ".collision.maxKillSpeed", shouldLog);
                    collision.minKillSpeed = Helpers.Limit(collision.minKillSpeed, Limits.MaxParticleKillSpeed, ps.name + ".collision.minKillSpeed", shouldLog);
                    Helpers.Limit(ref collisionbounce, Limits.MaxParticleBounceCount, ps.name + ".collision.bounce", shouldLog);
                    collision.bounce = collisionbounce;
                }
            }

            if (shape.mesh != null && shape.mesh.vertexCount > Limits.MaxParticleMeshVertices)
            {
                if (Helpers.Nuke(ps, ref Limits, shouldLog, "shape mesh vertex count", maxParticles))
                    return;
            }

            var rateoverdistance = emission.rateOverDistance;
            float effectiveRod = Helpers.GetMaxCurveValue(rateoverdistance);
            if (effectiveRod >= Limits.MaxParticleSimulationSpeed * 10)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"due to RateOverDistance being too high ({effectiveRod} > {Limits.MaxParticleSimulationSpeed * 10})", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref rateoverdistance, Limits.MaxParticleSimulationSpeed * 5, ps.name + ".emission.rateOverDistance", shouldLog);
                    emission.rateOverDistance = rateoverdistance;
                }
            }

            float burstMax = Helpers.SanitizeEmissionBursts(ps, ref Limits, emission, shouldLog, NukeAnyways, maxParticles);
            var rateovertime = emission.rateOverTime;
            float emissionRateOverTime = Helpers.GetMaxCurveValue(rateovertime);
            float effectiveEmission = Mathf.Max(emissionRateOverTime, burstMax, ps.emissionRate);
            if (effectiveEmission >= Limits.MaxParticleEmissionRate)
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "effective emission rate too high", maxParticles))
                        return;
                }
                else
                {
                    Helpers.Limit(ref rateovertime, Limits.MaxParticleEmissionRate, ps.name + ".emission.rateOverTime", shouldLog);
                    emission.rateOverTime = rateovertime;

                    if (ps.emissionRate > Limits.MaxParticleEmissionRate)
                    {
                        ps.emissionRate = Limits.MaxParticleEmissionRate;
                    }
                }
            }

            // buffer modes are never a standard usage let alone almost ever used in a safe way
            if (ps.emissionRate >= 0 && ps.maxParticles >= 0 && (ps.main.ringBufferMode == ParticleSystemRingBufferMode.LoopUntilReplaced || ps.main.ringBufferMode == ParticleSystemRingBufferMode.PauseUntilReplaced))
            {
                if (NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, $"having high emissionRate and maxParticles while ringbuffer enabled", maxParticles))
                        return;
                }
                else
                {
                    ps.emissionRate = Helpers.Limit(ps.emissionRate, Limits.MaxParticleEmissionRate, ps.name + ".emissionRate", shouldLog);
                    ps.main.maxParticles = (int)Helpers.Limit(ps.main.maxParticles, Limits.MaxParticleCount / 16, ps.name + ".main.maxParticles", shouldLog);
                    ps.main.ringBufferMode = ParticleSystemRingBufferMode.Disabled;
                }
            }

            var lineRenderer = ps.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                // sanitize line renderer used on particle system here
                // for example:
                // limit material count, numCapVertices, numCornerVertices and bounds to prevent extreme cases of very high values causing performance issues e.g
            }

            var trailRenderer = ps.GetComponent<TrailRenderer>();
            if (trailRenderer != null)
            {
                // sanitize trail renderer used on particle system here
                // for example:
                // limit numCapVertices and numCornerVertices to prevent extreme cases of very high values causing performance issues e.g
            }

            Transform transform = ps.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                var childPs = transform.GetChild(i).GetComponent<ParticleSystem>();
                if (childPs != null && childPs.gameObject != null)
                {
                    SanitizeParticleSystem(childPs, ref Limits, shouldLog, NukeAnyways); // sanitize child particle systems on children of the particle system's transform, in case there are any that aren't sub-emitters but could still cause issues
                }
            }

            if (ps != null && ps.gameObject != null)
            {
                var allPS = ps.GetComponents<ParticleSystem>();
                if (allPS != null)
                {
                    for (int i = 0; i < allPS.Count; i++)
                    {
                        var particleSystem = allPS[i];
                        if (particleSystem != null && particleSystem != ps && particleSystem.gameObject != null)
                        {
                            SanitizeParticleSystem(particleSystem, ref Limits, shouldLog, NukeAnyways); // sanitize any other particle systems on the same game object, in case there are multiple and some aren't sub-emitters but could still cause issues
                        }
                    }

                }
            }
        }

    }
}
