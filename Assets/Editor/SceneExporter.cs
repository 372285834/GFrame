using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneExporter : MonoBehaviour {

    [MenuItem("Tools/Turn off all scenes Lighting settings")]
    public static void
    TurnOffAllSceneLightingSettings()
    {
        for (var i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCountInBuildSettings; ++i)
        {
            var scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByBuildIndex(i);
            RenderSettings.skybox = null;
            Lightmapping.bakedGI = false;
            Lightmapping.realtimeGI = false;
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.Enlighten;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
    }

    [MenuItem("Tools/Export Scene")]
    // Use this for initialization
    public static void
    Export()
    {
        GameObject[] _roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        Dictionary<object, object> _dict = new Dictionary<object, object>();
        foreach (GameObject _gb in _roots)    //遍历所有gameobject
        {
            ExportObject(_gb, _dict.Count,_dict);
        }

        string sDict = TileEditor.HELua.Encode(_dict);

        FileStream fs = new FileStream("c:\\temp\\scene.lua", FileMode.OpenOrCreate);
        fs.SetLength(0);
        StreamWriter ws = new StreamWriter(fs, Encoding.UTF8);
        ws.Write(sDict);
        ws.Close();
        fs.Close();
    }

    public static string
    GetRootPathName(Transform _tm)
    {
        string pathName = "";
        while (_tm)
        {
            pathName = "/" + _tm.name + pathName;
            if (!_tm.parent)
                break;
            _tm = _tm.transform.parent;
        }
        return pathName;
    }

    public static bool
    IsTopPrefab(GameObject _go)
    {
        if (!_go.transform.parent)
            return true;
        Object _prefab_parent = EditorUtility.GetPrefabParent(_go)
                    , _prefab_grand_parent = EditorUtility.GetPrefabParent(_go.transform.parent.gameObject);
        return AssetDatabase.GetAssetPath(_prefab_parent) != AssetDatabase.GetAssetPath(_prefab_grand_parent);
    }

    public static List<double>
    ToList(Vector3 _v)
    {
        List<double> _list = new List<double>();
        _list.Add(System.Math.Round(_v.x,3));
        _list.Add(System.Math.Round(_v.y,3));
        _list.Add(System.Math.Round(_v.z,3));
        return _list;
    }

    public static List<double>
    ToList(Quaternion _v)
    {
        List<double> _list = new List<double>();
        _list.Add(System.Math.Round(_v.x, 3));
        _list.Add(System.Math.Round(_v.y, 3));
        _list.Add(System.Math.Round(_v.z, 3));
        _list.Add(System.Math.Round(_v.w, 3));
        return _list;
    }

    public static int
    ExportObject(GameObject _go, int _parent_id,Dictionary<object, object> _dict)
    {
        if (!IsTopPrefab(_go))
            return -1;

        PrefabType _prefab_type = PrefabUtility.GetPrefabType(_go);
        string _prefab_path = "";
        switch (_prefab_type)
        {
            case PrefabType.Prefab:
            case PrefabType.ModelPrefab:
            case PrefabType.PrefabInstance:
            case PrefabType.ModelPrefabInstance:
                {
                    _prefab_path = AssetDatabase.GetAssetPath(EditorUtility.GetPrefabParent(_go));
                    break;
                }
        }

        int nIndex = _dict.Count + 1;
        Dictionary<object, object> _dict_self = new Dictionary<object, object>();
        _dict.Add(nIndex, _dict_self);

        string sPathName = GetRootPathName(_go.transform);
        Debug.Log(_prefab_type + ":" + _prefab_path + ":" + sPathName);
        int i;
        for (i = 0; i < _go.transform.childCount; i++)
        {
            ExportObject(_go.transform.GetChild(i).gameObject, nIndex,_dict);
        }

        Transform _transform = _go.transform;

        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        position = _transform.position;
        rotation = _transform.rotation;
        scale = _transform.localScale;

        _transform.position = Vector3.zero;
        _transform.rotation = Quaternion.Euler(Vector3.zero);
        _transform.localScale = Vector3.one;

        Collider[] colliders = _transform.GetComponents<Collider>();
        foreach (Collider child in colliders)
        {
            DestroyImmediate(child);
        }

        Vector3 center = Vector3.zero;
        Renderer[] renders = _transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer child in renders)
        {
            center += child.bounds.center;
        }
        if (renders.Length > 0)
        {
            center /= renders.Length;
        }
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer child in renders)
        {
            bounds.Encapsulate(child.bounds);
        }

        BoxCollider boxCollider = _transform.gameObject.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;

        _transform.position = position;
        _transform.rotation = rotation;
        _transform.localScale = scale;


        Dictionary<object, object>  _dict_local_transform = new Dictionary<object, object>();
        _dict_local_transform.Add("position", ToList(_transform.localPosition));
        _dict_local_transform.Add("rotation", ToList(_transform.localRotation));
        _dict_local_transform.Add("scale", ToList(_transform.localScale));
        _dict_self.Add("LocalSpace", _dict_local_transform);

        Dictionary<object, object> _dict_world_transform = new Dictionary<object, object>();
        _dict_world_transform.Add("position", ToList(_transform.localPosition));
        _dict_world_transform.Add("rotation", ToList(_transform.localRotation));
        _dict_world_transform.Add("scale", ToList(_transform.localScale));
        _dict_self.Add("WorldSpace", _dict_world_transform);

        Dictionary<object, object> _dict_bounds = new Dictionary<object, object>();

        Vector3 _point = new Vector3();
        _point = bounds.center - bounds.size;
        _dict_bounds.Add("Min", ToList(_point));
        _point = bounds.center + bounds.size;
        _dict_bounds.Add("Max", ToList(_point));

        _dict_self.Add("Name", _go.name);
        _dict_self.Add("Path", sPathName);
        _dict_self.Add("AABB", _dict_bounds);
        _dict_self.Add("Prefab", _prefab_path);
        _dict_self.Add("RangeOfVisiblility", 50);
        _dict_self.Add("Parent", _parent_id);

        
        return nIndex;
    }

    [MenuItem("Edit/Render Pipeline/Upgrade Project Materials to Lightweight Simple Lighting Materials")]
    // Use this for initialization
    public static void
    ReplaceAllMaterialsToLWRSimpleLighting()
    {
        //Object[] _roots = Resources.LoadAll("",typeof(GameObject));
        //Scene[] _roots = UnityEngine.SceneManagement.SceneManager.GetAllScenes()//().GetRootGameObjects();
        //foreach (Object _gb in _roots)    //遍历所有gameobject
        Object[] _roots = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject _gb in _roots)    //遍历所有gameobject
        {
            //ReplaceMaterial((GameObject)_gb, "LightweightPipeline/Standard (Simple Lighting)");
        }
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string _assetPath in assetPaths)
        {
            if (!_assetPath.Contains(".prefab"))
                continue;
            GameObject _gb = (GameObject)AssetDatabase.LoadAssetAtPath(_assetPath, typeof(GameObject));
            if (!_gb)
                continue;
            Debug.Log("Fetch:"+_assetPath);
            ReplaceMaterial(_assetPath,_gb, "LightweightPipeline/Standard (Simple Lighting)");
            //Object.DestroyImmediate(_gb);
        }
        EditorUtility.UnloadUnusedAssets();

    }
    public static void
    ReplaceMaterial(string _name,GameObject _gb, string _shader)
    {
        PrefabType _prefab_type = PrefabUtility.GetPrefabType(_gb);
        string sPathName = GetRootPathName(_gb.transform);
        /*switch (_prefab_type)
        {
            case PrefabType.Prefab:
                {
                    break;
                }
            case PrefabType.ModelPrefab:
                {
                    break;
                }
            case PrefabType.PrefabInstance:
                {
                    break;
                }
            case PrefabType.ModelPrefabInstance:
                {
                    break;
                }
            default:
                {
                    return;
                }
        }*/
        Renderer[] renders = _gb.transform.GetComponentsInChildren<Renderer>(true) as Renderer[];
        //if ( _prefab_path.Length == 0 )
        //    return;
        if (renders.Length == 0)
        {
            Debug.Log(_prefab_type + ":" + _name + ":no renders");
            return;
        }
        bool _isRegularModel = false;
        foreach (Renderer _render in renders)
        {
            if (_render.GetType() == typeof(MeshRenderer) || _render.GetType() == typeof(SkinnedMeshRenderer))
            {
                _isRegularModel = true;
            }
        }
        if (!_isRegularModel)
        {
            return;
        }
        foreach (Renderer _render in renders)
        {
            if (_render.GetType() == typeof(MeshRenderer) || _render.GetType() == typeof(SkinnedMeshRenderer))
            {
                if (_render.sharedMaterial)
                {
                    _render.sharedMaterial.shader = Shader.Find(_shader);
                }
            }
        }
        int i;
        for (i = 0; i < _gb.transform.childCount; i++)
        {
            ReplaceMaterial(_name,_gb.transform.GetChild(i).gameObject, _shader);
        }
        Debug.Log("Replaced material: " + _prefab_type + ":" + _name);
    }
}
