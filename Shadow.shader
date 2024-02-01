Shader "Unlit/Shadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
      
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100
    CGINCLUDE



            #include "UnityCG.cginc"
            #define TAU 6.283185307
          
            #include "AutoLight.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 Ray : TEXCOORD1; 
                float3 N : NORMAL; 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 ray : TEXCOORD1;
                float4 vertex : SV_POSITION;
               
               
            };

            SamplerState point_clamp_sampler;
            SamplerState sampler_MainTex;
            Texture2D _MainTex;
            float4 _MainTex_ST;

       

    ENDCG
        
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
      

            
         
           #define TAU 6.28318
           #define PI 3.14159
          
             v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
              
                o.ray =  v.Ray ;
             
                return o;
            }
         
  
            
            sampler2D  _CameraDepthTexture,_ShadowCascade;
            float4 _MainTex_TexelSize;
            float4x4 InverseView;
            float4x4 InverseProj;
            int _samples;
            bool Dither = false;
            float _Albedo, _Phi;
            
            
          static  const float DitherPattern [4][4]= {0.0,0.5,0.125,0.625,
                                    0.75,0.25,0.875,0.375,
                                    0.1875,0.6875,0.0625,0.5625,
                                    0.9375,0.4375,0.8125,0.3125};
            

            //Use world space co-ordinate to find if that position is in shadow
float inshadow(float3 worldpos)
            {

                //Find what cascade the current world position is between
              float zer = length( worldpos - _WorldSpaceCameraPos);
            float4 near = float4 (zer >= _LightSplitsNear); 
             float4 far = float4 (zer < _LightSplitsFar);
             float4 weights = near * far;
                //Similar to a logical or bitwise operation eg
                //Near (1,0,0,0) * (1,1,1,1) = (1,0,0,0) Its in the first cascade.

                //Converts from world to matching shadow map pos based on what cascade its in
                float3 shadowCoord0 = mul(unity_WorldToShadow[0], float4( worldpos.xyz, 1.)).xyz; 
             float3 shadowCoord1 = mul(unity_WorldToShadow[1], float4( worldpos.xyz, 1.)).xyz;
             float3 shadowCoord2 = mul(unity_WorldToShadow[2], float4( worldpos.xyz, 1.)).xyz;
             float3 shadowCoord3 = mul(unity_WorldToShadow[3], float4( worldpos.xyz, 1.)).xyz;
            float3 coord = shadowCoord0 * weights.x + 	shadowCoord1 * weights.y + 	shadowCoord2 * weights.z + shadowCoord3 * weights.w; 
                  float shadowmask = tex2Dlod(_ShadowCascade, float4(coord.xy,0,0)).x;

                //If the position is deeper than whats shown in the shadow map it is beghind it meaning it is in shadow.
            shadowmask = shadowmask < coord.z;
                          

            return shadowmask;



                
            }




        

            //Aproximation of mei scattering based on g - how much will scatter forward
            //Costheta - similarity between view and light
            float Henyey( float g, float costheta)
            {

               float newval =  (1.0-g*g)/(4*PI*pow(1.0+g*g-(2.0*g)*costheta,3/2));
                return newval;
                
            }
            
            
            fixed4 frag (v2f i) : SV_Target
            {
                
                //Get screen Col
                fixed4 col = _MainTex.Sample(sampler_MainTex, i.uv);

                //Get the depth of object from camera to conver to world
                float CamDeep = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv).r;

                //put in range of -1 to 1
             i.ray =  i.ray*2-1;
        
             
             
            //Convert back to View space using screen coord and depth
            float4 view = mul(InverseProj, float4(i.ray.xy,CamDeep,1));
                
                float3 viewr = view.xyz / view.w;

                //Convert to world from view
                float3 world = mul(InverseView, float4(viewr,1));

                //Checks if pixel is in shadow [For Testing]
              float shadowmask = inshadow(world);

                
                //Length of ray to march through from cam to object 
                float max =  length(  _WorldSpaceCameraPos- world );

                //This is probobly not necessary
              //  max = min(max, 999999999);

                //How far to move each march
                float  s = max / _samples;

                
                float3 x =_WorldSpaceCameraPos;

                

                //eye direction from cam to point
              float3  viewDir = normalize( world-_WorldSpaceCameraPos);
                
            float3 L = float4(0,0,0,1);
                
                
                if(Dither)
                {
                    
                    float div = DitherPattern[(i.uv.x*_MainTex_TexelSize.z)%4][(i.uv.y*_MainTex_TexelSize.w)%4];
                    //offset starting position by matching fragment to matrix value;
                     x += (viewDir *( s*div));
                    
                }
                
           
             [loop]
             for(float l =0 ; l <  max; l += s) {
                 //March to new point in world space.
                x += viewDir * s;
                 
                 //Check if this point in world space is in shadow
                float v = inshadow(x);

                 //Add light based off where the light scatter found using look and light direction
                 float Li = Henyey(_Albedo, dot(viewDir, _WorldSpaceLightPos0))*v;
                 
                 //Add color
                 L +=  Li*_Phi;
            }
                L = L/_samples;
             
                 return float4( L,1);
          
            

                
            }
            ENDCG
        }
        
   
        
        Pass{
            
             //Point Sampler without Additive
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
 v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
              
                o.ray =  v.Ray ;
             
                return o;
            }
            fixed4 frag(v2f i ) : SV_Target{

                fixed4 col = _MainTex.Sample(point_clamp_sampler, i.uv);
                return col;
            }
            

            ENDCG
            }
        
        Pass{
            
            //Point Sampler with Additive
        Blend one one
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
             v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
              
                o.ray =  v.Ray ;
             
                return o;
            }
            fixed4 frag(v2f i) : SV_Target{

                fixed4 col = _MainTex.Sample(point_clamp_sampler, i.uv);
                return col;
            }
            

            ENDCG
            }
        
        Pass{
            
             //Depth of camera
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            //Texture2D  _CameraDepthTexture;
             v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
              
                o.ray =  v.Ray ;
             
                return o;
            }
            sampler2D  _CameraDepthTexture;
            fixed4 frag(v2f i) : SV_Target{

               // fixed4 col = _CameraDepthTexture.Sample(point_clamp_sampler,i.uv);
               fixed4 col = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);
                return float4(col.x,0,0,1);
            }
            

            ENDCG
            }
        
        // Simpler Method found here: https://github.com/SlightlyMad/VolumetricLights/blob/master/Assets/Shaders/BilateralBlur.shader
        // This method seems better but I wanted to be more accurate to the article.
        Pass{
            
         Blend one one
          
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

          Texture2D Quarter;
           sampler2D QuarterD;
float4 Quarter_TexelSize;
float4 QuarterD_TexelSize;
float4 _MainTex_TexelSize;
            float   _omegaspace , _omegatonal ;


            struct outvert
            {
 float2 uv : TEXCOORD0;
                float2 ray : TEXCOORD1;
                float4 vertex : SV_POSITION;
            float4  uv1 : TEXCOORD2;
                float4  uv2 : TEXCOORD3;


                
            };



            outvert vert (appdata v)
            {
                outvert o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.uv1 = float4(v.uv  - 0.5 * Quarter_TexelSize.xy,0,0);
                o.uv1.zw = o.uv1.xy + float2(Quarter_TexelSize.x,0);
                o.uv2.xy = o.uv1.xy  + float2(0,Quarter_TexelSize.y);
                o.uv2.zw = o.uv1.xy + Quarter_TexelSize.xy;
                o.ray =  v.Ray ;
             
                return o;
            }
            sampler2D  _CameraDepthTexture;




float4   _Fcol;
           
             
            fixed4 frag(outvert i) : SV_Target{

                float2 uvs[4] ;
                //get lower res uv
                uvs[0] = i.uv1.xy;
                uvs[1]= i.uv1.zw;
                uvs[2]= i.uv2.xy;
                uvs[3]= i.uv2.zw;
                //get higher res depth
                float samp = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv).r;
            
            //used to get bilinear weights
                 float2 ab = i.uv - uvs[0];
                float weight[4] ={1-ab.x*1-ab.y, ab.x*1-ab.y,1-ab.x*ab.y, ab.x*ab.y};
              
                float depthdiff[4] ;
                float cols[4] ;
                float weighttotal = 0;
                float nuweight[4];
                float4 final= float4(0,0,0,1);
                [unroll]
                for ( int i = 0; i<4; i++)
                {   //depth of lower res samples. get diffrence from higher res and make higher the close they are.
                    depthdiff[i] = 1/ (0.00000000000001+(abs( samp-SAMPLE_DEPTH_TEXTURE(QuarterD, uvs[i]).r)));
                    //get lower sample color
                    cols[i] =  Quarter.Sample(point_clamp_sampler, uvs[i]);
                    
                    nuweight[i] = depthdiff[i] * weight[i];
                    weighttotal += nuweight[i];
                    
                    
                }
                [unroll]
                for (int j=0; j<4; j++)
                {
                    //normalize weight
                    nuweight[j] /=  weighttotal;
                    //scale by weights
                    final += cols[j]*nuweight[j];
                    
                    
                }
               
                
return final*_Fcol;


                
                
               
                
            }
            

            ENDCG
            }
    }
    }
    
    
    
    
    

