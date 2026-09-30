Shader "EchoesOfTheRift/CharacterPalette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Skin ("Skin", Color) = (1,0.8,0.6,1)
        _Hair ("Hair", Color) = (0.2,0.1,0.1,1)
        _Cloth ("Cloth", Color) = (0.2,0.5,0.5,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            fixed4 _Skin, _Hair, _Cloth;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                if(c.r>0.95 && c.g<0.05 && c.b<0.05) c.rgb=_Skin.rgb;
                else if(c.g>0.95 && c.r<0.05 && c.b<0.05) c.rgb=_Hair.rgb;
                else if(c.b>0.95 && c.r<0.05 && c.g<0.05) c.rgb=_Cloth.rgb;
                return c*i.color;
            }
            ENDCG
        }
    }
}
