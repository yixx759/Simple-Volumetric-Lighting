using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField ] private float _Phi;
    [SerializeField, Range(1,1000) ] private int _samples =20 ;
    [SerializeField ] private Color _Fcol = Vector4.one ;
    [SerializeField ] private float GaussianStandDev = 1.5f ;
  
    [FormerlySerializedAs("te")] [SerializeField ] private bool Dither = false ;
  
    
    
    
    
    private void OnEnable()
    {
        print(VolShader.passCount);
        Downsamp = assighn;
    }



    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Downsamp = assighn;
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
        Graphics.Blit(resolua,resolu,VolShader,0);
        


   
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
