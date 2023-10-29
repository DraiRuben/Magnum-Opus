Shader"LightUpEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlapColor ("Overlap Color",Color) = (1,1,1,1)
        _BaseColor ("Base Color",Color) = (1,1,1,1)
    }
    SubShader
    {

        Tags { "RenderType"="Transparent" }


        LOD 100
        Pass
        {

            Stencil {
                Ref 0
                Comp Equal
                Pass IncrSat
                Fail IncrSat 
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            float4 _BaseColor;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.texcoord); 
                fixed4 col = fixed4(texColor.rgb * _BaseColor.rgb, texColor.a);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Stencil {
                Ref 1
                Comp Less
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            float4 _OverlapColor;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.texcoord);

                fixed4 col = fixed4(texColor.rgb * _OverlapColor.rgb, texColor.a);
                return col;
            }
            ENDCG
        }
    }
}