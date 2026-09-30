Shader "EchoesOfTheRift/IllustratedHero"
{
 Properties
 {
  [PerRendererData] _MainTex ("Sprite",2D)="white" {}
  _Skin ("Skin",Color)=(1,.8,.6,1)
  _Hair ("Hair",Color)=(1,.35,.08,1)
  _Eyes ("Eyes",Color)=(.1,.8,1,1)
  _Customize ("Customize",Float)=0
  _Race ("Race",Float)=0
 }
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False"}
  Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   sampler2D _MainTex;float4 _MainTex_TexelSize;fixed4 _Skin,_Hair,_Eyes;float _Customize,_Race;
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target
   {
    float2 grid=max(1,floor(_MainTex_TexelSize.zw*.95));
    fixed4 c=tex2D(_MainTex,(floor(i.uv*grid)+.5)/grid);
    float2 local=frac(i.uv*3);
    if(_Customize>.5&&local.y>.4)
    {
     bool hair=c.r>c.g*1.25&&c.g>c.b*1.7&&c.r>.2;
     bool skin=(c.r>c.g&&c.g>c.b&&c.b>c.g*.48)||((_Race==3||_Race==4)&&c.g>c.r*.8&&c.g>c.b*1.4)||(_Race==7&&c.r>c.g*1.4&&c.b>c.g*.65);
     if(hair)c.rgb=_Hair.rgb*(.3+max(c.r,max(c.g,c.b))*.85);
     else if(skin)c.rgb=_Skin.rgb*(.25+max(c.r,max(c.g,c.b))*.8);
     else if(local.y>.5&&local.y<.7&&c.b>.7&&c.g>.65&&c.r<.55)c.rgb=_Eyes.rgb;
    }
    return c*i.color;
   }
   ENDCG
  }
 }
}
