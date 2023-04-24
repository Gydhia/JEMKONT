using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DownBelow.Rendering
{
    public class GridPass : ScriptableRenderPass
    {
        private Material _gridMaterial;
        private Mesh _terrainMesh;

        public GridPass(Material gridMaterial, Mesh terrainMesh)
        {
            this._gridMaterial = gridMaterial;
            this._terrainMesh = terrainMesh;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(name: "GridPass");

            cmd.DrawMesh(_terrainMesh, Matrix4x4.identity, _gridMaterial, 0, 0);
            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }
    }

    public class GridRendererFeature : ScriptableRendererFeature
    {
        private GridPass _gridPass;

        public Material GridMaterial;
        public Mesh TerrainMesh;

        public override void Create()
        {
            _gridPass = new GridPass(GridMaterial, TerrainMesh);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(GridMaterial != null && TerrainMesh != null)
            {
                renderer.EnqueuePass(_gridPass);
            }
        }
    }

}
