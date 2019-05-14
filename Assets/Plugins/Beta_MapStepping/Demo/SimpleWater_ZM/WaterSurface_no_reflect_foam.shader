// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WaterSurface_no_reflect_foam"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_RefractTex ("Refract Texture", 2D) = "white" {}
		_NormalMask ("Normal Mask", 2D) = "white" {}
		//_ReflectTex("Reflect Texture", 2D) = "white"{}
		_BumpTex ("Bump Texture", 2D) = "white"{}
		_FlowTex ("Flow Texture", 2D) = "white"{}
		_FlowSpeed ("Flow Speed", Range(-10.0, 10.0)) = 1.0
		_BumpStrength ("Bump strength", Range(0.0, 10.0)) = 1.0
		_BumpDirection ("Bump direction(2 wave)", Vector)=(1,1,0,0)
		_BumpTiling ("Bump tiling", Vector)=(0.0625,0.0625,0,0)
		_FresnelTex("Fresnel Texture", 2D) = "white" {}
		_Skybox("skybox", Cube)="white"{}
		_Specular("Specular Color", Color)=(1,1,1,0.5)
		_Params("shiness,Refract Perturb,Reflect Perturb", Vector)=(128, 0.025, 0.05, 0)
		_SunDir("sun direction", Vector)=(0,0,0,0)
		_FoamTex ("Foam Texture (R)", 2D) = "black" {}
		_RollFoamU ("Roll Foam U", Float) = 1
	    _RollFoamV ("Roll Foam V", Float) = 0
	    _FoamRange ("Foam Range", Float) = 2
	    _DepthRange ("Depth Range", Float) = 2
	    _DepthColor("Depth Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
		LOD 100

		Pass
		{
			offset 1,1
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			uniform sampler2D _CameraDepthTexture;
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
				float3 normal : NORMAL;
				//float2 texcoord7 : TEXCOORD7;
				//float2 uv2:TEXCOORD4;
				//float3 normal:NORMAL;
				//float3 tangent:TANGENT;
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
				half2 FoamTex : TEXCOORD4;
				half2 uv1 : TEXCOORD7;
				//half2 uv2 : TEXCOORD8;
				float3 normalDir : TEXCOORD5;
				float4 posWorld : TEXCOORD06;
				half2 bumpCoords:TEXCOORD1;
				half2 bumpCoords2:TEXCOORD8;
				half3 viewVector:TEXCOORD2;
				float4 projPos : TEXCOORD3;
				half4 vertex : SV_POSITION;
			};
			float _RollFoamU;
			float _RollFoamV;
			half4 _Color;
			sampler2D _RefractTex;
			sampler2D _NormalMask;
			sampler2D _FoamTex;
			sampler2D _FlowTex;
			float4 _FlowTex_ST;
			float4 _FoamTex_ST;
			float4 _RefractTex_ST;
			float4 _NormalMask_ST;
			sampler2D _BumpTex;
			half _BumpStrength;
			half _FlowSpeed;
			half4 _BumpDirection;
			half4 _BumpTiling;
			sampler2D _FresnelTex;
			samplerCUBE _Skybox;
			half4 _Specular;
			half4 _Params;
			half4 _SunDir;
			half4 _DepthColor;
			float _FoamRange;
			float _DepthRange;
			
			half3 PerPixelNormal(sampler2D bumpMap, half2 coords, half bumpStrength) 
			{
				float2 bump = (UnpackNormal(tex2D(bumpMap, coords.xy))) * 0.5;
				//float2 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)) + UnpackNormal(tex2D(bumpMap, coords.zw))) * 0.5;
				//bump += (UnpackNormal(tex2D(bumpMap, coords.xy*2))*0.5 + UnpackNormal(tex2D(bumpMap, coords.zw*2))*0.5) * 0.5;
				//bump += (UnpackNormal(tex2D(bumpMap, coords.xy*8))*0.5 + UnpackNormal(tex2D(bumpMap, coords.zw*8))*0.5) * 0.5;
				float3 worldNormal = float3(0,0,0);
				worldNormal.xz = bump.xy * bumpStrength;
				worldNormal.y = 1;
				return worldNormal;
			}
			
			inline float FastFresnel(half3 I, half3 N, half R0)
			{
				float icosIN = saturate(1-dot(I, N));
				float i2 = icosIN*icosIN;
				float i4 = i2*i2;
				return R0 + (1-R0)*(i4*icosIN);
			}


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				half3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				half4 screenPos = ComputeScreenPos(o.vertex);
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				o.uv.xy = v.uv;
				//o.uv1 = v.texcoord7;
				o.FoamTex = TRANSFORM_TEX(v.uv, _FoamTex);
				//o.RefractTex = TRANSFORM_TEX(v.uv, _RefractTex);
				o.bumpCoords = (worldPos.xz + _Time.yy * _BumpDirection.xy) * _BumpTiling.xy;
				o.bumpCoords2 = (worldPos.xz + _Time.yy * _BumpDirection.zw) * _BumpTiling.zw;
				//o.bumpCoords.xyzw = (worldPos.xzxz + _Time.yyyy * _BumpDirection.xyzw) * _BumpTiling.xyzw;
				o.uv1 = v.uv;
				o.viewVector = (worldPos - _WorldSpaceCameraPos.xyz);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    
				float2 refuv = (i.posWorld.xz/10);
				half4 refractColor = tex2D(_RefractTex,TRANSFORM_TEX(refuv, _RefractTex))*_Color;
				half3 flowColor = ((tex2D(_FlowTex,i.uv)* 2-1) * _FlowSpeed);
				half3 flowColor2 = ((tex2D(_FlowTex,i.uv)* 2-1) * _FlowSpeed);
				float dif1 = frac(_Time.y * 0.25 + 0.5);
                float dif2 = frac(_Time.y * 0.25 );

                half lerpVal = abs((0.5 - dif1)/0.5);

				fixed4 result = fixed4(0,0,0,1);
				fixed4 allresult = fixed4(0,0,0,1);
				//i.bumpCoords.xyzw = (i.posWorld.xzxz + _Time.yyyy * _BumpDirection.xyzw) * ((_BumpTiling.x - flowColor.x * dif1),(_BumpTiling.y - flowColor.y * dif1),(_BumpTiling.z - flowColor.x * dif1),(_BumpTiling.w - flowColor.y * dif1));
				//i.bumpCoords2.xyzw = (i.posWorld.xzxz + _Time.yyyy * _BumpDirection.xyzw) * ((_BumpTiling.x - flowColor.x * dif2),(_BumpTiling.y - flowColor.y * dif2),(_BumpTiling.z - flowColor.x * dif2),(_BumpTiling.w - flowColor.y * dif2));
				half2 kk = flowColor * dif1;
				half2 kk2 = flowColor * dif2;
				//half4 kk2 = ((i.bumpCoords.x - flowColor.x * dif2),(i.bumpCoords.y - flowColor.y * dif2),(i.bumpCoords.z - flowColor.x * dif2),(i.bumpCoords.w - flowColor.y * dif2));
				half3 q;
				//q = lerp(kk, kk2, lerpVal);
				half3 worldNormala = (PerPixelNormal(_BumpTex, i.bumpCoords-kk, _BumpStrength));
				half3 worldNormalb = (PerPixelNormal(_BumpTex, i.bumpCoords-kk2, _BumpStrength));
				half3 worldNormalc = normalize(lerp(worldNormala, worldNormalb, lerpVal));
				half3 worldNormald = (PerPixelNormal(_BumpTex, i.bumpCoords2-kk, _BumpStrength));
				half3 worldNormale = (PerPixelNormal(_BumpTex, i.bumpCoords2-kk2, _BumpStrength));
				half3 worldNormalf = normalize(lerp(worldNormald, worldNormale, lerpVal));
				half3 worldNormal = normalize((worldNormalc+worldNormalf)/2);
				//half3 worldNormal = normalize(PerPixelNormal(_BumpTex, i.bumpCoords, _BumpStrength));
				half3 viewVector = normalize(i.viewVector);
				half3 halfVector = normalize((normalize(_SunDir.xyz)-viewVector));

				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate ((sceneZ-partZ)*_FoamRange);

				half2 offsets = worldNormal.xz*viewVector.y;
				//half4 refractColor = tex2D(_RefractTex, offsets*_Params.y)*_Color;

				//
				half3 reflUV = reflect( viewVector, worldNormal);
				half3 reflectColor = texCUBE(_Skybox, reflUV);
				//
				half2 fresnelUV = half2( saturate(dot(-viewVector, worldNormal)), 0.5);
				half fresnel = tex2D(_FresnelTex, fresnelUV).r;

				i.normalDir = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float sceneZ2 = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ2 = max(0,i.projPos.z - _ProjectionParams.g);
                float3 emissive = lerp(float4(0,0,0,0),_DepthColor,(1.0 - saturate((saturate((sceneZ2-partZ2)/_DepthRange)/0.5*dot(viewDirection,i.normalDir)+0.5))));
                //half4 watercolor = (emissive,1);

				half2 uvoff = i.FoamTex;
				uvoff.x += _Time.yx * _RollFoamU;
				uvoff.y += _Time.yx * _RollFoamV;
				half foam = tex2D (_FoamTex,uvoff).r;
				half normask = tex2D (_NormalMask,TRANSFORM_TEX(refuv, _NormalMask)).r;
				//half foam = tex2D (_FoamTex, i.uv+offsets*_Params.y).r;
				//
				if(IsGammaSpace())
				{
					fresnel = pow(fresnel, 2.2);
				}
				//fresnel = FastFresnel(-viewVector, worldNormal, 0.02);

				result.xyz = lerp(refractColor.xyz, reflectColor.xyz, fresnel);
				//fade = pow(clamp(-fade,0.0,1.0),10.2);
				fade =(-fade+1)*foam;
				half3 specularColor = _Specular.w*pow(max(0, dot(worldNormal, halfVector)), _Params.x);
				result.xyz += _Specular.xyz*specularColor;
				result = half4(emissive,1)+result;
				result = lerp(result,1,pow(fade,1.2)*1.5);
				allresult = lerp(refractColor,result,normask);
				return allresult;
			}
			ENDCG
		}
	}
//	FallBack "Diffuse"
}
