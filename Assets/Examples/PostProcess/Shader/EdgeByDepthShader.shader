Shader "Unlit/EdgeByDepthShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
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
                float2 uv[5] : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			half4 _MainTex_TexelSize;
			fixed  _EdgeOnly;
			fixed4 _EdgeColor;
			fixed4 _BackgroundColor;
			float _SampleDistance;
			half4 _Sensitivity;
			sampler2D _CameraDepthNormalsTexture;

			half CheckSame(half4 center,half4 sample_){
				
				// 从深度贴图中解码出法线。
				half3 centerNormal =DecodeViewNormalStereo(center) ;			//center.xy ;
				// （这里用来解码 深度: 因为深度信息被 编码进入zw 分量中）
				float centerDepth  = DecodeFloatRG(center.zw);
				half3 sampleNormal = DecodeViewNormalStereo(sample_);		//sample_.xy;
				float sampleDepth =DecodeFloatRG(sample_.zw);

				//  两个 法线的 差值 （结果受到敏感度的影响）是否小于阈值//CG 中 0 表示否，1 表示true 而且可以混用 。
				half3 diffNormal = abs (centerNormal - sampleNormal)* _Sensitivity.x ;
				// 如果平面，或近似，那么法线差值 约等于零。 那么分量相加 几乎没有。
				// 同一深度，判断边，通过法线。
				bool isSameNormal = (diffNormal.x + diffNormal.y +diffNormal.z)<_Sensitivity.z;
				float diffDepth  = abs(centerDepth- sampleDepth)* _Sensitivity.y ;
				bool isSameDepth = diffDepth <_Sensitivity.w;

				// 以球 举例，如果 没有判定边 法线、深度都是True  // 深度、法线都判断不出边，才是没边
				return isSameNormal&& isSameDepth ? 1.0:0.0;

			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv[0] = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

				o.uv[1] = o.uv[0] + _MainTex_TexelSize.xy * half2(1,1)*_SampleDistance;
				o.uv[2] = o.uv[0] + _MainTex_TexelSize.xy * half2(-1,-1)*_SampleDistance;
				o.uv[3] = o.uv[0] + _MainTex_TexelSize.xy * half2(-1,1)*_SampleDistance;
				o.uv[4] = o.uv[0] + _MainTex_TexelSize.xy * half2(1,-1)*_SampleDistance;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				half4 sample1 = tex2D(_CameraDepthNormalsTexture,i.uv[1]);		// 对深度发现贴图进行采样：右上角
				half4 sample2 = tex2D(_CameraDepthNormalsTexture,i.uv[2]);			// 左下角
				half4 sample3 = tex2D(_CameraDepthNormalsTexture,i.uv[3]);				//左上角
				half4 sample4 = tex2D(_CameraDepthNormalsTexture,i.uv[4]);				//右下角

				half  edge = 1.0;
				edge *= CheckSame(sample1,sample2);
				edge *= CheckSame(sample3,sample4);

				fixed4 withEdgeColor = lerp(_EdgeColor,tex2D(_MainTex,i.uv[0]),edge);
				// 用于只看 边线，观察用。
				fixed4 onlyEdgeColor = lerp(_EdgeColor,_BackgroundColor,edge);

				return lerp(withEdgeColor,onlyEdgeColor,_EdgeOnly);

                //// sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
               // return col;
            }

			

            ENDCG
        }
    }

	FallBack Off
}
