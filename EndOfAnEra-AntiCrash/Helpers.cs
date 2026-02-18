using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace EndOfAnEra_AntiCrash
{
    internal class Helpers
    {
        internal static bool Nuke(ParticleSystem ps, ref Limits Limits, bool shouldLog, string reason, int? maxParticlesToRemove = null)
        {
            if (maxParticlesToRemove.HasValue)
                Limits.currentParticleCount -= maxParticlesToRemove.Value;

            Limits.nukedParticleSystems++;

            if (shouldLog) Console.WriteLine($"[AvatarSanitizer] Removed ParticleSystem '{ps?.name}' because of '{reason}'");

            UnityEngine.Object.DestroyImmediate(ps, true);
            return true;
        }

        internal static float Limit(float value, float max, string name = "", bool shouldLog = false)
        {
            if (value > max)
            {
                if (shouldLog)
                {
                    Console.WriteLine($"[AvatarSanitizer] Clamped {name} from {value} to {max}");
                }
            }

            value = Mathf.Min(value, max);
            return value;
        }

        internal static void Limit(ref ParticleSystem.MinMaxCurve curve, float max, string name = "ParticleSystem", bool shouldLog = false)
        {
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    {
                        float old = curve.constant;
                        if (old > max)
                        {
                            curve.constant = max;
                            if (shouldLog)
                                Console.WriteLine($"[AvatarSanitizer] Clamped constant {name} from {old} to {max}");
                        }
                        break;
                    }

                case ParticleSystemCurveMode.TwoConstants:
                    {
                        float oldMin = curve.constantMin;
                        float oldMax = curve.constantMax;
                        bool changed = false;

                        if (oldMin > max) { curve.constantMin = max; changed = true; }
                        if (oldMax > max) { curve.constantMax = max; changed = true; }

                        if (changed && shouldLog)
                            Console.WriteLine($"[AvatarSanitizer] Clamped TwoConstants {name} from min:{oldMin}, max:{oldMax} to max:{max}");
                        break;
                    }

                case ParticleSystemCurveMode.Curve:
                    LimitCurve(curve.curve, max, shouldLog, name);
                    break;

                case ParticleSystemCurveMode.TwoCurves:
                    LimitCurve(curve.curveMin, max, shouldLog, name + ".curveMin");
                    LimitCurve(curve.curveMax, max, shouldLog, name + ".curveMax");
                    break;
            }
        }

        private static void LimitCurve(AnimationCurve? curve, float max, bool shouldLog, string name)
        {
            if (curve == null) return;

            var keys = curve.keys;
            bool changed = false;

            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (key.value > max)
                {
                    if (shouldLog)
                        Console.WriteLine($"[AvatarSanitizer] Clamped curve key {name} from {key.value} to {max}");

                    key.value = max;
                    keys[i] = key;
                    changed = true;
                }
            }

            if (changed)
                curve.keys = keys;
        }

        internal static long EstimateSubEmitterParticles(ParticleSystem.SubEmittersModule subs)
        {
            long total = 0;
            for (int i = 0; i < subs.subEmittersCount; i++)
            {
                var sub = subs.GetSubEmitterSystem(i);
                if (sub != null)
                    total += sub.main.maxParticles;
            }
            return total;
        }

        internal static float SanitizeEmissionBursts(ParticleSystem ps, ref Limits Limits, ParticleSystem.EmissionModule emission, bool shouldLog, bool NukeAnyways, int maxParticles)
        {
            float burstMax = 0f;

            if (emission.burstCount > 0)
            {
                var bursts = new Il2CppReferenceArray<ParticleSystem.Burst>(emission.burstCount);
                emission.GetBursts(bursts);

                bool burstTooLarge = false;
                int maxAllowedBursts = Limits.MaxParticleEmissionBurstCount;

                for (int i = 0; i < bursts.Length; i++)
                {
                    var b = bursts[i];
                    float maxCountValue = Helpers.GetMaxCurveValue(b.count);

                    burstMax = Mathf.Max(burstMax, Mathf.Max(maxCountValue, b.cycleCount, b.maxCount));

                    if (maxCountValue > maxAllowedBursts ||
                        b.cycleCount > maxAllowedBursts ||
                        b.maxCount > maxAllowedBursts)
                    {
                        burstTooLarge = true;
                    }
                }

                if (burstTooLarge && NukeAnyways)
                {
                    if (Helpers.Nuke(ps, ref Limits, shouldLog, "emission bursts too large", maxParticles))
                        return 0f;
                }
                else if (burstTooLarge)
                {
                    for (int i = 0; i < bursts.Length; i++)
                    {
                        var b = bursts[i];
                        var count = b.count;

                        Helpers.Limit(ref count, maxAllowedBursts, ps.name + $".Burst[{i}].count", shouldLog);
                        b.count = count; // reassign after limiting - forgot to do originally whoops

                        b.cycleCount = (int)Helpers.Limit(b.cycleCount, maxAllowedBursts, ps.name + $".Burst[{i}].cycleCount", shouldLog);
                        b.maxCount = (short)Helpers.Limit(b.maxCount, maxAllowedBursts, ps.name + $".Burst[{i}].maxCount", shouldLog);

                        bursts[i] = b;
                    }

                    if (bursts.Length > maxAllowedBursts)
                    {
                        var trimmed = new Il2CppReferenceArray<ParticleSystem.Burst>(maxAllowedBursts);
                        for (int i = 0; i < maxAllowedBursts; i++)
                            trimmed[i] = bursts[i];

                        bursts = trimmed;
                        if (shouldLog)
                            Console.WriteLine($"[AvatarSanitizer] Trimmed bursts to {maxAllowedBursts} for {ps.name}");
                    }

                    emission.SetBursts(bursts);
                }
            }

            return burstMax;
        }

        internal static float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
        {
            return curve.mode switch
            {
                ParticleSystemCurveMode.Constant => curve.constant,
                ParticleSystemCurveMode.TwoConstants => Mathf.Max(curve.constantMin, curve.constantMax),
                ParticleSystemCurveMode.Curve => curve.curve?.keys.Max(k => k.value) ?? 0f,
                ParticleSystemCurveMode.TwoCurves => Mathf.Max(curve.curveMin?.keys.Max(k => k.value) ?? 0f, curve.curveMax?.keys.Max(k => k.value) ?? 0f),
                _ => 0f
            };
        }
    }
}
