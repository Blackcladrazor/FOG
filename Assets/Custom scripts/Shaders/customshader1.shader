    Shader "shader/customshader1" {
    Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
    _Shininess ("Shininess", Range (0.01, 120)) = 3
    _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    _BumpMap ("Normalmap", 2D) = "bump" {}
    _SpecMap ("Specular map", 2D) = "black" {}
    }
    SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 400
     
    CGPROGRAM
    #pragma surface surf CustomPhong
     
     
    sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _SpecMap;
    float4 _Color;
    float _Shininess;
     
    struct Input {
    float2 uv_MainTex;
    float2 uv_SpecMap;
    };
     
    //custom output struct
    struct SurfaceCustomOutput {
    fixed3 Albedo;
    fixed3 Normal;
    fixed3 Emission;
    fixed3 SpecularColor;
    half Specular;
    fixed Gloss;
    fixed Alpha;
    };
     
    inline half4 LightingCustomPhong (SurfaceCustomOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
    {
    //calculate diffuse and the specular vector
    float diff = dot(s.Normal, lightDir);
    float3 reflectionVector = normalize(2.0 * s.Normal * diff - lightDir);
     
    //calculate the phong specular
    float spec = pow(max(0.0f,dot(reflectionVector, viewDir)), _Shininess) * s.Specular;
    float3 finalSpec = s.SpecularColor * spec * _Specularcolor.rgb
     
    //create final color
    half4 c;
    c.rgb = (s.Albedo * _LightColor0.rgb * diff) + (_LightColor0.rgb * finalSpec);
    c.a = s.Alpha;
    return c;
    }
     
    void surf (Input IN, inout SurfaceCustomOutput o) {
    float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    float4 specMask = tex2D(_SpecMap, IN.uv_SpecularMask) * _SpecColor;
     
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    o.Specular = specMask.r;
    o.SpecularColor = specMask.rgb;
    }
     
    ENDCG
    }
     
    FallBack "Specular"
    }