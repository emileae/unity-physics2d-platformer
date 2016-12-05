Shader "Custom/Emile/MyFirstShader" {

	Properties{
		_MainTexture("Main Color (RGB) yo", 2D) = "white" {}
		_Color("Colour", Color) = (1,1,1,1)
	}

	SubShader{
		// each pass is a draw call
		Pass{
			// CG shader language
			CGPROGRAM

			// define vertex and fragment functions
			#pragma vertex vertexFn
			#pragma fragment fragmentFn

			// this pulls in the unity helper functions
			#include "UnityCG.cginc"

			// can get objects vertices, normals, colour, UVs etc.
			// float4 = 1,1,1,1, float2 = 1,1 etc.
			struct appdata {
				float4 vertexPos : POSITION;
				float2 uvPos : TEXCOORD0;
			};


			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _Color;// pull in the variabled form higher up in the file
			sampler2D _MainTexture; // sample the 2d texture



			// build the object
			v2f vertexFn(appdata IN){
				v2f OUT;

				// nvidia shader docs mul = multiply
				// UNITY_MATRIX_MVP - model view projection, so position vertex according to model, camera, perspective etc.
				// here the IN.uvPos and IN.vertexPos comes from the appdata struct comes form the v2f struct
				OUT.position = mul(UNITY_MATRIX_MVP, IN.vertexPos);
				OUT.uv = IN.uvPos;

				return OUT;
			}

			// now color the pixels in...
			// need to include sv_target just needs to be included
			fixed4 fragmentFn(v2f IN) : SV_Target{

				// here the IN.uv comes form the v2f struct
				float4 textureColor = tex2D(_MainTexture, IN.uv);// pass in texture adn uv and draw to screen

				return textureColor * _Color;// multiply texture color by texture color
			}

			// Vertex function --> builds the object
			// Fragment function --> Colour the object in...? lighting here?

			ENDCG
		}
	}

}