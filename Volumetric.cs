using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public enum DownSample
{
    Full,
    Half,
    Quarter
}


public class Volumetric : MonoBehaviour
{
  
    [FormerlySerializedAs("a")] [SerializeField] private Material VolShader;
    [SerializeField] private DownSample assighn;
     public static DownSample Downsamp;
    [SerializeField ,Range(-1,1)] private float _Albedo;
    [SerializeField ,Range(0.0001f,20)] private float _Scale=1;
    [SerializeField ] private float _Phi;
    [SerializeField, Range(1,1000) ] private int _samples =20 ;
    [SerializeField ] private Color _Fcol = Vector4.one ;
    [SerializeField ] private float GaussianStandDev = 1.5f ;
    
    //This controls attenuation on spot light
    [SerializeField ] private float consta = 1.5f ;
    [SerializeField ] private float lin = 1.5f ;
    [SerializeField ] private float quad = 1.5f ;
    [SerializeField ] private GameObject tester ;
  
  
    [FormerlySerializedAs("te")] [SerializeField ] private bool Dither = false ;
    [SerializeField] private bool _Point = false;
    [SerializeField]private Light _dePoint;
    [SerializeField] private Texture3D t3d;
   
    
    //make own noise compute shader for noise 3d texture.
    
    private float theta;
    
    
    private void OnEnable()
    {
        print(VolShader.passCount);
        Downsamp = assighn;
    }

    private void Start()
    {
       

        theta = Mathf.Cos((_dePoint.spotAngle / 2) * Mathf.Deg2Rad);
       
    }

    private void Update()
    {
       // this.c.Dispatch(0, 512/8,512/8,1);
        // float3 d = viewDir;
        // float3 ca = PLightPos;
        // float3 v = PLightDir;
        // float3 co = o-ca;
        //
        // float cost =  (Theta);
        // float dotdv = dot(d, v);
        // float dotcov = dot(co, v);
        //
        //    
        //       
        //
        // float a = (dotdv * dotdv) - cost * cost;
        // float b = 2 * (dotdv * dotcov - dot(d, co * (cost * cost)));
        // float c = (dotcov * dotcov) - dot(co, co * (cost * cost));
        //
        // float determ = (b * b) - 4 * a * c;
        //
        // float3 endP;
        // float3 startP;
        // float posi = ((-b + sqrt(determ)) / (2 * a));
        // float negi = ((-b - sqrt(determ)) / (2 * a));
        // float wo = length(world - o);  
        Vector3 o = this.transform.position;
        Vector3 d = this.transform.forward;
        Vector3 ca = _dePoint.transform.position;
        Vector3 v = _dePoint.transform.forward;

        Vector3 co = o-ca;
        
        float cost =  (theta);
        float dotdv = Vector3.Dot(d, v);
        float dotcov = Vector3.Dot(co, v);
        
        float a = (dotdv * dotdv) - cost * cost;
        float b = 2 * (dotdv * dotcov - Vector3.Dot(d, co * (cost * cost)));
        float c = (dotcov * dotcov) - Vector3.Dot(co, co * (cost * cost));  

        float determ = (b * b) - 4 * a * c;
        
        Vector3 endP;
        Vector3 startP;
        float posi = ((-b + Mathf.Sqrt(determ)) / (2 * a));
        float negi = ((-b - Mathf.Sqrt(determ)) / (2 * a));

        if (determ < 0)
        {
//            print("No intersect");
            
        }
        else if (determ ==0)
        {
         //   print("One");
        }
        else
        {
            float t = Mathf.Max(posi, negi);
            float t1 = Mathf.Min(posi, negi);
            Vector3 s = o+t1*d;
            Vector3 e = o+t*d;
            Debug.DrawLine(o, e, Color.red);
            Debug.DrawLine(o, s, Color.green);
            

        }


    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Vector4 pos = _dePoint.transform.position;
        Downsamp = assighn;
        //float theta = (_dePoint.spotAngle/2)*Mathf.Deg2Rad;
        //Mathf.Cos(theta) do here to update angle live
        VolShader.SetFloat("_Albedo", _Albedo);
        VolShader.SetFloat("_Phi", _Phi);
        VolShader.SetFloat("_Scale", _Scale);
        VolShader.SetInt("_samples", _samples);
        VolShader.SetColor("_Fcol", _Fcol );
        Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
        VolShader.SetMatrix("InverseView", Camera.main.cameraToWorldMatrix);
        VolShader.SetMatrix("InverseProj", projectionMatrix.inverse);
        
      

        RenderTexture resolu;
        RenderTexture resolud;
        RenderTexture resolua;
        if (Downsamp == DownSample.Full)
        {
             resolu = RenderTexture.GetTemporary(Screen.width, Screen.height);
            
             resolud = RenderTexture.GetTemporary(Screen.width, Screen.height);
             resolua = RenderTexture.GetTemporary(Screen.width, Screen.height);
            
             
        }
        else if (Downsamp == DownSample.Half)
        {
            resolu = RenderTexture.GetTemporary(Screen.width/2, Screen.height/2);
            resolud = RenderTexture.GetTemporary(Screen.width/2, Screen.height/2);
            resolua = RenderTexture.GetTemporary(Screen.width/2, Screen.height/2);
            
        }
        else
        {
            resolu = RenderTexture.GetTemporary(Screen.width/4, Screen.height/4);
           resolud = RenderTexture.GetTemporary(Screen.width/4, Screen.height/4);
           resolua = RenderTexture.GetTemporary(Screen.width/4, Screen.height/4);
           
        }
        resolu.filterMode = FilterMode.Point;
        resolua.filterMode = FilterMode.Point;
        resolud.filterMode = FilterMode.Point;
        source.filterMode = FilterMode.Point;
       VolShader.SetInt("Dither" ,Convert.ToInt16(Dither) );
       VolShader.SetFloat("_sd",GaussianStandDev);
       
           //scale tec3d maybe have 3 sets of 3d texture and swap out
       Graphics.Blit(source,resolud, VolShader,3);
       VolShader.SetTexture("QuarterD" ,resolud );
       
       
       
    
     

       
       
       
  

     
        
     Graphics.Blit(source,resolua);
     if (!_Point)
     {
         VolShader.SetTexture("noise", t3d);

         Graphics.Blit(resolua,resolu,VolShader,0);
     }
     else
     {
          
         //Manual
            Vector3 w = Vector3.Normalize((_dePoint.transform.forward+_dePoint.transform.position) - _dePoint.transform.position);
            Vector3 u = Vector3.Normalize( Vector3.Cross(w,_dePoint.transform.up));
            Vector3 v = Vector3.Cross(w,u);
            Vector4 col1 = new Vector4(u.x, v.x, w.x,0 );
            Vector4 col2 = new Vector4(u.y, v.y, w.y,0 );
            Vector4 col3 = new Vector4(u.z, v.z, w.z,0) ;
            Vector4 col4 = new Vector4(Vector3.Dot(-_dePoint.transform.position,u),Vector3.Dot(-_dePoint.transform.position,v),Vector3.Dot(-_dePoint.transform.position,w),1);
         Matrix4x4 ma = new Matrix4x4(col1, col2, col3, col4);
         //Matrix4x4 ma = Matrix4x4.TRS(pos, _dePoint.transform.rotation,Vector3.one).inverse;
     
     
         float n = _dePoint.shadowNearPlane;
     
         float f = _dePoint.range;
      
         //Manual
              float ang = 1 / (Mathf.Tan(((_dePoint.spotAngle*(Mathf.PI/180))*0.5f)));
              float q = (f+n) / ( f-n);
              float qn = (2*f*n) / ( f-n);
                   Vector4 ccol1 = new Vector4(ang, 0, 0, 0);
              Vector4 ccol2 = new Vector4(0, ang, 0, 0);
              Vector4 ccol3 = new Vector4(0, 0, q, -1);
              Vector4 ccol4 = new Vector4(0,0,qn,0);
         Matrix4x4 ma1 = new Matrix4x4(ccol1, ccol2, ccol3, ccol4);
     
        // Matrix4x4 ma1 = Matrix4x4.Perspective(_dePoint.spotAngle,1,f,n);
         Matrix4x4 clip = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
      
         Matrix4x4 la = clip * ma1;
         la[0, 2] *= -1;
         la[1, 2] *= -1;
         la[2, 2] *= -1;
         la[3, 2] *= -1;
         Matrix4x4 final = la *ma;
     
     
         VolShader.SetMatrix("_unity_WorldToShadow", final);
         VolShader.SetFloat("consta", consta);
         VolShader.SetFloat("lin", lin);
         VolShader.SetFloat("quad", quad);
         VolShader.SetVector("PLightPos",pos);
         VolShader.SetVector("PLightDir",_dePoint.transform.forward);
         VolShader.SetVector("PTestPos",tester.transform.position);
         VolShader.SetFloat("Theta", theta);
         Graphics.Blit(resolua,resolu,VolShader,7);
        // Graphics.Blit(source,destination,VolShader,7);
     }
     
        
     
     
     
     Graphics.Blit(source,destination);
     Graphics.Blit(resolu,resolua,VolShader,5);
        Graphics.Blit(resolua,resolu,VolShader,6);
        VolShader.SetTexture("Quarter" ,resolu );
        
     
        Graphics.Blit(source,destination,VolShader,4);
        
     
     

    
       RenderTexture.ReleaseTemporary(resolu);
       RenderTexture.ReleaseTemporary(resolud);
       RenderTexture.ReleaseTemporary(resolua);
        
    }
}

