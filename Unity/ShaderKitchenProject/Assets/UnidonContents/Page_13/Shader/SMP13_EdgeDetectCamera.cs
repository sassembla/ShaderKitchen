using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
public class SMP13_EdgeDetectCamera:ImageEffectBase {

	void Start () {
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit (source, destination, material);
	}
}
