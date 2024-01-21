using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;


[RequireComponent(typeof(Light))]
public class MakeShadowGlobal : MonoBehaviour
{
    CommandBuffer cb = null;
   public RenderTexture ShadowmapCopy;
   
   /*
    * Create a command buffer create a render texture and copy shadow map into new texture to be sampled.
    * For Later Updates I will be lowering the scale when sampling.
    */
       

   void OnEnable()
    {
        
        //From https://shahriyarshahrabi.medium.com/custom-shadow-mapping-in-unity-c42a81e1bbf8
        
        Light Light = this.GetComponent<Light>();
        
        
        
        cb = new CommandBuffer
        {
            name = "CopyLightPass"
        };

        RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;
        if (Volumetric.Downsamp == DownSample.Full)
        {
            
            ShadowmapCopy = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
        }
        else if (Volumetric.Downsamp == DownSample.Half) 
        {
            ShadowmapCopy = new RenderTexture(1024/2, 1024/2, 16, RenderTextureFormat.ARGB32);
        }
        else
        {
            ShadowmapCopy = new RenderTexture(1024/4, 1024/4, 16, RenderTextureFormat.ARGB32);
        }

        
        ShadowmapCopy.filterMode = FilterMode.Point;

      
        cb.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);

      
        var id = new RenderTargetIdentifier(ShadowmapCopy);
        cb.Blit(shadowmap, id);

      
        cb.SetGlobalTexture("_ShadowCascade", id);

        
       
        Light.AddCommandBuffer(LightEvent.AfterShadowMap, cb);

        
      
           
    }
    
    
}
