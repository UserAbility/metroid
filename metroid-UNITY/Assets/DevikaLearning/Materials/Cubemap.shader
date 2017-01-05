// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Cubemap from textures" {
Properties {
    _FrontTex ("Front (+Z)", 2D) = "white" {}
    _BackTex ("Back (-Z)", 2D) = "white" {}
    _LeftTex ("Left (+X)", 2D) = "white" {}
    _RightTex ("Right (-X)", 2D) = "white" {}
    _UpTex ("Up (+Y)", 2D) = "white" {}
    _DownTex ("down (-Y)", 2D) = "white" {}
}
 
 
 
 
SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" }
    Cull Front ZWrite Off Fog { Mode Off }
    Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma exclude_renderers flash
#include "UnityCG.cginc"
sampler2D _FrontTex;
sampler2D _BackTex;
sampler2D _LeftTex;
sampler2D _RightTex;
sampler2D _UpTex;
sampler2D _DownTex;
 
 
 
 
struct appdata {
    float4 vertex   : POSITION;
    float3 normal   : NORMAL;
};
struct v2f {
    float4 pos  : SV_POSITION;
    float3 n    : TEXCOORD1;
};
v2f vert (appdata v) {
    v2f o;
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    o.n = mul((float3x3)unity_ObjectToWorld,v.normal);
    return o;
}
half4 frag( v2f i ) : COLOR {
 
 
 
 
    float3 n = i.n;
    float3 an= abs(n);
 
 
 
 
    float3 xyz;
    xyz.x = (an.x>max(an.y,an.z)) ? 1 : 0;
    xyz.y = (an.y>max(an.x,an.z)) ? 1 : 0;
    xyz.z = (an.z>max(an.x,an.y)) ? 1 : 0;
    xyz = (n>0) ? xyz : -xyz;
 
 
 
 
    float3 satxyz = saturate(-xyz);
    float3 natxyz = saturate(xyz);
 
 
 
 
    float2 xc =-(float2(n.zy)/n.x)*0.5+0.5;
    float2 yc =-(float2(n.zx)/n.y)*0.5+0.5;
    float2 zc = (float2(n.xy)/n.z)*0.5+0.5;
    float3 side1 = tex2D(_FrontTex,xc).xyz;
    float3 side4 = tex2D(_DownTex,yc).xyz;
    float3 side6 = tex2D(_LeftTex,zc).xyz;
    xc.y = 1-xc.y;
    yc.y = 1-yc.y;
    zc.y = 1-zc.y;
    float3 side2 = tex2D(_BackTex,xc).xyz;
    float3 side3 = tex2D(_UpTex,yc).xyz;
    float3 side5 = tex2D(_RightTex,zc).xyz;
   
    float3 cube =
        side1 * satxyz.x + side2 * natxyz.x +
        side3 * satxyz.y + side4 * natxyz.y +
        side5 * satxyz.z + side6 * natxyz.z;
    return half4(cube,0);
}
ENDCG
        }
    }
}