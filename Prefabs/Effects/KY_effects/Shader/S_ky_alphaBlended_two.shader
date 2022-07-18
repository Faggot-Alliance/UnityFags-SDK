// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:32724,y:32693,varname:node_4795,prsc:2|emission-2393-OUT,alpha-6616-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:31249,y:32483,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:0b63641d1fc3f44a3b265ed4e1b5ac27,ntxv:0,isnm:False|UVIN-5734-OUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32495,y:32793,varname:node_2393,prsc:2|A-320-OUT,B-2053-RGB,C-8175-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32126,y:32802,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:8175,x:32141,y:32954,ptovrint:False,ptlb:emissivePower,ptin:_emissivePower,varname:node_8175,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Power,id:320,x:32359,y:32601,varname:node_320,prsc:2|VAL-3723-OUT,EXP-102-OUT;n:type:ShaderForge.SFN_ValueProperty,id:102,x:32098,y:32697,ptovrint:False,ptlb:texDensity,ptin:_texDensity,varname:node_102,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_SwitchProperty,id:3723,x:32147,y:32515,ptovrint:False,ptlb:useTexColor,ptin:_useTexColor,varname:node_3723,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6074-A,B-6074-RGB;n:type:ShaderForge.SFN_ValueProperty,id:698,x:31540,y:33096,ptovrint:False,ptlb:alphaDensity,ptin:_alphaDensity,varname:node_698,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Power,id:9039,x:31868,y:33023,varname:node_9039,prsc:2|VAL-6074-A,EXP-698-OUT;n:type:ShaderForge.SFN_Multiply,id:6616,x:32371,y:33057,varname:node_6616,prsc:2|A-2053-A,B-9039-OUT;n:type:ShaderForge.SFN_TexCoord,id:4641,x:30658,y:32376,varname:node_4641,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:5581,x:30479,y:32724,varname:node_5581,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:4589,x:30276,y:32510,ptovrint:False,ptlb:texSpdX,ptin:_texSpdX,varname:node_4589,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:1756,x:30246,y:32605,ptovrint:False,ptlb:texSpdY,ptin:_texSpdY,varname:node_1756,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:2266,x:30467,y:32510,varname:node_2266,prsc:2|A-4589-OUT,B-1756-OUT;n:type:ShaderForge.SFN_Multiply,id:9196,x:30746,y:32636,varname:node_9196,prsc:2|A-2266-OUT,B-5581-T;n:type:ShaderForge.SFN_Add,id:5734,x:31052,y:32469,varname:node_5734,prsc:2|A-4641-UVOUT,B-9196-OUT;proporder:6074-8175-102-3723-698-4589-1756;pass:END;sub:END;*/

Shader "KY/alphaBlended_two" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _emissivePower ("emissivePower", Float ) = 1
        _texDensity ("texDensity", Float ) = 1
        [MaterialToggle] _useTexColor ("useTexColor", Float ) = 0
        _alphaDensity ("alphaDensity", Float ) = 1
        _texSpdX ("texSpdX", Float ) = 0
        _texSpdY ("texSpdY", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
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
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _emissivePower;
            uniform float _texDensity;
            uniform fixed _useTexColor;
            uniform float _alphaDensity;
            uniform float _texSpdX;
            uniform float _texSpdY;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_5581 = _Time + _TimeEditor;
                float2 node_5734 = (i.uv0+(float2(_texSpdX,_texSpdY)*node_5581.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_5734, _MainTex));
                float3 emissive = (pow(lerp( _MainTex_var.a, _MainTex_var.rgb, _useTexColor ),_texDensity)*i.vertexColor.rgb*_emissivePower);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(i.vertexColor.a*pow(_MainTex_var.a,_alphaDensity)));
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
