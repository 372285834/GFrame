using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using Frame;
using UnityEditor.SceneManagement;

public class MapWindow : EditorWindow
{
    [MenuItem("WindowTools/MapEditorWindow")]
    public static void Open()
    {
        MapWindow window = (MapWindow)EditorWindow.GetWindow(typeof(MapWindow));
        window.minSize = new Vector2(500, 300);
        LoadData();
    }
    public static UnityEngine.SceneManagement.Scene OpenScene(MapDataStyle style)
    {
        if (style == null)
            style = EditorPath.mapDataStyle;
        return UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneUrl, UnityEditor.SceneManagement.OpenSceneMode.Single);
    }
    public static string prefabUrl = "Assets/BundleEditing/Scene/shamo/prefab";
    public static string sceneUrl = "Assets/BundleEditing/Scene/shamoEditor.unity";
    static MapDataStyle mStyle;
    static List<GameObject> prefabList = new List<GameObject>();
    static Dictionary<string, MapItemPrefabData> mDic = new Dictionary<string, MapItemPrefabData>();
    public static void LoadData()
    {
        mStyle = EditorPath.mapDataStyle;
        prefabList = AssetDatabaseX.LoadAssets<GameObject>(prefabUrl, "*.prefab");
        prefabList.RemoveAll(x => x.GetComponent<MapItemMgr>() != null);
        mDic.Clear();
        //for (int i = 0; i < mStyle.mapItemPrefabDataList.Count; i++)
        //{
        //    MapItemPrefabData data = mStyle.mapItemPrefabDataList[i];
        //    mDic[data.guid] = data;
        //}
        for (int i=0;i<prefabList.Count;i++)
        {
            GameObject prefab = prefabList[i];
            string path = AssetDatabase.GetAssetPath(prefab);
            string guid = AssetDatabase.AssetPathToGUID(path);
            AssetImporter import = AssetImporter.GetAtPath(path);
            MapItemMono mono = prefab.GetComponent<MapItemMono>();
            if (mono == null)
            {
                prefab.hideFlags = HideFlags.None;
                mono = prefab.AddComponent<MapItemMono>();
                // PrefabUtility.ApplyAddedComponent(mono, path, InteractionMode.AutomatedAction);
                //List<AddedComponent> list = PrefabUtility.GetAddedComponents(GameObject prefabInstance);
            }
            mono.mRenders = mono.gameObject.GetComponentsInChildren<MeshRenderer>();
            EditorUtility.SetDirty(mono);
            if (!mDic.ContainsKey(guid))
            {
                mDic[guid] = new MapItemPrefabData();
                mDic[guid].guid = guid;
            }
            MapItemPrefabData data = mDic[guid];

            //import.assetBundleName = "scene/prefab/" + mono.name + ".prefab.k";
            //import.SaveAndReimport();

            data.abName = import.assetBundleName;
            data.prefab = mono;
        }
    }
    string[] inspectToolbarStrings = { "All", "Prefab", "Mesh" };
    enum InspectType
    {
        All, Prefab, Mesh,
    };
    InspectType ActiveInspectType = InspectType.All;
    Vector2 listScrollPos = new Vector2(0, 0);
    float ThumbnailWidth = 40;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh",GUILayout.MaxWidth(200f)))
            LoadData();
        if (mStyle == null)
            return;
        if(GUILayout.Button("AutoAllSize", GUILayout.MaxWidth(200f)))
            AutoAllSize();
        if (GUILayout.Button("Save", GUILayout.MaxWidth(200f)))
            Save();
        GUILayout.EndHorizontal();
       // GUILayout.BeginHorizontal();
        GUILayout.Label("Count: " + prefabList.Count);
        InspectType curType = (InspectType)GUILayout.Toolbar((int)ActiveInspectType, inspectToolbarStrings);
        ActiveInspectType = curType;

        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);
        RenderList(ActiveInspectType);
        EditorGUILayout.EndScrollView();
        ThumbnailWidth = EditorGUILayout.Slider(ThumbnailWidth, 20, 200);
        // GUILayout.EndHorizontal();
        //  GUILayout.BeginHorizontal();
    }
    void RenderList(InspectType tab)
    {
        foreach(var data in mDic.Values)
        {
            if (data.prefab == null)
                continue;
            MapItemMono mono = data.prefab;
            if (tab == InspectType.All || (int)tab == (int)mono.eType + 1)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(mono.name, GUILayout.Width(200f), GUILayout.Height(ThumbnailWidth)))
                {
                    Selection.activeObject = data.prefab;
                }
                GUILayout.BeginVertical(GUILayout.Height(ThumbnailWidth));
                //EditorGUI.indentLevel = 100;
                GUILayout.BeginHorizontal();
                mono.eType = (eMapItemType)EditorGUILayout.EnumPopup("eMapItemType:", mono.eType, GUILayout.Width(300f));
                //if (data.prefab.transform.childCount > 0)
                //     data.eType = eMapItemType.Prefab;
                mono.callLua = EditorGUILayout.Toggle("callLua:", mono.callLua, GUILayout.Width(300f));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                mono.size = EditorGUILayout.IntSlider("size:", mono.size, 1, 110, GUILayout.Width(300f));
                if(GUILayout.Button("AutoSize", GUILayout.Width(100f)))
                {
                    AutoSize(mono);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(7f);
            }
        }
    }
    public void AutoAllSize()
    {
        foreach (var data in mDic.Values)
        {
            AutoSize(data.prefab);
        }
        AssetDatabase.SaveAssets();
    }
    void AutoSize(MapItemMono prefab)
    {
        if (prefab == null)
            return;
        Bounds bounds = calcBounds(prefab.gameObject);
        prefab.size = (int)Mathf.Max(bounds.size.x, bounds.size.z);
        if(prefab.transform.childCount == 0 && prefab.GetComponent<MeshRenderer>() != null && prefab.GetComponent<Collider>() == null)
        {
            prefab.eType = eMapItemType.Mesh;
        }
        EditorUtility.SetDirty(prefab);
    }
    public static void Save()
    {
        mStyle.mapItemPrefabDataList.Clear();
        foreach (var data in mDic.Values)
        {
            if (data.prefab == null)
                continue;

            //MapItemMono mono = data.prefab;
            //Renderer[] rends = mono.GetComponentsInChildren<Renderer>(true);
            //for (int i = 0; i < rends.Length; i++)
            //{
            //    for (int j = 0; j < rends[i].sharedMaterials.Length; j++)
            //    {
            //        Material mat = rends[i].sharedMaterials[j];
            //        if (mat != null)
            //        {
            //            string path2 = AssetDatabase.GetAssetPath(mat);
            //            if (!path2.EndsWith(".mat") || path2.Contains("BundleResources"))
            //                continue;
            //            AssetImporter import2 = AssetImporter.GetAtPath(path2);
            //            import2.assetBundleName = "material/scene/" + mat.name + ".mat.k";
            //            import2.SaveAndReimport();
            //        }
            //    }
            //}
            if (data.eType == eMapItemType.Mesh)
            {
                MeshFilter mFilter = data.prefab.GetComponent<MeshFilter>();
                MeshRenderer mRender = data.prefab.GetComponent<MeshRenderer>();
                data.mesh = mFilter.sharedMesh;
                data.materials = mRender.sharedMaterials;
                // data.prefab = null;
            }
            else
            {
                
            }
            mStyle.mapItemPrefabDataList.Add(data);
        }
        EditorUtility.SetDirty(mStyle);
        AssetDatabase.SaveAssets();
    }


    public static Bounds calcBounds(GameObject go)
    {
        Vector3 min = Vector3.zero, max = Vector3.zero;

        MeshRenderer[] renders = go.GetComponentsInChildren<MeshRenderer>();
        if (renders.Length != 0)
        {
            min = renders[0].bounds.min;
            max = renders[0].bounds.max;

            for (int i = 1; i < renders.Length; ++i)
            {
                if (min.x > renders[i].bounds.min.x)
                    min.x = renders[i].bounds.min.x;

                if (min.y > renders[i].bounds.min.y)
                    min.y = renders[i].bounds.min.y;

                if (min.z > renders[i].bounds.min.z)
                    min.z = renders[i].bounds.min.z;

                if (max.x < renders[i].bounds.max.x)
                    max.x = renders[i].bounds.max.x;

                if (max.y < renders[i].bounds.max.y)
                    max.y = renders[i].bounds.max.y;

                if (max.z < renders[i].bounds.max.z)
                    max.z = renders[i].bounds.max.z;
            }
        }

        Bounds b = new Bounds();
        b.SetMinMax(min, max);
        return b;
    }


    //[MenuItem("WindowTools/showBirthNode")]
    //public static void showBirthNode()
    //{
    //    BirthNode.showGizmos = true;
    //}
    //[MenuItem("WindowTools/hideBirthNode")]
    //public static void hideBirthNode()
    //{
    //    BirthNode.showGizmos = false;
    //}

    [UnityEditor.MenuItem("WindowTools/更新shamo数据")]
    public static void EditorCreateMapDataStyle()
    {
        Selection.activeObject = EditorPath.mapDataStyle;
        MapDataStyleEditor.UpdateAll();
    }
}