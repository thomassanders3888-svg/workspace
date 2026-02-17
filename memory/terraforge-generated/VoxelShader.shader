 

Here's a sample shader code that you can modify to achieve your requirements:

```csharp
Shader "Custom/VoxelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AmbientOcclusionMap("Ambient Occlusion Map", 2D) = "black" {}
        _TilingX ("Tiling X", Float) = 1.0
        _TilingY ("Tiling Y", Float) = 1.0
        _TilingZ ("Tiling Z", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _AmbientOcclusionMap;
            float _TilingX, _TilingY, _TilingZ;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * float2(_TilingX, _TilingY);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Ambient occlusion
                float ao = tex2D(_AmbientOcclusionMap, i.uv).r;

                // Triplanar mapping
                float3 n = normalize(i.worldNormal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 uDir = normalize(worldPos + n.x * float3(1, 0, 0));
                float3 vDir = normalize(worldPos + n.y * float3(0, 1, 0));
                float3 wDir = normalize(worldPos + n.z * float3(0, 0, 1));

                fixed4 uCol = tex2D(_MainTex, i.uv.xy / _TilingX);
                fixed4 vCol = tex2D(_MainTex, i.uv.xz / _TilingY);
                fixed4 wCol = tex2D(_MainTex, i.uv.yx / _TilingZ);

                // Blend triplanar colors
                float3 finalColor = uCol.rgb * ao + vCol.rgb
