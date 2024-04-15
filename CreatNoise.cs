using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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
        
        //dosnt work has to be converted to Texture 3D but you can feed r into a shader directly
        AssetDatabase.CreateAsset(r, "Assets/FogVolume.asset");
        
    }

    // Update is called once per frame
    void Update()
    {
        
       
     //   pl.transform.GetComponent<Renderer>().material.mainTexture = r;
        
        
      
        
        
        
        
        
        
    }
}
