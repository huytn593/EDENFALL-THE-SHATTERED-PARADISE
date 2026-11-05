Shader "Edenfall/Environment/VegetationWind"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _WindSpeed ("Wind Speed", Float) = 2.5
        _WindAmplitude ("Wind Amplitude", Float) = 0.08
        _WindFrequency ("Wind Frequency", Float) = 4.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _WindSpeed;
            float _WindAmplitude;
            float _WindFrequency;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                float4 v = IN.vertex;

                // Wind displacement affects vertices with high Y (tops of plants sway more)
                float sway = sin(_Time.y * _WindSpeed + v.x * _WindFrequency) * _WindAmplitude * max(0.0, v.y);
                v.x += sway;

                OUT.vertex = UnityObjectToClipPos(v);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                return c;
            }
            ENDCG
        }
    }
}
