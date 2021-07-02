using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class JsonClass
{
    public GameObject[] myObject;
    public int roomTypeId;
    public int buildingId;
    public string dong;
    public float height;
}

public class MyFramework : MonoBehaviour
{
    public List<JsonClass> jsonClass;
    public bool success;
    public int code;
    public JsonManager JsonScript;
    public Texture2D Texture_local;

    void Start()
    {
        //텍스처를 미리 로딩해준다.
        if (File.Exists(Application.dataPath + "/Samples/texture/buildingTester_d.png"))
        {
            byte[] byteTex = File.ReadAllBytes(Application.dataPath + "/Samples/texture/buildingTester_d.png");
            if (byteTex.Length > 0)
            {
                Texture_local = new Texture2D(0, 0);
                Texture_local.LoadImage(byteTex);
            }
        }
        JsonScript.LoadJson(jsonClass);
    }

    private void OnDestroy()
    {
        int totalCount = jsonClass.Count;
        for (int i = (totalCount - 1); i >= 0; i--)
        {
            int ObjCount = jsonClass[0].myObject.Length;
            for (int j = (ObjCount - 1); j >= 0; j--)
                Destroy(jsonClass[i].myObject[j]);
            jsonClass.RemoveAt(i);
        }
    }


}
