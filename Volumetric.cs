using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volumetric : MonoBehaviour
{
  
    [SerializeField] private Material a;
    [SerializeField ,Range(-1,1)] private float _Albedo;
    [SerializeField ] private float _Phi;
    [SerializeField ] private int _samples =20 ;
    [SerializeField ] private Color _Fcol = Vector4.one ;
 
  
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        a.SetFloat("_Albedo", _Albedo);
        a.SetFloat("_Phi", _Phi);
        a.SetInt("_samples", _samples);
        a.SetColor("_Fcol", _Fcol );
        
        
        
        
        
        Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
        a.SetMatrix("InverseView", Camera.main.cameraToWorldMatrix);
        a.SetMatrix("InverseProj", projectionMatrix.inverse);
        Graphics.Blit(source,destination,a);
        
    }
}
