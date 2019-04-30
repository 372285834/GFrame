using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
[CanEditMultipleObjects, CustomEditor(typeof(MapDataStyle))]
[ExecuteInEditMode]
public class MapDataStyleEditor : Editor
{
    MapDataStyle mScript;
    public override void OnInspectorGUI()
    {
        mScript = (MapDataStyle)target;
        //serializedObject.Update();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("更新预设数据"))
        {
            MapWindow.LoadData();
            MapWindow.Save();
        }
        if (GUILayout.Button("更新地图数据"))
        {
            UpdateMapData(mScript);
        }
        if (GUILayout.Button("更新All"))
        {
            UpdateAll();
        }
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
        if (Application.isPlaying && mScript.ChunkPoolDataList.Count > 0)
        {
            if (GUILayout.Button("ReLoad"))
            {
                ReLoad();
            }
        }
        //if (GUILayout.Button("Test"))
        //{
        //    Test();
        //}
    }
    public static void UpdateAll()
    {
        MapWindow.LoadData();
        MapWindow.Save();
        UpdateMapData(EditorPath.mapDataStyle);
    }
    static Dictionary<string, MapItemPrefabData> mapItemPrefabDataDic = new Dictionary<string, MapItemPrefabData>();
    public static void UpdateMapData(MapDataStyle style)
    {
        List<MapItemPrefabData> mapItemPrefabDataList = style.mapItemPrefabDataList;
        List<MapItemPos> dataList = style.dataList;
        dataList.Clear();
        mapItemPrefabDataDic.Clear();
        for (int i = 0; i < mapItemPrefabDataList.Count; i++)
        {
            mapItemPrefabDataList[i].tempId = i;
            mapItemPrefabDataDic[mapItemPrefabDataList[i].guid] = mapItemPrefabDataList[i];
        }
        UnityEngine.SceneManagement.Scene scene = MapWindow.OpenScene(style);
        List<GameObject> gos = new List<GameObject>();
        //scene.GetRootGameObjects(gos);
        GameObject[] goArr = scene.GetRootGameObjects();
        for (int i=0;i< goArr.Length; i++)
        {
            GetPrefabRegular(goArr[i], gos);
        }
        int count = gos.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject go = gos[i];
            EditorUtility.DisplayProgressBar("处理中", go.name + ":" + i + " /" + count, (float)i / (float)count);
            addDataToList(dataList, go);
        }
        EditorUtility.SetDirty(style);
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        Debug.Log("地图数据更新完毕:" + dataList.Count);
    }
    static void GetPrefabRegular(GameObject go, List<GameObject> gos)
    {
        MapItemMono[] monos = go.GetComponentsInChildren<MapItemMono>();
        for (int i = 0; i < monos.Length; i++)
        {
            EditorUtility.DisplayProgressBar("处理中", i + "/" + monos.Length, (float)i/(float)monos.Length);
            gos.Add(monos[i].gameObject);
        }
        /*
        PrefabAssetType type = PrefabUtility.GetPrefabAssetType(go);
        if (type == PrefabAssetType.Regular)
        {
            EditorUtility.DisplayProgressBar("处理中", gos.Count.ToString(), 0f);
            gos.Add(go);
        }
       // else
        {
            int num = 0;
            while (num < go.transform.childCount)
            {
                GetPrefabRegular(go.transform.GetChild(num).gameObject,gos);
                num++;
            }
        }*/
    }
    static void addDataToList(List<MapItemPos> dataList,GameObject go)
    {
        GameObject prefab = PrefabUtility.FindPrefabRoot(go);
        string path = AssetDatabase.GetAssetPath(prefab);
        string guid = AssetDatabase.AssetPathToGUID(path);
        MapItemPrefabData data = null;
        mapItemPrefabDataDic.TryGetValue(guid, out data);
        if (data != null)
        {
            MapItemPos itemPos = new MapItemPos();
            itemPos.id = (ushort)data.tempId;
            itemPos.pos = go.transform.position;
            itemPos.euler = go.transform.eulerAngles;
            itemPos.scale = go.transform.lossyScale;
            MapItemMono.SerializeLightMapData(go.GetComponent<MapItemMono>(),itemPos);
            dataList.Add(itemPos);
        }
    }

    void Test()
    {
        int poolLength = mScript.ChunkPoolDataList.Count;
        int count = mScript.dataList.Count;
        for (int i = 0; i < mScript.mapItemPrefabDataList.Count; i++)
        {
            MapItemPrefabData data = mScript.mapItemPrefabDataList[i];
            mScript.mapItemPrefabDataDic[data.tempId] = data;
            data.Init();
            if (data.isPreLoad)
            {
                data.Load();
            }
            //if (data.eType == eMapItemType.Prefab)
        }
        StringBuilder sb = new StringBuilder();
        int[] itemNums = new int[poolLength];
        StringBuilder[] sbNums = new StringBuilder[poolLength];
        for (int i = 0; i < count; i++)
        {
            MapItemPos itemPos = mScript.dataList[i];
            MapItemPrefabData data = mScript.GetPrefabData(itemPos.id);
            if (data != null)
            {
                int size = Mathf.FloorToInt(data.size * itemPos.size);
                for (int j = 0; j < poolLength; j++)
                {
                    if (size <= mScript.ChunkPoolDataList[j].cell.x || j == poolLength - 1)
                    {
                        itemNums[j]++;
                        if (j == poolLength -1)
                        {
                            sbNums[j].Append(data.prefab.name + ", size:" + size);
                        }
                        break;
                    }
                }
            }
        }
        for(int i=0;i<poolLength;i++)
        {
            sbNums[i].Insert(0, "num:" + itemNums[i] + "\n");
            Debug.Log(sbNums[i].ToString());
        }
    }

    void ReLoad()
    {
        for (int i = 0; i < mScript.ChunkPoolList.Count; i++)
        {
            mScript.ChunkPoolList[i].ClearItem();
        }
        mScript.ChunkPoolList.Clear();
        mScript.Init();
    }
}


