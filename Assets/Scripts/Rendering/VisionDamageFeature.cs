using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace PolishTanks.Rendering
{
    /// <summary>
    /// Renderer feature that spoils the game view where broken vision blocks are: each one covers a few patches of
    /// the screen with cracked glass, blur and grey static, opaque once the block is destroyed, plus a red pulse at
    /// the screen edges while the engine burns. Reads <see cref="VisionDamageState"/> and only adds its pass to game cameras while
    /// the state is active, so an undamaged tank costs nothing.
    /// </summary>
    public class VisionDamageFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader shader;
        [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        private Material material;
        private VisionDamagePass pass;

        public override void Create()
        {
            CoreUtils.Destroy(material);
            material = null;
            pass = null;
            if (shader == null)
                return;
            material = CoreUtils.CreateEngineMaterial(shader);
            pass = new VisionDamagePass(material) { renderPassEvent = passEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (pass != null && VisionDamageState.IsActive)
                renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
            material = null;
            pass = null;
        }

        private sealed class VisionDamagePass : ScriptableRenderPass
        {
            private static readonly int CenterId = Shader.PropertyToID("_VisionCenter");
            private static readonly int ParamsId = Shader.PropertyToID("_VisionParams");
            private static readonly int LossAId = Shader.PropertyToID("_VisionLossA");
            private static readonly int LossBId = Shader.PropertyToID("_VisionLossB");

            private readonly Material material;

            public VisionDamagePass(Material material)
            {
                this.material = material;
                requiresIntermediateTexture = true;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var cameraData = frameData.Get<UniversalCameraData>();
                if (cameraData.cameraType != CameraType.Game || !cameraData.camera.orthographic)
                    return;
                var resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer)
                    return;

                Apply(cameraData.camera);
                var source = resources.activeColorTexture;
                var description = renderGraph.GetTextureDesc(source);
                description.name = "_VisionDamageColor";
                description.clearBuffer = false;
                var destination = renderGraph.CreateTexture(description);
                renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, destination, material, 0),
                    "Vision Damage");
                resources.cameraColor = destination;
            }

            private void Apply(Camera camera)
            {
                var viewport = camera.WorldToViewportPoint(VisionDamageState.Position);
                var clearRadius = VisionDamageState.ClearRadius / (2f * camera.orthographicSize);
                material.SetVector(CenterId, new Vector4(viewport.x, viewport.y, camera.aspect, clearRadius));
                material.SetVector(ParamsId, new Vector4(VisionDamageState.Seed, VisionDamageState.Blocks, VisionDamageState.FireAlarm, 0f));
                material.SetVector(LossAId, VisionDamageState.LossA);
                material.SetVector(LossBId, VisionDamageState.LossB);
            }
        }
    }
}
