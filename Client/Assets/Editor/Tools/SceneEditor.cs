using UnityEngine;
using UnityEditor;
using System.Collections;
using highlight;

public class SceneEditor
{
    [InitializeOnLoadMethod]
    static void Start()
    {
        EditorApplication.update += Update;
        Selection.selectionChanged = SelectionChanged;
        SceneView.onSceneGUIDelegate = OnSceneGUI;
    }
    static MapItemMono curItem;
    static void Update()
    {

        GameObject go = Selection.activeGameObject;
        if (go != null)
        {
            curItem = go.GetComponent<MapItemMono>();
        }
        UpdateUISerializeFieldInfo();
    }
    static void UpdateUISerializeFieldInfo()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
            return;
        if (!Application.isPlaying)
        {
            ISerializeField isField = go.GetComponent<ISerializeField>();
            if (isField != null)
                isField.SerializeFieldInfo();
        }
    }
    public static bool IsLimitSceneSelectGameObject = true;
   // static BirthNode selectNode = null;
    static void SelectionChanged()
    {
        UnityEngine.Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] is GameObject)
            {
                //if ((objects[i] as GameObject).GetComponent<BirthNode>() == null)
                //{
                //    BirthNode node = (objects[i] as GameObject).GetComponentInParent<BirthNode>();
                //    if (node != null)
                //    {
                //        selectNode = node;


                //    }
                //}
            }
        }
    }


    static Vector2 lastPos;
    static Rect mRect;
    static RaycastHit hit;
    static Vector3 curPos = Vector2.zero;
    static LayerMask mask = LayerMask.GetMask("Default");
    static bool isMoveing = false;
    static void OnSceneGUI(SceneView sceneView)
    {
        //if (!DrawSplat)
        //    return;
        UnityEngine.Event e = UnityEngine.Event.current;
        if (e.control)
            return;
        if (e.alt && e.type != UnityEngine.EventType.ScrollWheel) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector2 pos = e.mousePosition + new Vector2(10f, 5f);
        Physics.Raycast(ray, out hit, 1000f, mask);
        curPos = hit.point;
        if (e.isKey)
        {
            if (curItem != null)
            {
                if (e.keyCode == KeyCode.Space)
                {
                    if (!isMoveing && e.rawType == EventType.KeyDown)
                    {
                        Undo.RecordObject(curItem.transform, "MoveMapItemMono");
                       //  Debug.LogError("MoveMapItemMono");
                    }
                    curItem.transform.position = curPos;
                    isMoveing = true;
                }
                if (e.rawType == EventType.KeyUp)
                    isMoveing = false;
            }

        }
        
        if (e.control)
        {
            //HandleUtility.AddDefaultControl(0);

            //if (e.type == UnityEngine.EventType.MouseMove)
            //{
            //    lastPos = curPos;
            //    e.Use();
            //}
            //else if (e.type == UnityEngine.EventType.MouseDrag)
            //{
            //    lastPos = curPos;
            //    e.Use();
            //}
            //else if (e.type == UnityEngine.EventType.MouseUp)
            //{

            //    e.Use();
            //}
            GUILayout.BeginArea(new Rect(pos, new Vector2(120f, 40f)));

            GUILayout.Label(curPos.ToString());
            GUILayout.EndArea();
        }
        else if (e.shift)
        {

            GUILayout.BeginArea(new Rect(pos, new Vector2(120f, 40f)));

            GUILayout.Label(curPos.ToString());
            GUILayout.EndArea();
        }
        
        SceneView.RepaintAll();

        //lastPos = pos;
    }
}