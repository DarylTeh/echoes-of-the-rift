using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public static class PixelPresentation
{
    public static void Configure(Camera camera)
    {
        if (camera == null) return;
        if (GraphicsSettings.currentRenderPipeline != null)
            throw new System.InvalidOperationException("Pixel presentation requires the Built-in render pipeline.");
        camera.orthographic = true;
        camera.allowHDR = camera.allowMSAA = camera.allowDynamicResolution = false;
        QualitySettings.antiAliasing = 0;
        QualitySettings.shadows=ShadowQuality.Disable;
        QualitySettings.vSyncCount=0;
        Application.targetFrameRate=60;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        var pixels = camera.GetComponent<PixelPerfectCamera>();
        if (pixels == null) pixels = camera.gameObject.AddComponent<PixelPerfectCamera>();
        pixels.assetsPPU = 64;
        pixels.refResolutionX = 1280;
        pixels.refResolutionY = 720;
        pixels.upscaleRT = false;
        pixels.pixelSnapping = true;
        pixels.cropFrameX = pixels.cropFrameY = false;
        pixels.stretchFill = false;
        camera.orthographicSize = 5.625f;
    }
}
