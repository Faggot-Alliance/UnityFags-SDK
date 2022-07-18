// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:32716,y:32678,varname:node_4795,prsc:2|emission-5874-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:31946,y:32708,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Fresnel,id:7578,x:31351,y:32919,varname:node_7578,prsc:2|EXP-9673-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:9459,x:32368,y:32944,ptovrint:False,ptlb:useFresnel,ptin:_useFresnel,varname:node_9459,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-2053-A,B-5691-OUT;n:type:ShaderForge.SFN_Multiply,id:5691,x:32125,y:32958,varname:node_5691,prsc:2|A-2053-A,B-1649-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9673,x:31135,y:32930,ptovrint:False,ptlb:fresPower,ptin:_fresPower,varname:node_9673,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_SwitchProperty,id:1649,x:31955,y:33021,ptovrint:False,ptlb:fresInv,ptin:_fresInv,varname:node_1649,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7578-OUT,B-9824-OUT;n:type:ShaderForge.SFN_OneMinus,id:9824,x:31743,y:33067,varname:node_9824,prsc:2|IN-7578-OUT;n:type:ShaderForge.SFN_Multiply,id:5874,x:32404,y:32732,varname:node_5874,prsc:2|A-2053-RGB,B-5025-OUT,C-9459-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5025,x:32174,y:32796,ptovrint:False,ptlb:emissive,ptin:_emissive,varname:node_5025,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:9459-9673-1649-5025;pass:END;sub:END;*/

Shader "KY/add_onlyColor" {
    Properties {
        [MaterialToggle] _useFresnel ("useFresnel", Float ) = 0
        _fresPower ("fresPower", Float ) = 1
        [MaterialToggle] _fresInv ("fresInv", Float ) = 0
        _emissive ("emissive", Float ) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform fixed _useFresnel;
            uniform float _fresPower;
            uniform fixed _fresInv;
            uniform float _emissive;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float node_7578 = pow(1.0-max(0,dot(normalDirection, viewDirection)),_fresPower);
                float3 emissive = (i.vertexColor.rgb*_emissive*lerp( i.vertexColor.a, (i.vertexColor.a*lerp( node_7578, (1.0 - node_7578), _fresInv )), _useFresnel ));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
