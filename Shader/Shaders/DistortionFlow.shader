Shader "Custom/DistortionFlow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset]_FlowMap("Flow(RG,A noise)",2D)="black"{}
        [NoScaleOffset]_DerivHeightMap("Deriv(AG) Height(B)",2D) = "black"{}
        _UJump("U jump per pahse",Range(-0.25,0.25))=0.25
        _VJump("V jump per pahse",Range(-0.25,0.25)) = 0.25
        _Tiling("Tiling", Float) = 1
        _Speed("Speed", Float) = 1
        _FlowOffset("Flow Offset", Float) = 0
        _FlowStrength("Flow Strength", Float) = 1
        _HeightScale("Height Scale,Constant",Float) = 0.25
        _HeightScaleModulated("Height Scale,Modulated", Float) = 0.75
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "Flow.cginc"
        sampler2D _MainTex,_FlowMap,_DerivHeightMap;
        float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset;
        float _HeightScale, _HeightScaleModulated;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float3 FlowUVW(float2 uv,float2 flowVector,float2 jump,float flowOffset,float tiling,float time,bool flowB){
            float phaseOffset = flowB ? 0.5 : 0;
            float progress=frac(time+ phaseOffset);
            float3 uvw;
            uvw.xy = uv - flowVector * (progress- flowOffset);
            uvw.xy *= tiling;
            uvw.xy += phaseOffset;
            uvw.xy += (time - progress) * jump;
            uvw.z=1-abs(1-2*progress);
            return uvw;
        }
        float3 UnpackDerivativeHeight(float4 textureData) {
            float3 dh = textureData.agb;
            dh.xy = dh.xy * 2 - 1;
            return dh;
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //float2 flowVector=tex2D(_FlowMap,IN.uv_MainTex).rg*2-1;
            float3 flow = tex2D(_FlowMap, IN.uv_MainTex).rgb;
            flow.xy = flow.xy * 2 - 1;
            flow *= _FlowStrength;
            //flowVector *= _FlowStrength;
            float noise=tex2D(_FlowMap,IN.uv_MainTex).a;
            float time = _Time.y* _Speed +noise;
            float jump = float2(_UJump, _VJump);

            float3 uvwA = FlowUVW(IN.uv_MainTex, flow.xy,jump, _FlowOffset, _Tiling,time, false);
            float3 uvwB = FlowUVW(IN.uv_MainTex, flow.xy,jump, _FlowOffset, _Tiling,time, true);

            float finalHeightScale = flow.z * _HeightScaleModulated + _HeightScale;
            float3 dhA =
                UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwA.xy)) * (uvwA.z* finalHeightScale);
            float3 dhB =
                UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwB.xy)) * (uvwB.z* finalHeightScale);
            o.Normal = normalize(float3(-(dhA.xy + dhB.xy), 1));
            fixed4 texA = tex2D (_MainTex,uvwA.xy) *uvwA.z * _Color;
            fixed4 texB = tex2D(_MainTex, uvwB.xy) * uvwB.z * _Color;
            fixed4 c = (texA + texB) * _Color;
            o.Albedo = c.rgb;
            //o.Albedo = pow(dhA.z + dhB.z, 2);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
