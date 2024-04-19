using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Volumetric))]
public class FogVolumePlacer : Editor
{
    private void OnSceneGUI()
    {

        Volumetric v = target as Volumetric;

        v.SDForigin = Handles.PositionHandle(v.SDForigin, Quaternion.identity);
        v.SDFsize = Handles.ScaleHandle(v.SDFsize, v.SDForigin + Vector3.right, Quaternion.identity);
        Handles.DrawWireCube(v.SDForigin, v.SDFsize);

    }
}
