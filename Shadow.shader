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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
      

            #include "UnityCG.cginc"
               
          
            #include "AutoLight.cginc"
         
           #define TAU 6.28318
           #define PI 3.14159
          
            
         

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
            
            sampler2D _MainTex , _CameraDepthTexture,_ShadowCascade;
            float4 _MainTex_ST,  _Fcol;
            float4x4 InverseView;
            float4x4 InverseProj;
            int _samples;
            float _Albedo, _Phi;
            
            

            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
              
                o.ray =  v.Ray ;
             
                return o;
            }


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
                fixed4 col = tex2D(_MainTex, i.uv);

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
              
           
             [loop]
             for(float l =0 ; l <  max; l += s) {
                 //March to new point in world space.
                x += viewDir * s;
                 
                 //Check if this point in world space is in shadow
                float v = inshadow(x);

                 //Add light based off where the light scatter found using look and light direction
                 float Li = Henyey(_Albedo, dot(viewDir, _WorldSpaceLightPos0))*v;
                 
                 //Add color
                 L +=  Li*_Phi*_Fcol;
            }
                L = L/_samples;
             
                 return float4(col+ L,1);
          
            

                
            }
            ENDCG
        }
    }
}
