Shader "UI/AlphaMaskHole"
{
    Properties
    {
        _MainTex ("Base (Mask Texture)", 2D) = "white" {}
        _Cutoff ("Cutoff", Range(0, 1)) = 0.5
        _ScreenAspect ("Screen Aspect", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;
            float _ScreenAspect;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Centered UV (0,0) at center, range [-1, 1]
                float2 centeredUV = i.uv * 2.0 - 1.0;
                centeredUV.x *= _ScreenAspect;

                // distance from center
                float dist = length(centeredUV);

                // Sample texture
                float4 tex = tex2D(_MainTex, i.uv);

                // Mask by texture alpha and cutoff
                float mask = step(dist, _Cutoff * _ScreenAspect * 1.5f) * tex.a;

                // Inside the mask: transparent (show scene), outside: black
                return lerp(float4(0, 0, 0, 1), float4(0, 0, 0, 0), mask);
            }
            ENDCG
        }
    }
}
