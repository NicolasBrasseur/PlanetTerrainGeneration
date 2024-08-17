Shader "Custom/Atmosphere"
{
    Properties
    {
         _BaseColor ("Color", Color) = (1,1,1,1)
         _HorizonColor("HorizonColor", Color) = (1,1,1,1)
         _Radius("Radius", float) = 1
         _Density("Density", float) = 1
         _DensityPower("EdgePower", float) = 1
         _LightingRadius("LightingRadius", Range(-2, 2)) = 0
         _PlanetVisibility("PlanetVisibilityModifier", Range(-20, 50)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float3 _BaseColor;
                float3 _HorizonColor;
                float _Radius;
                float _Density;
                float _DensityPower;
                float _LightingRadius;
                float _PlanetVisibility;
            CBUFFER_END
        ENDHLSL

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
    	    Cull Front

        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_DepthTexture);
            SAMPLER(sampler_DepthTexture);

            struct vertData
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 N : ATTR0;
                float3 wPos : ATTR1;
                float3 viewDir : ATTR2;
                float4 screenPos : ATTR3;
                float4 viewPos : ATTR4;
            };

            float3 raySphereIntersect(float3 rayOrigin, float3 rayDirection, float3 sphereOrigin, float sphereRadius, float max_depth) 
            {
                float t = dot(sphereOrigin - rayOrigin, rayDirection);
                float3 P = rayOrigin + rayDirection * t;
                float y = length(sphereOrigin - P);

                if(y > sphereRadius) 
                {
                    return float3(-1.0, -1.0, 0);
                }

                float x = sqrt(sphereRadius * sphereRadius - y * y);
                float t1 = max(t - x, 0.0);
                float t2 = min(t + x, max_depth);

                return float3(t1, t2, t);
            }

            v2f vert(vertData v)
            {
                v2f o = (v2f)0;

                o.pos = TransformObjectToHClip(v.pos.xyz);
                o.N = -mul((float3x3)unity_ObjectToWorld, v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.pos).xyz;
                float3 viewPos = TransformWorldToView(o.wPos.xyz);
                o.viewPos = float4(viewPos,1.0f);
                o.screenPos = ComputeScreenPos(o.pos);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.N);
                float3 L = float3(0,1,0);
                float diffuse = max(dot(N,L),0);

                float rawDepth = SampleSceneDepth(i.screenPos.xy / i.screenPos.w);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float cameraDistance = -sceneEyeDepth / normalize(i.viewPos.xyz).z;

                float3 camPos = _WorldSpaceCameraPos;
                float3 baseWorldPos = unity_ObjectToWorld._m03_m13_m23;

                float3 viewDir = normalize(i.wPos - camPos.xyz);
                float3 rsi = raySphereIntersect(camPos, viewDir, baseWorldPos, _Radius, cameraDistance - _PlanetVisibility + 20.0f);

                Light mainLight = GetMainLight();
                float3 lightDirection = normalize(mainLight.direction);
                float3 frontNormalDirection = normalize((camPos + viewDir * max(rsi.x, 0.0)) - baseWorldPos);
                float3 backNormalDirection = normalize((camPos + viewDir * max(rsi.y, 0.0)) - baseWorldPos);

                float4 o;

                float thickness = pow(max(0, rsi.y - rsi.x), (_DensityPower)) * (_Density / (_Radius * _Radius)) * pow(10, -_DensityPower);
                float exposition = saturate((dot(frontNormalDirection, lightDirection) + dot(backNormalDirection, lightDirection) + _LightingRadius));
                float lightedDensity = max(thickness * exposition, 0.0);

                o.rgb = lerp(_HorizonColor, _BaseColor, exposition); //* diffuse;
                o.a = clamp(lightedDensity / _Radius, 0.0, 1.0);

                return o;
            }
        ENDHLSL
        }
    }
}
