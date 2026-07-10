Shader "Custom/HandTracking_BuiltIn"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        _ThumbColor ("Thumb Color", Color) = (1,1,1,1)
        _FingerColor_1 ("Finger Color 1", Color) = (1,1,1,1)
        _FingerColor_2 ("Finger Color 2", Color) = (1,1,1,1)
        _FingerColor_3 ("Finger Color 3", Color) = (1,1,1,1)
        _FingerColor_4 ("Finger Color 4", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0,1)) = 0.7
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0

        fixed4 _Color;
        fixed4 _EdgeColor;
        fixed4 _ThumbColor;
        fixed4 _FingerColor_1;
        fixed4 _FingerColor_2;
        fixed4 _FingerColor_3;
        fixed4 _FingerColor_4;
        half _Alpha;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Alpha = _Alpha;
        }
        ENDCG
    }
    
    FallBack "Transparent/Diffuse"
}
