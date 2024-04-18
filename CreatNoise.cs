using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor;

public class CreatNoise : MonoBehaviour
{
    
    [SerializeField] private ComputeShader c;
    [SerializeField]private RenderTexture r;
    [SerializeField] private GameObject pl;
    [SerializeField] float persistance = 0.5f;
    [SerializeField] float amp = 1;
    [SerializeField] float freq = 8;
    [SerializeField] float freqchange = 2;
    [SerializeField] float seed = 2;
    [SerializeField] int octaves = 4;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private int n =6;

    struct fourbytes
    {
        private Byte one;
        private Byte two;
        private Byte three;
        private Byte four;
        

    }
    
    //https://forum.unity.com/threads/save-a-3d-render-texture-to-file.1204267/
    void SaveRT3DToTexture3DAsset(RenderTexture rt3D, string pathWithoutAssetsAndExtension)
    {
        int width = rt3D.width, height = rt3D.height, depth = rt3D.volumeDepth;
        var a = new NativeArray<fourbytes>(width * height * depth, Allocator.Persistent, NativeArrayOptions.UninitializedMemory); //change if format is not 8 bits (i was using R8_UNorm) (create a struct with 4 bytes etc)
        AsyncGPUReadback.RequestIntoNativeArray(ref a, rt3D, 0, (_) =>
        {
           // Texture3D output = new Texture3D(width, height, depth, rt3D.graphicsFormat, false);
           Texture3D output = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
            output.SetPixelData(a, 0);
            output.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            AssetDatabase.CreateAsset(output, $"Assets/{pathWithoutAssetsAndExtension}.asset");
            AssetDatabase.SaveAssetIfDirty(output);
            a.Dispose();
            rt3D.Release();
        });
    }
    // Start is called before the first frame update
    void Start()
    {
       
        

        
        
        //set var



      //  r = new Texture3D(1920, 1080, 16, TextureFormat.RGBA32, false);
       // r.wrapMode = TextureWrapMode.Repeat;
       r = new RenderTexture(1920, 1080, 0);
       r.enableRandomWrite = true;
       r.dimension = TextureDimension.Tex3D;
       r.wrapMode = TextureWrapMode.Repeat;
       r.volumeDepth = 16;
       r.Create();
        c.SetTexture(0,"Result",r);
        c.SetFloat("persistance", persistance);
        c.SetFloat("amp", amp);
        c.SetFloat("freq", freq);
        c.SetFloat("freqchange", freqchange);
        c.SetFloat("seed", seed);
        c.SetInt("octaves", octaves);
        c.SetVector("offset", offset);
        
        c.Dispatch(0, 1920/8,1080/8,1);

        SaveRT3DToTexture3DAsset(r, "FogVolume");
      //  AssetDatabase.CreateAsset(r, "Assets/FogVolume.asset");
        
    }

    // Update is called once per frame
    void Update()
    {
        
       
     //   pl.transform.GetComponent<Renderer>().material.mainTexture = r;
        
        
      
        
        
        
        
        
        
    }
}
