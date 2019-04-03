using highlight.timeline;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace highlight
{
    public class TimeWindow : EditorWindow
    {
        public static TimeWindow Inst
        {
            get
            {
                return _Inst;
            }
        }
        static TimeWindow _Inst;
        static string Suffix = ".tl";
        static string timelineDir = "Assets/Arts/Timeline/";
        //List<GTimeline> tList = new List<GTimeline>();
        static List<TimelineStyle> allStyleList;
        static Dictionary<string, TimelineStyle> allDic = new Dictionary<string, TimelineStyle>();
        static string[] allStyleNames;
        static int[] allStyleNamesIndexs;
        //List<GTimelineStyle> showList = new List<GTimelineStyle>();
        [MenuItem("WindowTools/Timeline", false, 1)]
        public static void ShowEditor()
        {
            if (_Inst == null)
            {
                _Inst = EditorWindow.GetWindow<TimeWindow>();
                _Inst.titleContent = new GUIContent("Timeline");
                _Inst.minSize = new Vector2(250f, 200f);
                _Inst.LoadData();
            }
        }
        void OnEnable()
        {
            if (!Directory.Exists(timelineDir))
                Directory.CreateDirectory(timelineDir);
        }
        void LoadData()
        {
            TimelineFactory.Init();
            TimelineFactory.AutoRelease = false;
            allStyleList = LoadAssets(timelineDir, "*" + Suffix);
            InitStyles();
            curSelectIdx = 0;
            UpdateSelectSkill();
        }
        void InitStyles()
        {
            allDic.Clear();
            allStyleNames = new string[allStyleList.Count];
            allStyleNamesIndexs = new int[allStyleList.Count];
            for (int i = 0; i < allStyleList.Count; i++)
            {
                allDic[allStyleList[i].name] = allStyleList[i];
                allStyleNames[i] = allStyleList[i].name;
                allStyleNamesIndexs[i] = i;
            }
        }
        void UpdateSelectSkill()
        {
            if (allStyleList.Count == 0)
                return;
            if(root != null)
            {
                GameObject.DestroyImmediate(root.gameObject);
            }
            TimeNode node = TimelineNode.Creat(allStyleList[curSelectIdx]);
            SetSelect(node);
        }
        void SetSelect(TimeNode node)
        {
            if (curNode == node)
                return;
            curNode = node;
            Selection.activeGameObject = node.gameObject;
            this.Repaint();
        }
        static UnityEngine.Object selectObj;
        public static TimeNode curNode;
        public static TimelineNode root { get { return curNode == null ? null : curNode.root; } }
        [InitializeOnLoadMethod]
        static void Start()
        {
            EditorApplication.update += Update;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchWindowOnGui;
        }
        static void Update()
        {
            if (Selection.objects != null && Selection.objects.Length == 1 && selectObj != Selection.objects[0])
            {
                selectObj = Selection.objects[0];
                string path = AssetDatabase.GetAssetPath(Selection.objects[0]);
                if (path.EndsWith(".tl"))
                {
                    ShowEditor();
                    int idx = allStyleList.FindIndex(x => x.name == Selection.objects[0].name);
                    if (idx > -1)
                    {
                        Inst.curSelectIdx = idx;
                        Inst.UpdateSelectSkill();
                        Inst.Repaint();
                    }
                }
            }
            if(Inst == null)
            {
                if (root != null)
                {
                    GameObject.DestroyImmediate(root.gameObject);
                }
            }
            else
            {
                if(Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<TimeNode>() != null)
                {
                    TimeNode node = Selection.activeGameObject.GetComponent<TimeNode>();
                    if (node.obj == null)
                    {
                        GameObject.DestroyImmediate(node.gameObject);
                    }
                    else if(node != curNode)
                    {
                        Inst.SetSelect(node);
                    }
                }
            }
            if(curNode != null)
            {
                curNode.UpdateData();
            }
        }
        public static bool EnableCustomHierarchy = true;
        // 绘制Hiercrch
        static void HierarchWindowOnGui(int instanceId, Rect selectionRect)
        {
            if (!EnableCustomHierarchy) return;
            try
            {
                // CheckBox // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
               // var rectCheck = new Rect(selectionRect);
               // rectCheck.x += rectCheck.width - 20;
               // rectCheck.width = 18;
                // 通过ID获得Obj
                var obj = EditorUtility.InstanceIDToObject(instanceId);
                var go = (GameObject)obj;// as GameObject;
                if(go.GetComponent<TimeNode>() != null)
                {
                    EventType eType = Event.current.type;
                    if (eType == EventType.ContextClick)
                    {
                        if (selectionRect.Contains(Event.current.mousePosition))
                        {
                            TimeNode node = go.GetComponent<TimeNode>();
                            Inst.SetSelect(node);
                            Inst.ShowTimelineMenu(node);
                            Event.current.Use();
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            drawTitle();
            GUILayout.Space(5f);
            drawAddNode();
            GUILayout.Space(5f);
            drawRoot();
            GUILayout.Space(5f);
            DrawTreeTimeline(root, 0);

            EditorGUILayout.EndVertical();
        }
        int curSelectIdx = 0;
        void drawTitle()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh", GUILayout.MinHeight(25f), GUILayout.MinWidth(50f)))
            {
                if (Inst == null)
                    ShowEditor();
                LoadData();
            }
            if (allStyleNames == null)
                return;
            int newIdx = EditorGUILayout.IntPopup(curSelectIdx, allStyleNames, allStyleNamesIndexs, GUILayout.MinHeight(25f), GUILayout.MinWidth(50f)); //(rt, curSelectIdx, allStyleNames);
            if (newIdx != curSelectIdx)
            {
                curSelectIdx = newIdx;
                UpdateSelectSkill();
            }
            if (GUILayout.Button("+", GUILayout.MinHeight(25f), GUILayout.MinWidth(50f))) 
            {
                creatTimeline();
            }
            EditorGUILayout.EndHorizontal();
            
        }

        public enum NodeType
        {
            空节点,
            动画,
            特效,
            导弹,
        }
        NodeType curType = NodeType.空节点;
        void drawAddNode()
        {
            if (curNode != null)
            {
                EditorGUILayout.BeginHorizontal();
                curType = (NodeType)EditorGUILayout.EnumPopup(curType);
                if (GUILayout.Button("添加子节点", GUILayout.MinHeight(25f), GUILayout.MinWidth(50f)))
                {
                    addChild(curNode,curType);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        void addChild(TimeNode parent,NodeType type)
        {
            TimeNode node = parent.AddChild(null);
            SetSelect(node);
            switch (type)
            {
                case NodeType.空节点:
                    break;
                case NodeType.动画:
                    break;
                case NodeType.特效:
                    break;
                case NodeType.导弹:
                    break;
                default:
                    break;
            }
        }
        void creatTimeline()
        {
            TimelineStyle newTL = TimelineStyle.CreatDefault("timeline" + allStyleList.Count);
            if (!Directory.Exists(timelineDir))
                Directory.CreateDirectory(timelineDir);
            allStyleList.Add(newTL);
            InitStyles();
            curSelectIdx = allStyleList.Count - 1;
            UpdateSelectSkill();
            // SetSelect(root.Add(newTL, 0));
            root.isChange = true;
        }
        private void DrawTreeTimeline(TimeNode node, int level)
        {
            if (node == null || node.obj == null)
                return;
            EditorGUILayout.BeginHorizontal();
            if (node == curNode)
            {
                GUI.contentColor = Color.green;
            }
            else
            {
                GUI.contentColor = Color.white;
            }
            TimeStyle style = node.style;
            FrameRange validRange = node.obj.GetMaxFrameRange();
            FrameRange rang = style.Range;
            float sliderStartFrame = rang.Start;
            float sliderEndFrame = rang.End;
            EditorGUI.BeginChangeCheck();
            float allW = position.width - 150f;
            float evtW = allW * (float)validRange.Length / node.root.obj.Length;
            float startX = allW * (float)validRange.Start / node.root.obj.Length;
            GUILayout.Space(startX);
            if (GUILayout.Button(node.name, GUILayout.Width(70f)))
                SetSelect(node);
            GUILayout.Label(validRange.Start.ToString(), GUILayout.Width(20f));
            GUI.enabled = !node.isRoot;
            EditorGUILayout.MinMaxSlider(ref sliderStartFrame, ref sliderEndFrame, validRange.Start, validRange.End, GUILayout.Width(evtW));
            GUI.enabled = true;
            GUILayout.Label(validRange.End.ToString(), GUILayout.Width(30f));
            if (!node.isRoot)
            {
                rang = FrameRange.Resize((int)sliderStartFrame, (int)sliderEndFrame, validRange);
                if (rang != style.Range)
                    root.isChange = true;
                style.Range = rang;
                node.obj.OnStyleChange();
            }
            if (EditorGUI.EndChangeCheck())
            {
                SetSelect(node);
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < node.transform.childCount; i++)
            {
                DrawTreeTimeline(node.transform.GetChild(i).GetComponent<TimeNode>(), level + 1);
            }
        }

        void drawRoot()
        {
            if (root == null)
                return;
            string rName = "保存-" + root.name;
            if (root.isChange)
                rName += "*";
            TimelineStyle style = root.timelineStyle;
            if (style == null)
                return;
            if (GUILayout.Button(rName, GUILayout.MinHeight(30f)))
            {
                Save(style);
            }
            EditorGUILayout.BeginHorizontal();
            // root.name = EditorGUILayout.TextField(root.name);
            GUILayout.Label("帧率：", EditorStyles.label);
            style.FrameRate = EditorGUILayout.IntField(style.FrameRate);
            GUILayout.Label("总帧数:", EditorStyles.label);
            style.x = 0;
            style.y = EditorGUILayout.IntSlider(style.y,1,999999);
            GUILayout.Label("t:" + style.LengthTime + "s", GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();
        }


        void ShowTimelineMenu(TimeNode node)
        {
            GenericMenu menu = new GenericMenu();
            foreach (NodeType item in Enum.GetValues(typeof(NodeType)))
            {
                menu.AddItem(new GUIContent("添加/" + item.ToString()), false, AddEventMenu, new KeyValuePair<NodeType, TimeNode>(item, node));
            }
            if (!node.isRoot)
            {
                menu.AddItem(new GUIContent("复制"), false, DuplicateMenu, node);
                menu.AddItem(new GUIContent("删除"), false, DeleteEvent, node);
            }
            menu.ShowAsContext();
        }
        void AddEventMenu(object param)
        {
            KeyValuePair<NodeType, TimeNode> kvp = (KeyValuePair<NodeType, TimeNode>)param;
            addChild(kvp.Value, kvp.Key);
        }
        void DuplicateMenu(object param)
        {
            TimeNode node = (TimeNode)param;
            TimeStyle clone = CloneStyle(node.style);// (node.style).Clone() as TimeStyle;
            TimeNode newNode = node.parent.AddChild(clone);
            SetSelect(newNode);
        }
        void DeleteEvent(object param)
        {
            TimeNode node = (TimeNode)param;
            TimeNode parent = node.parent;
            parent.RemoveChild(node);
            SetSelect(parent);
        }
        public static TimeStyle CloneStyle(TimeStyle source)
        {
            string json = JsonConvert.SerializeObject(source, Newtonsoft.Json.Formatting.Indented, getSetting());
            TimeStyle to = JsonConvert.DeserializeObject(json, typeof(TimeStyle), getSetting()) as TimeStyle;
            return to;
        }
        public static void Save(TimelineStyle tls)
        {
            string json = JsonConvert.SerializeObject(tls, Newtonsoft.Json.Formatting.Indented, getSetting());
            //string json = GJsonInfo.Serialize(tls);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(timelineDir + tls.name + Suffix, false, new System.Text.UTF8Encoding(false));
            sw.Write(json);
            sw.Flush();
            sw.Close();
            sw.Dispose();
            if (Inst != null)
            {
                root.isChange = false;
                Inst.Repaint();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public static List<TimelineStyle> LoadAssets(string path, string pattern = "*")
        {
            if (!Directory.Exists(path))
                return null;
            List<TimelineStyle> list = new List<TimelineStyle>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] fls = dirInfo.GetFiles(pattern, SearchOption.AllDirectories);
            foreach (var file in fls)
            {
                string json = File.ReadAllText(file.FullName);
                try
                {
                    TimelineStyle ps = JsonConvert.DeserializeObject(json, typeof(TimelineStyle), getSetting()) as TimelineStyle;// GJsonInfo.DeSerialize(json) as GTimelineStyle;
                    if (ps != null)
                        list.Add(ps);
                    //bool b = RefreshStyle(ps, false);
                  //  if (b)
                   // {
                   //     Save(ps);
                  //  }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                    Debug.LogError(json);
                }
            }
            return list;
        }
        public static JsonSerializerSettings getSetting()
        {
            JsonSerializerSettings setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore };
            return setting;
        }

    }
}