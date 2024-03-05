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

[ExecuteInEditMode]
public class Volumetric : MonoBehaviour
{
  
    [FormerlySerializedAs("a")] [SerializeField] private Material VolShader;
    [SerializeField] private DownSample assighn;
     public static DownSample Downsamp;
    [SerializeField ,Range(-1,1)] private float _Albedo;
    [SerializeField ] private float _Phi;
    [SerializeField, Range(1,1000) ] private int _samples =20 ;
    [SerializeField ] private Color _Fcol = Vector4.one ;
    [SerializeField ] private float GaussianStandDev = 1.5f ;
    
    //This controls attenuation on spot light
    [SerializeField ] private float consta = 1.5f ;
    [SerializeField ] private float lin = 1.5f ;
    [SerializeField ] private float quad = 1.5f ;
  
  
    [FormerlySerializedAs("te")] [SerializeField ] private bool Dither = false ;
    [SerializeField] private bool _Point = false;
    [SerializeField]private Light _dePoint;
    
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


   

    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Vector4 pos = _dePoint.transform.position;
        Downsamp = assighn;
        //float theta = (_dePoint.spotAngle/2)*Mathf.Deg2Rad;
        //Mathf.Cos(theta) do here to update angle live
        VolShader.SetFloat("_Albedo", _Albedo);
        VolShader.SetFloat("_Phi", _Phi);
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
       
      
       Graphics.Blit(source,resolud, VolShader,3);
       VolShader.SetTexture("QuarterD" ,resolud );
       
       
       
    
     

       
       
       
  

     
        
     Graphics.Blit(source,resolua);
     if (!_Point)
     {
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
         //This one seems to be less acurate
        // Matrix4x4 ma = Matrix4x4.TRS(pos, _dePoint.transform.rotation,Vector3.one).inverse;
     
     
         float n = _dePoint.shadowNearPlane;
     
         float f = _dePoint.range;
      
         //Manual
         //      float ang = 1 / (Mathf.Tan(((_dePoint.spotAngle*(Mathf.PI/180))*0.5f)));
         //      float q = (f+n) / ( f-n);
         //      float qn = (2*f*n) / ( f-n);
         //           Vector4 ccol1 = new Vector4(ang, 0, 0, 0);
         //      Vector4 ccol2 = new Vector4(0, ang, 0, 0);
         //      Vector4 ccol3 = new Vector4(0, 0, q, -1);
         //      Vector4 ccol4 = new Vector4(0,0,qn,0);
         // Matrix4x4 ma1 = new Matrix4x4(ccol1, ccol2, ccol3, ccol4);
     
         Matrix4x4 ma1 = Matrix4x4.Perspective(_dePoint.spotAngle,1,f,n);
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
         VolShader.SetFloat("Theta", theta);
         Graphics.Blit(resolua,resolu,VolShader,7);
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

