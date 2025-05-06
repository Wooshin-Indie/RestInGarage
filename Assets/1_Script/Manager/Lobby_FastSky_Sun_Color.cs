using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;

[ExecuteAlways]
public class Lobby_FastSky_Sun_Color : MonoBehaviour
{
    Light _light;
    [SerializeField] private Color dayColour;
    [SerializeField] private Color eveningColour;

    //[SerializeField] private Camera reflectionCamera;
    //[SerializeField] private Cubemap cubeMap;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _light.transform.localEulerAngles = new Vector3(63f, -33f, 0f);
        RenderSettings.ambientIntensity = 1.7f;



        float dotProduct = Vector3.Dot(-transform.forward, Vector3.up);
        float clampedDot = Mathf.Clamp((dotProduct + 0.9f), 0, 1);
        float topDot = (1 - Mathf.Clamp01(dotProduct)) * Mathf.Clamp01(Mathf.Sign(dotProduct));
        float bottomDot = (1 - Mathf.Clamp01(-dotProduct)) * Mathf.Clamp01(Mathf.Sign(-dotProduct));
        topDot = Mathf.Pow(math.smoothstep(0f, 0.9f, topDot), 5);
        bottomDot = Mathf.Pow(bottomDot, 5);

        _light.color = Color.Lerp(dayColour, eveningColour, topDot + bottomDot);
    }

    //[ContextMenu("RenderToCubeMap")]
    //public void RenderToCubemap()
    //{
    //    reflectionCamera.RenderToCubemap(cubeMap);
    //    cubeMap.Apply();
    //    RenderSettings.customReflection = cubeMap;
    //}
}
