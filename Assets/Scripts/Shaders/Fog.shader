Shader "Custom/Fog"
{
    Properties
    {
        [Header(Main Texture)]
        [Space]
        _MainTex ("Texture", 2D) = "white" {}                                       //  Visible Texture.
        [Toggle]_Invert("Invert", Float) = 0
        
        _Color("Tint Color", Color) = (1, 1, 1, 0.5)
        _Alpha("Alpha", Range(-1.0, 1)) = 1
        [PowerSlider(1.0)]_IntersectionThresholdMax("Fog Transition", Range(0, 2)) = 0.5            //  Vertical Fog Distance.
        [Space]
        [Toggle]_UseCookie("Use Cookie", Float) = 0
        _Cookie("Cookie", 2D) = "white" {}
        _CookieStrength("Cookie Strength", Range(0, 1)) = 1
        [Space]
        [Header(Transforms)]
        [Toggle]_Rotation("Rotation - (Also rotates the distortion, but speeds are separate)", Float) = 0
        _RotationSpeed("RotationSpeed", Range(-1, 1)) = 0
        _OriginX("Origin X", Range(-2, 2)) = 0.0                                    //  Change the origin of the circle rotating the distortion.
        _OriginY("Origin Y", Range(-2, 2)) = 0.0
        

        [Header(Distortion)]        
        _DistortTex("Texture", 2D) = "white" {}                                     //  Texture used to distort the visible texture.
        [Toggle]_UseMainDistort("Use Main Texture", Float) = 0
        [Toggle]_DistortCookie("Use Cookie", Float) = 0
        _DistortCookieAmount("Amount", Range(0, 1)) = 1
        [Toggle]_MainDistort("Add Main Texture Distortion?", Float) = 0             //  See line 208. Multiplies the Main texture into the distortion, without being rotated.
        _MainDistortAmount("Amount", Range(0, 1)) = 1                               //  The overall effect of this is it moves the bright parts of the image more so can help 
                                                                                    //  tie the distortion into the image if they are significantly different.

        [Toggle]_TestDistortion("Show Distortion Texture (Main texture will not appear, if checked)", Float) = 0                //  Toggled on displays the distortion texture so the rotations are easily visible.
        [Space]
        [PowerSlider(2.0)]_DistortSpeed("Speed", Range(0, 2)) = 0                                   //  Distortion parameters.
        _Magnitude("Magnitude", Range(0, 10)) = 0
        _Period("Period", Range(-3, 3)) = 1
        _Offset("Period Offset", Range(0, 15)) = 0                                  //  Accidental feature, made by messing with the incomplete code that inspired the distortion.
                                                                                    //  I don't fully understand atm but from playing with it, it appears that it
                                                                                    //  offsets the "zero" point of the time variable, so instead of the sine wave going +/-, 
                                                                                    //  the offset can shift it in either direction. This is then modified by the distortion texture(s).
                                                                                    //  Negative looks much the same as positive, so for more range on the slider I just use positive.
                                                                                    //  It creates a standing wave type effect that is a useful option.### MAY BE GOOD IF ANIMATED###
                                                                                    
                                                                                    
        [Header(Distortion Transforms)]
        _DistortRotationSpeed("Rotation Speed", Range(-2, 2)) = 0
        [Space]
                
        [Toggle]_Translate("Translation", Float) = 0                                //  Moves the centre of the distortion around the path of the specified circle.
        //_TranslateSpeed("Translation Speed", Range(-2, 2)) = 0                        //  Angular speed.
        _SpeedX("X Speed", Range(-0.5, 0.5)) = 0

        _SpeedY("Y Speed", Range(-0.5, 0.5)) = 0
        //_Radius("Radius", Range(0, 1)) = 0.1                                      //  Because it's an angular speed, the radius affects the linear speed.

        [Header(Misc)]
        [Toggle]_FogOrder("Apply Fog Before Main Texture?", Float) = 0              //  Applying the fog before the visible texture or after.
        
        
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;              //  Main texture uv.
                float2 uv2 : TEXCOORD1;             //  Distortion texture uv.
                                                    //  Cookie texture will use Main texture uv, so the distortion and rotation is applied to it. It would be a nice feature to be able
            };                                      //  to move the cookie around and/or not distort/rotate but we won't have enough texture coordinates for fog. Extendable?

            struct v2f
            {
                
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 scrPos : TEXCOORD2;          //  Vertical fog will use screen position 
                UNITY_FOG_COORDS(3)                 //  Regular fog will use coordinate 4
                float4 vertex : SV_POSITION;
                
                
            };

            float2 sinusoid(float2 x, float2 m, float2 M, float2 p)     //  2D Sine function that does the distortion
            {
                float2 e = M - m;
                float2 c = 3.1415 * 2.0 / p;
                return e / 2.0 * (1.0 + sin(x * c)) + m;
            }

            

            sampler2D _CameraDepthTexture;
            float4 _Color;
            float _IntersectionThresholdMax;
            float _Magnitude;
            float _Period;
            float _Offset;
            float _DistortRotationSpeed;
            float _Translate;
            float _TestDistortion;
            //float _TranslateSpeed;
            //float _Radius;
            float _FogOrder;
            float _MainDistort;
            float _DistortSpeed;
            float _OriginX;
            float _OriginY;
            float _Invert;
            float _Alpha;
            float _CookieStrength;
            float _UseCookie;
            float _Rotation;
            float _RotationSpeed;
            float _MainDistortAmount;
            float _DistortCookie;
            float _DistortCookieAmount;
            float _SpeedX;
            float _SpeedY;
            float _UseMainDistort;
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _Cookie;
            
                            
            sampler2D _DistortTex;
            float4 _DistortTex_ST;
        
            v2f vert (appdata v)
            {
                
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                if(_Translate == 1.0)                       //  Simple circle translation. ***Expand with linear translation on each axis, oscillation toggle for the option of
                {                                               //  making it a circle and would allow ellipses***

                    //float2 distCoords = float2(sin(_Time[1] * _TranslateSpeed * 2.0), cos(_Time[1] * _TranslateSpeed * 2.0)) * _Radius;
                    float2 scrollCoords = float2(_SpeedX, _SpeedY) * _Time[1];
                    
                    if(_UseMainDistort == 1.0)
                    {
                        _MainTex_ST.zw = scrollCoords;
                    }
                    else
                    {
                        _DistortTex_ST.zw = scrollCoords;               //  _ST.zw accesses the offset parameters, z = "X", w = "Y". Values will not change in the inspector.
                    }
                }

                if(_Rotation == 1.0)                    //  The rotation for the main texture.
                {
                    float SinX = sin(_RotationSpeed * _Time[1]);        //  Sine and cosine of the time, modified by RotationSpeed
                    float CosX = cos(_RotationSpeed * _Time[1]);
                

                    float2x2 rotationMatrix = float2x2( CosX, -SinX, SinX, CosX);   //  Rotation matrix. Changing the sign of cosX or sinX gives a cyclic stretch. ***EXPLORE***

                    v.uv.x += (_OriginX + 0.5) * -1.0;          //  Offsets the rotation origin to place it at the centre. (0, 0) is the bottom left corner, (1, 1) is top right,
                    v.uv.y += (_OriginY + 0.5) * -1.0;          //  so (0.5, 0.5) is the centre. Not sure why it need to be negative here, but it does.

                    v.uv.xy = mul ( v.uv.xy, rotationMatrix );  //  Applies the rotation to the uv
                }
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                if(_Rotation == 1.0)                //  The main rotation for the distortion. Same as above but must be done on different uv coords
                {                                                           //  so that we can create a rotation relative to the main texture.
                    float sinX = sin(_DistortRotationSpeed * _Time[1]);     
                    float cosX = cos(_DistortRotationSpeed * _Time[1]);
                

                    float2x2 rotationMatrix = float2x2( cosX, -sinX, sinX, cosX);   

                    v.uv2.x += (_OriginX + 0.5) * -1.0;
                    v.uv2.y += (_OriginY + 0.5) * -1.0;

                    v.uv2.xy = mul ( v.uv2.xy, rotationMatrix );
                }
                
                if(_UseMainDistort == 1.0)
                {
                    o.uv2 = TRANSFORM_TEX(v.uv2, _MainTex);     //  Applies the texture to the rotated uv
                }
                else
                {
                    o.uv2 = TRANSFORM_TEX(v.uv2, _DistortTex);
                }

                o.scrPos = ComputeScreenPos(o.vertex);          //  Compute the screen position for scrPos uv
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                if(_Rotation == 1.0)
                {
                    i.uv.x += 0.5;      //  Re-centres the images uv's for rotating the textures around the centre of the quad.
                    i.uv.y += 0.5;      //  Done with one toggle because I always want them to rotate together if the rotation is centred. 
                    i.uv2.x += 0.5;
                    i.uv2.y += 0.5;
                }
                fixed4 distort;
                
                if(_UseMainDistort == 1.0)
                {
                    distort = tex2D(_MainTex, i.uv2);       //  The distortion texture applied to the rotated uv
                }
                else
                {
                    distort = tex2D(_DistortTex, i.uv2);
                }

                if(_DistortCookie == 1.0)
                {
                    fixed4 DistortCookie = tex2D(_Cookie, i.uv2);
                    distort *= saturate(DistortCookie + (1.0 - _DistortCookieAmount));
                }
                fixed4 MainTex;         //  Unassigned because it may have to go through uv2 before ending on uv
                            
                
                float time1 = sin(_Time[1] * 5.0 * _DistortSpeed) + _Offset;    //  Speed control scaled into a useable range to modify time.
                float time2 = cos(_Time[1] * 5.0 * _DistortSpeed) + _Offset;    //  For _Offset, see line 25.
                float MainX;
                float MainY;

                if(_MainDistort == 1.0)         //  If the toggle is checked, the Main texture's value at (x, y) is assigned for use in the sinusoid function.
                {
                    MainTex = tex2D(_MainTex, i.uv2);
                    MainX = MainTex.x * _MainDistortAmount;
                    MainY = MainTex.y * _MainDistortAmount;
                }
                else
                {
                    MainX = 0.0;
                    MainY = 0.0;
                }


                float2 Displacement = sinusoid
                (
                    float2(time1 * (distort.x + MainX), time2 * (distort.y + MainY)),   //  Distortion (& optional Main) texture's x and y simply added to the time parameter 
                    float2(-_Magnitude * 0.001, -_Magnitude * 0.001),                   //  of the sinusoid function. The 0.001 puts _Magnitude slider in a more useable 
                    float2(_Magnitude * 0.001, _Magnitude * 0.001),                     //  range. No idea why it scaled so big.
                    float2(_Period, _Period)
                );

                
                i.uv.xy += Displacement;                        //  Sinusoidal displacement added to the main uv's x and y values. 
                MainTex = tex2D(_MainTex, i.uv);                //  Puts the texture on the displaced uv.

                
                if(_Invert == 1.0)                              //  Invert option in inspector, before color tint and cookie
                    {
                        MainTex.r = 1.0 - MainTex.r;
                        MainTex.g = 1.0 - MainTex.g;
                        MainTex.b = 1.0 - MainTex.b;
                    }
                
                if(_UseCookie == 1.0)
                {
                    fixed4 Cookie = tex2D(_Cookie, i.uv); //tex2Dproj(_Cookie, i.scrPos);       //  Use the cookie texture with i.uv, so will distort and rotate with the main texture. ***Toggle for static using i.uv3***
                    
                    MainTex.rgb *= saturate(Cookie + (1.0 - _CookieStrength));  //  In order to make CookieStrength = 0 fully white, I need to add to the black parts. When it's == 1,
                                                                                //  zero is added so the cookie texture is unaffected. Add to every fragment while clamping at 1.0.
                }   

                float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));   //  Accesses the camera's depth texture using screen position coords.
                float diff = saturate(_IntersectionThresholdMax * (depth - i.scrPos.w));                    //  Multiply the threshold by the depth texture (at each screen pixel)
                                                                                                            //  minus the w component. What is that?

                fixed4 col = lerp(fixed4(_Color.rgb, 0.0), _Color, diff * diff * diff * diff * (diff * (6 * diff - 15) + 10));  //  Interpolates between the tint colour and depth expoentially(?)

                if(_FogOrder == 1.0)
                {
                    UNITY_APPLY_FOG(i.fogCoord, col);   //  Toggle : Apply the fog before Main texture
                }

                if(_TestDistortion == 1)                //  Toggle : switches which texture is applied so user can see what the distortion texture is doing quickly and easily  
                {
                    col.rgb *= distort.rgb;
                }
                else
                {                                       //  ***Mod : use the darkness of the main texture to mess further
                                        
                    
                    float lum = Luminance(MainTex.rgb);
                    
                    col.rgb *= MainTex.rgb;                 

                    col.a *= saturate(lum + _Alpha);

                }

                if(_FogOrder == 0.0)
                {
                    UNITY_APPLY_FOG(i.fogCoord, col);   //  Toggle : Apply the fog after MAin texture
                }

                return col;
            }
            
            ENDCG
        }
    }
}