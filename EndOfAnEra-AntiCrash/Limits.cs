using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EndOfAnEra_AntiCrash
{
    public class Limits
    {
        // ========================== Particles ==========================
        internal int nukedParticleSystems;
        internal long currentParticleCount;
        internal int particleSystemCount;
        internal int recursionDepth;

        public static int MaxParticleSystems = 256;
        public static int MaxParticleCount = 40000;
        public static float MaxParticleSimulationSpeed = 20f;
        public static int MaxParticleTrails = 6;
        public static float MaxParticleEmissionRate = 150f;
        public static int MaxParticleEmissionBurstCount = 150;
        public static int MaxParticleRibbons = 20000;
        public static int MaxParticleCollisionShapes = 4096;
        public static uint MaxParticleMeshVertices = 250000u;
        public static float MaxParticleDuration = 150000f;
        public static float MaxParticleTime = 600f;
        public static float MaxParticleBounceCount = 1f;
        public static float MaxScrollSpeed = 200f;
        public static float MaxParticleSize = 200f;
        public static int MaxParticleNestingDepth = 2;
        public static float MaxParticleAnimationCycle = 20f;
        public static float MaxChildParticleSystems = 2f;
        public static float MaxParticleKillSpeed = 12500;

        public static Vector2 MaxBufferRange = new Vector2(0, 0);
        public static Vector2 MaxBuffer = new Vector2(0, 0);

        // ========================== Lights ==========================
        internal int nukedLightSources;
        internal int lightSourceCount;
        internal float totalLightIntensity;

        public static int MaxLightSources = 27;
        public static float MaxLightIntensity = 25f;
        public static float MaxLightRange = 25f;
        public static float MaxSpotAngle = 179f;
        public static float MaxShadowStrength = 20f;
        public static float MaxTotalLightIntensity = 25f;

        // ========================== Meshes ==========================
        internal int nukedMeshes;
        internal int meshCount;
        internal int totalSubmeshCount;

        public const int MaxScale = 125;
        public const int MinScale = -125;
        public const int MaxSafeTris = 8000;
        public static uint MaxPolygonsPerMesh = 75000u;
        public static int MaxVerticesPerMesh = 125000;
        public static int MaxMeshes = 1500;
        public static int MaxSubMeshesPerRenderer = 20;
        public static int MaxMeshVertices = 750000;
        public static int MaxMeshTriangles = 450000;
        public static int MaxTotalSubmeshes = 2550;
        public static int MaxOverlappingRenderers = 50000;
        public static int WarnOverlappingRenderers = 30000;
        public static float MaxDegenerateTrianglePercent = 0.75f;

        // ========================== Colliders ==========================
        internal int nukedCollider;
        internal int ColliderCount;
        internal float totalColliderVolume;

        public static float MaxTotalColliderVolume = 2500f;
        public static int MaxColliders = 36;
        public static float MaxColliderSize = 10f;

        // ========================== Cloth ==========================
        internal int nukedCloths;
        internal int clothCount;
        internal int currentVertexCount;

        public static int MaxCloth = 500;
        public static int MaxClothVertices = 15000;
        //public static float MaxClothSolverFrequency = 180f;
        public static int MaxClothCapsuleColliders = 12;

        // ========================== Materials ==========================
        internal int nukedMaterials;
        internal int materialCount;
        internal int emissiveMaterialCount;
        internal int transparentMaterialCount;
        internal int outlineMaterialCount;
        internal int tessellatedMaterialCount;
        internal int totalMaterialPasses;
        internal int duplicateMaterials;
        internal HashSet<string> seenMaterialNames = new HashSet<string>();
        internal HashSet<Material> seenMaterials = new HashSet<Material>();

        public static int MaxMaterials = 3500;
        public static int MaxMaterialLayers = 1500;
        public static int MaxMaterialSlotsPerRenderer = 38;
        public static int MaxDistinctMaterials = 9;
        public static int MaxSameMaterialOnSkinnedMeshRenderers = 26;
        public static int MaxChildSkinnedMeshRenderers = 36;
        public static int MaxDuplicateSkinnedMeshRenderers = 16;
        public static int MaxChildRenderers = 36;
        public static int MaxDuplicateRenderers = 16;
        public static int MaxChildMeshFilters = 36;
        public static int MaxDuplicateMeshFilters = 16;
        public static int MaxDuplicateGameObjects = 36;
        public static int MaxOverallAvatarDuplicates = 68;
        public static float MaxMaterialFloatValue = 1000000f;
        public static float MaxMaterialVectorMagnitude = 1000000f;

        // ========================== Polygons ==========================
        internal uint polygonCount;

        public static uint MaxPolygons = 3250000u;

        // ========================== Shaders ==========================
        internal int nukedShaders;
        internal int shaderCount;

        public static int MaxShaders = 325;
        public static int MaxShaderKeywords = 256;

        // ========================== Blendshapes ==========================
        internal bool removedBlendshapeKeys;
        internal int totalBlendshapeCount;

        public static int MaxBlendshapes = 1000;
        public static int MaxTotalBlendshapes = 10000;

        // ========================== Audio ==========================
        internal int audioSourceCount;
        internal float totalAudioClipLength;
        internal int nukedAudioMixers;
        internal int nukedAudioSources;

        public static int MaxAudioSources = 350;
        public static float MaxAudioClipLength = 100000f;
        public static float MaxAudioVolume = 1.5f;
        public static float MaxTotalAudioClipLength = 1500f; // seconds

        // ========================== Animators ==========================
        internal int animatorCount;
        internal int totalAnimatorParameters;

        public static int MaxAnimators = 151;
        public static int MaxAnimatorLayers = 35;
        public static int MaxAnimatorParameters = 156;
        public static int MaxTotalAnimatorParameters = 256;

        // ========================== Constraints ==========================
        public static int MaxConstraints = 230;
        public static int MaxSpringJoints = 10;

        // ========================== Transforms ==========================
        public static int MaxTransforms = 12000;

        // ========================== MonoBehaviours ==========================
        internal int monoBehaviourCount;
        internal int nukedUnauthorizedComponents;

        public static int MaxMonobehaviours = 1500;

        // ========================== Rigidbodies ==========================
        internal int nukedRigidbodies;
        internal int rigidbodyCount;

        public static int MaxRigidbodies = 68;
        public static float MaxRigidbodyMass = 10000f;
        public static float MaxRigidbodyAngularVelocity = 100f;
        public static float MaxRigidbodyDepenetrationVelocity = 100f;

        // ========================== Cameras ==========================
        internal int nukedCameras;
        internal int cameraCount;

        public static int MaxCameras = 10;
        public static float MaxFieldOfView = 265f;

        // ========================== Bones ==========================
        internal int totalBoneCount;

        public static int MaxBoneCount = 6000;
        public static int MaxTotalBones = 60000;

        // ========================== Renderers ==========================
        internal int trailRendererCount;
        internal int lineRendererCount;
        internal int spriteRendererCount;
        internal int projectorCount;
        internal int decalProjectorCount;
        internal int totalRendererCount;

        public static int MaxTrailRenderers = 21;
        public static int MaxLineRenderers = 10;
        public static int MaxLineRendererPoints = 500;
        public static int MaxSpriteRenderers = 8;
        public static int MaxProjectors = 5;
        public static int MaxDecals = 24;
        public static int MaxTotalRendererCount = 2730;

        // ========================== Textures ==========================
        internal int textureCount;
        internal int nukedTextures;
        internal long totalTextureMemoryBytes;

        public static float MaxTextureOffset = 1000f;
        public static float MaxTextureScale = 1000f;
        public static float MinTextureScale = 0.0001f;
        public static int MaxTexturesPerMaterial = 175;
        public static int MaxSingleTextureMemoryMB = 128;
        public static int MaxTotalTextureMemoryMB = 7208;
        public static float MaxTextureAspectRatio = 512f;
        public static int MaxMipmapLevels = 24;
        public static int Max3DTextureDepth = 32;
        public static int MaxCubemapSize = 1024;
        public static int MaxTextureArraySlices = 20;
        public static int MaxTextureCount = 3840;
        public static int MaxTextureSize = 8192;

        // ========================== Other ==========================
        internal int totalGameObjects;

        public static int MaxTotalComponents = 15000;
        public static int MaxDepth = 255;
        public static int MaxChildren = 255;
        public static float MaxBoundsRadius = 2500000f;
        public static int MaxTotalMaterialPasses = 64;
        public static int MaxTotalGameObjects = 7000;

        // ========================== New Global Limits ==========================
        internal int totalUniqueShaderCount;
        internal HashSet<string> uniqueShaderNames = new HashSet<string>();

        public static int MaxTotalUniqueShaders = 208;
    }
}
