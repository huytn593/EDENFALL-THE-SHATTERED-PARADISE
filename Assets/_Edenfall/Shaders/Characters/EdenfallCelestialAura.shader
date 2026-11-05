Shader "Edenfall/Characters/CelestialAura"
{
    Properties
    {
        _MainTex ("Character Sprite", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _AuraColor ("Aura Glow Color", Color) = (1.0, 0.85, 0.4, 1.0)
        _AuraIntensity ("Aura Intensity", Range(0, 5)) = 1.8
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _RimThickness ("Rim Thickness", Range(0.001, 0.05)) = 0.015
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
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
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _AuraColor;
            float _AuraIntensity;
            float _PulseSpeed;
            float _RimThickness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Sample neighbors for aura rim detection
                float2 offset = float2(_RimThickness, _RimThickness);
                float aUp    = tex2D(_MainTex, IN.texcoord + float2(0, offset.y)).a;
                float aDown  = tex2D(_MainTex, IN.texcoord - float2(0, offset.y)).a;
                float aLeft  = tex2D(_MainTex, IN.texcoord - float2(offset.x, 0)).a;
                float aRight = tex2D(_MainTex, IN.texcoord + float2(offset.x, 0)).a;

                float outline = saturate(aUp + aDown + aLeft + aRight) - mainColor.a;
                outline = max(0.0, outline);

                float pulse = 0.85 + 0.15 * sin(_Time.y * _PulseSpeed);
                fixed4 aura = _AuraColor * _AuraIntensity * pulse * outline;

                fixed4 finalColor = mainColor + aura;
                finalColor.a = max(mainColor.a, outline * _AuraColor.a);
                return finalColor;
            }
            ENDCG
        }
    }
}
