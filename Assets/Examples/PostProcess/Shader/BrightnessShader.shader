Shader "Unlit/BrightnessShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Brightnees("Brightnees",float)= 1
		_Saturation ("Saturation",float) = 1
		_Contarast ("Contarast",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			ZTest  Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"



            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			half _Brightnees;
			half _Saturation;
			half _Contarast;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				
				//为什么：这么算，我是真的看不懂？
				float3 finalColor = col .rgb * _Brightnees;    // 亮度看得懂
				fixed luminace = 0.2125 * col.r + 0.7154*col.g+0.0721*col.b;  //饱和度
				fixed3 luminaceColor = fixed3(luminace,luminace,luminace);
				finalColor = lerp(luminaceColor,finalColor,_Saturation);
				fixed3 avgColor = fixed3 (0.5,0.5,0.5);       // 对比度，我感觉更像灰度。
				finalColor = lerp(avgColor,finalColor,_Contarast);
				return fixed4(finalColor,col.a);
                // apply fog
               // UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
            }
            ENDCG
        }

    }
		FallBack Off
}
