Shader "Edenfall/Cinematic/CinematicDissolve"
{
    Properties
    {
        _MainTex ("Scene CG Image", 2D) = "white" {}
        _NoiseTex ("Dissolve Noise", 2D) = "white" {}
        _Progress ("Transition Progress", Range(0, 1)) = 0.0
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _EdgeColor ("Edge Burn Color", Color) = (1.0, 0.9, 0.5, 1.0)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _Progress;
            float _EdgeWidth;
            fixed4 _EdgeColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed noise = tex2D(_NoiseTex, i.uv).r;

                if (noise < _Progress)
                {
                    discard;
                }

                if (noise < _Progress + _EdgeWidth)
                {
                    col = lerp(_EdgeColor, col, (noise - _Progress) / _EdgeWidth);
                }

                return col;
            }
            ENDCG
        }
    }
}
