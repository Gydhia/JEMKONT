using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class UniversalRenderPipelineHelper
{
    private static FieldInfo RenderDataList_FieldInfo;

    static UniversalRenderPipelineHelper()
    {
        try
        {
            var pipelineAssetType = typeof(UniversalRenderPipelineAsset);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            RenderDataList_FieldInfo = pipelineAssetType.GetField("m_RendererDataList", flags);
        }
        catch (System.Exception e)
        {
            Debug.LogError("UniversalRenderPipelineHelper reflection cache build failed. Maybe the API has changed? \n" + e.Message);
        }
    }

    public static ScriptableRendererData[] GetRendererDataList(UniversalRenderPipelineAsset asset = null)
    {
        try
        {
            if (asset == null)
            {
                asset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            }
            if (asset == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("GetRenderFeature() current renderpipleline is null.");
#endif
                return null;
            }

            if (RenderDataList_FieldInfo == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("GetRenderFeature() reflection failed to get m_RendererDataList field.");
#endif
                return null;
            }

            var renderDataList = (ScriptableRendererData[])RenderDataList_FieldInfo.GetValue(asset);
            return renderDataList;
        }
        catch
        {
            // Fail silently if reflection failed.
            return null;
        }
    }

    public static T GetRendererFeature<T>(UniversalRenderPipelineAsset asset = null) where T : ScriptableRendererFeature
    {
        var renderDataList = GetRendererDataList();
        if (renderDataList == null || renderDataList.Length == 0)
            return null;

        foreach (var renderData in renderDataList)
        {
            foreach (var rendererFeature in renderData.rendererFeatures)
            {
                if (rendererFeature is T)
                {
                    return rendererFeature as T;
                }
            }
        }

        return null;
    }

    public static ScriptableRendererFeature GetRendererFeature(string typeName, UniversalRenderPipelineAsset asset = null)
    {
        var renderDataList = GetRendererDataList();
        if (renderDataList == null || renderDataList.Length == 0)
            return null;

        foreach (var renderData in renderDataList)
        {
            foreach (var rendererFeature in renderData.rendererFeatures)
            {
                if (rendererFeature == null)
                    continue;

                if (rendererFeature.name.Contains(typeName))
                {
                    return rendererFeature;
                }
            }
        }

        return null;
    }

    public static bool IsRendererFeatureActive<T>(UniversalRenderPipelineAsset asset = null, bool defaultValue = false) where T : ScriptableRendererFeature
    {
        var feature = GetRendererFeature<T>(asset);
        if (feature == null)
            return defaultValue;

        return feature.isActive;
    }

    public static bool IsRendererFeatureActive(string typeName, UniversalRenderPipelineAsset asset = null, bool defaultValue = false)
    {
        var feature = GetRendererFeature(typeName, asset);
        if (feature == null)
            return defaultValue;

        return feature.isActive;
    }

    public static void SetRendererFeatureActive<T>(bool active, UniversalRenderPipelineAsset asset = null) where T : ScriptableRendererFeature
    {
        var feature = GetRendererFeature<T>(asset);
        if (feature == null)
            return;

        feature.SetActive(active);
    }

    public static void SetRendererFeatureActive(string typeName, bool active, UniversalRenderPipelineAsset asset = null)
    {
        var feature = GetRendererFeature(typeName, asset);
        if (feature == null)
            return;

        feature.SetActive(active);
    }
}