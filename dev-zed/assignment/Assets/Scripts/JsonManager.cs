using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class JsonManager : MonoBehaviour
{
    public MyFramework mainframe;
    int count1 = 0;

    //구조체 리셋함수.
    private void ResetClass(JsonClass mydata)
    {
        mydata.roomTypeId = 0;
        mydata.buildingId = 0;
        mydata.dong = "";
        mydata.height = 0.0f;
    }

    //JSON 파일을 로딩.
    public void LoadJson(List<JsonClass> jsonClass)
    {
        int totnum = 0;
        JsonClass mydata = new JsonClass();
        ResetClass(mydata);
        char[] delmiter = { ':', '"', '[', ']', '{', '}', '\n', '\r', ',' };
        string jsonstr;
        if (File.Exists(Application.dataPath + "/Samples/json/dong.json"))
            jsonstr = File.ReadAllText(Application.dataPath + "/Samples/json/dong.json");
        else
            return;
        string[] deleteLine = { "data", "roomtypes", "meta" }, words = jsonstr.Split(delmiter), RealData;

        //words 에 Json에서 읽어온 데이터중 필요없는 부분을 제거하고 실제 데이터와 데이터 name만 남긴다.
        //words 안에 있는 데이터의 총갯수를 구한다.
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                totnum++;
        }
        RealData = new string[totnum];
        totnum = 0;
        for (int i = 0; i < words.Length; i++)
        {
            bool selectbool = false;
            if (words[i].Length > 0)
            {
                for (int j = 0; j < deleteLine.Length; j++)
                {
                    //값을가지지 않는 데이터의 이름들을 제거한다. (값을 가지고 있는 실제 데이터의 이름과 데이터만 남게된다.)
                    if (deleteLine[j] == words[i])
                    {
                        selectbool = true;
                        break;
                    }
                }

                if (selectbool == false)
                {
                    //words 에서 실제 사용하는 데이터만 RealData 에 모아놓는다.
                    RealData[totnum] = words[i];
                    totnum++;
                }
            }
        }

        //RealData 에 있는 데이터를 저장하거나 처리해준다.
        for (int i = 0; i < RealData.Length; i++)
        {
            switch (RealData[i])
            {
                case "coordinatesBase64s":
                    {
                        int ai = 1;
                        while (true)
                        {
                            if (RealData[i + ai] == "룸타입id")
                                break;
                            ai++;
                        }
                        mydata.myObject = new GameObject[(ai - 1)];

                        //Base64 로 되어있는 데이터를 일반데이터로 변환하고 오브젝트를 만들어주고 만들어진 오브젝트를 리턴해준다.
                        for (int j = 0; j < (ai - 1); j++)
                            mydata.myObject[j] = ConvertBase64(RealData[i + (j + 1)]);
                        i += (ai - 1);
                    }
                    break;
                case "룸타입id":
                    i++;
                    mydata.roomTypeId = int.Parse(RealData[i]);
                    if (RealData[i + 1] == "coordinatesBase64s")
                    {
                        //하나의 데이터가 끝났다면 list에 저장해주고 임시구조체를 다시 리셋해준다.
                        jsonClass.Add(mydata);
                        mydata = new JsonClass();
                        ResetClass(mydata);
                    }
                    break;
                case "bd_id":
                    i++;
                    mydata.buildingId = int.Parse(RealData[i]);
                    break;
                case "동":
                    i++;
                    mydata.dong = RealData[i];
                    break;
                case "지면높이":
                    i++;
                    mydata.height = float.Parse(RealData[i]);
                    jsonClass.Add(mydata);
                    mydata = new JsonClass();
                    ResetClass(mydata);
                    break;
                case "success":
                    i++;
                    if (RealData[i] == "false")
                        mainframe.success = false;
                    break;
                case "code":
                    i++;
                    mainframe.code = int.Parse(RealData[i]);
                    break;
            }
        }
    }

    private GameObject ConvertBase64(string data)
    {
        int totalCount;
        List<Vector3> SpotPoint = new List<Vector3>();

        //Base64 데이터를 byte 데이터로 변환해준다
        System.Text.Encoding encoding = new System.Text.UTF8Encoding();
        byte[] bytes = System.Convert.FromBase64String(data);

        //byte 데이터를 Vector3 데이터로 바꿔주고 list에 모아놓는다.(정점정보)
        for (int i = 0; i < bytes.Length; i += 12)
        {
            Vector3 dummy = new Vector3(BitConverter.ToSingle(bytes, i), BitConverter.ToSingle(bytes, i + 8), BitConverter.ToSingle(bytes, i + 4));
            SpotPoint.Add(dummy);
        }

        int[] vertexIndex = new int[SpotPoint.Count];
        totalCount = SpotPoint.Count;

        //정점을 찍어줄때 필요한 index버퍼에 값을 채워준다.
        for (int i = 0; i < SpotPoint.Count; i++)
            vertexIndex[i] = i;

        //오브젝트의 정점정보돠 정점인덱스정보를 줘서 오브젝트를 생성한다.
        GameObject InstanceData = MakeObject(SpotPoint, vertexIndex);

        //임시저장소 제거
        for (int i = (totalCount - 1); i >= 0; i--)
            SpotPoint.RemoveAt(i);

        return InstanceData;
    }

    private GameObject MakeObject(List<Vector3> vertiecs, int[] IndexData)
    {
        float uvx2 = 0.5f, uvx3 = 0.75f, uvx5 = 1.0f, uvy2 = 0.0f, uby3 = 1.0f, Height = 0.0f, MinHeight = 0.0f, MaxHeight = 0.0f, uvx0 = 150.0f / mainframe.Texture_local.width,
            uvx1 = 245.0f / mainframe.Texture_local.width, uvy0 = 50.0f / mainframe.Texture_local.height, uvy1 = 232.0f / mainframe.Texture_local.height;
        bool cmp = false;
        int TexKind = 0, uvsCount = 0, num = 0;

        //임시저장소 할당.
        Vector3[] dummy = new Vector3[vertiecs.Count], normals = new Vector3[vertiecs.Count / 3];
        Vector2[] uvs = new Vector2[vertiecs.Count];
        float[] dirAngle = new float[vertiecs.Count / 3];

        //오브젝트의 이름을 넣어주고 mesh를 생성해주고 meshfilter,meshrenderar, material을 추가해준다.
        GameObject go = new GameObject("Mesh" + count1++);
        Mesh mesh_Local = new Mesh();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        Material mt = new Material(Shader.Find("Unlit/Texture"));

        Vector2[] uvdummy1 = { new Vector2(uvx1, uvy0), new Vector2(uvx0, uvy0), new Vector2(uvx0, uvy1), new Vector2(uvx1, uvy1), new Vector2(uvx1, uvy0), new Vector2(uvx0, uvy1) };
        Vector2[] uvdummy2 = { new Vector2(uvx1, uvy0), new Vector2(uvx0, uvy0), new Vector2(uvx0, uvy1), new Vector2(uvx0, uvy1), new Vector2(uvx1, uvy1), new Vector2(uvx1, uvy0) };
        Vector2[] uvdummy3 = { new Vector2(uvx3, uby3), new Vector2(uvx3, uvy2), new Vector2(uvx2, uby3), new Vector2(uvx3, uby3), new Vector2(uvx3, uvy2), new Vector2(uvx2, uby3) };
        Vector2[] uvdummy4 = { new Vector2(uvx5, uby3), new Vector2(uvx5, uvy2), new Vector2(uvx3, uby3), new Vector2(uvx5, uby3), new Vector2(uvx5, uvy2), new Vector2(uvx3, uby3) };

        //정점의 노멀값과 forward와의 각도을 계산해서 모아놓는다.
        for (int i = 0; i < vertiecs.Count; i += 3)
        {
            normals[i / 3] = Vector3.Normalize(Vector3.Cross((vertiecs[i + 1] - vertiecs[i]), (vertiecs[i + 2] - vertiecs[i])));
            dirAngle[i / 3] = CalculateAngle(Vector3.forward, normals[i / 3]);
        }

        //버텍스 인덱스와 uv인덱스를 만들어준다.
        for (int i = 0; i < vertiecs.Count; i++)
        {
            if (i % 3 == 0)
            {
                TexKind = 0;
                if (vertiecs[i].y == vertiecs[i + 1].y && vertiecs[i + 1].y == vertiecs[i + 2].y)
                    TexKind = 2;
                else
                {
                    MinHeight = Mathf.Min(vertiecs[i].y, vertiecs[i + 1].y, vertiecs[i + 2].y);
                    MaxHeight = Mathf.Max(vertiecs[i].y, vertiecs[i + 1].y, vertiecs[i + 2].y);
                    Height = MaxHeight - MinHeight;

                    if (vertiecs[i].y >= vertiecs[i + 1].y)
                        if (dirAngle[i / 3] >= 180.0f && dirAngle[i / 3] <= 220.0f)
                            TexKind = 1;
                }
            }

            if ((i % 6) == 0)
            {
                cmp = true;
                uvsCount = 0;
            }

            if (cmp == true)
                if (i > 0)
                    if (vertiecs[i] == vertiecs[i - 1])
                        cmp = false;

            dummy[i] = vertiecs[i];

            if (TexKind == 0)
                if (cmp == false)
                    uvs[i] = uvdummy2[uvsCount++];
                else
                    uvs[i] = uvdummy1[uvsCount++];
            else
            if (TexKind == 1)
                uvs[i] = uvdummy3[uvsCount++];
            else
                uvs[i] = uvdummy4[uvsCount++];
        }

        //텍스처를 연결해주고 버텍스인덱스,uv인덱스,정점버퍼를 연결해준다.
        mt.mainTexture = mainframe.Texture_local;
        mt.SetTextureScale("_MainTex", new Vector2(1f, (int)(Height / 3)));
        mr.material = mt;
        mesh_Local.vertices = dummy;
        mesh_Local.uv = uvs;
        mesh_Local.triangles = IndexData;
        mesh_Local.RecalculateNormals();
        mf.mesh = mesh_Local;

        //생성된 오브젝트의 parent를 지정해준다.
        go.transform.parent = this.transform;
        return go;
    }

    private float CalculateAngle(Vector3 fwd, Vector3 target)
    {
        float angle = Vector3.Angle(fwd, target);

        if (AngleDir(fwd, target, Vector3.up) == -1)
        {
            angle = 360.0f - angle;
            if (angle > 359.9999f)
                angle -= 360.0f;
        }
        return angle;
    }

    private int AngleDir(Vector3 fwd, Vector3 target, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, target);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
            return 1;
        else
        if (dir < 0.0f)
            return -1;
        else
            return 0;
    }
}
