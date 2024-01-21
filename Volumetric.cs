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
    [SerializeField ] private int _samples =20 ;
    [SerializeField ] private Color _Fcol = Vector4.one ;

    private void OnEnable()
    {
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
        if (Downsamp == DownSample.Full)
        {
             resolu = RenderTexture.GetTemporary(Screen.width, Screen.height);
            
        }
        else if (Downsamp == DownSample.Half)
        {
            resolu = RenderTexture.GetTemporary(Screen.width/2, Screen.height/2);
            
        }
        else
        {
            resolu = RenderTexture.GetTemporary(Screen.width/4, Screen.height/4);
        }



        Graphics.Blit(source,resolu,VolShader,0);
        
        Graphics.Blit(resolu,destination);
        
       Graphics.Blit(source,destination,VolShader,1);
       resolu.Release();
        
    }
}
