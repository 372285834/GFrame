using UnityEngine;
using UnityEditor;
using GP;
using System.Collections.Generic;
using System.IO;
using System;
namespace GPEditor
{
    public class NodeEditor : EditorWindow
    {
        static string Suffix = ".tl";
        static string skillDir = "Assets/Arts/Skills/";
        //List<GTimeline> tList = new List<GTimeline>();
        static Dictionary<string, GTimelineStyle> allDic = new Dictionary<string, GTimelineStyle>();
        List<GTimelineStyle> showList = new List<GTimelineStyle>();
        [MenuItem("WindowTools/Skill", false, 1)]
        static void ShowEditor()
        {
            NodeEditor editor = EditorWindow.GetWindow<NodeEditor>();
            editor.titleContent = new GUIContent("Timeline");
            editor.minSize = new Vector2(300f,200f);
        }
        [MenuItem("WindowTools/exc", false, 1)]
        static void exc()
        {
            //EditorApplication.applicationContentsPath
        }
        TreeNode root;
        TreeNode mSelectData;
        TreeNode selectTimeLine;
        TreeNode selectData { 
            get
            {
                return mSelectData;
            }
            set
            {
                mSelectData = value;
                if (mSelectData.obj is GTimelineStyle)
                    selectTimeLine = mSelectData;
                else if (!object.Equals(mSelectData.parent, null) && mSelectData.parent.obj is GTimelineStyle)
                    selectTimeLine = mSelectData.parent;
            }
        }
        public GTimelineStyle curStyle { get { return !object.Equals(selectTimeLine, null) ? selectTimeLine.obj as GTimelineStyle : null; } }
        void OnEnable()
        {
            if (!Directory.Exists(skillDir))
                Directory.CreateDirectory(skillDir);
            InitData();
            if (root.Length > 0)
                selectData = root.childs[0];
        }
        void InitData()
        {
            GTimelineFactory.Init();
            GTimelineFactory.AutoRelease = false;
            List<GTimelineStyle> allList = LoadAssets(skillDir, "*" + Suffix);
            allDic.Clear();
            for (int i = 0; i < allList.Count; i++)
            {
                allDic[allList[i].name] = allList[i];
            }
            root = new TreeNode();
            root.name = "root";
            root.Add(allList);
            if (!object.Equals(selectData, null))
                selectData.isOpen = true;
        }
        void Update()
        {
            Repaint();
        }
        int treeIndex;
        Vector2 scrollPos = Vector2.zero;
        Vector2 pos = Vector2.zero;
        int levelSize = 0;
        float cellHeight = 18f;
        void OnGUI()
        {
            Rect rect = Rect.zero;
            rect.x = 1f;
            rect.y = 1f;
            drawTitle(rect);
            pos = new Vector2(2f, 25f);
            drawTree(pos);
            drawEvent();
        }
        void creatTimeline()
        {
            GTimelineStyle newTL = GTimelineStyle.CreatDefault("skill" + allDic.Count);
            if (!Directory.Exists(skillDir))
                Directory.CreateDirectory(skillDir);
            allDic.Add(newTL.name, newTL);
            selectTimeLine = selectData = root.Add(newTL, 0);
        }
        void drawTitle(Rect rt)
        {
            float bWdith = 60f;
            if (drawButton(rt, "Refresh", bWdith, 20f))//(GUILayout.Button("Refresh", GUILayout.MinHeight(30f), GUILayout.MinWidth(50f)))
            {
                InitData();
            }
            rt.x += 62f;
            if (curStyle != null)
            {
                if (drawButton(rt,"Apply-" + curStyle.name, 120f, 20f))//(GUILayout.Button("Apply-" + curStyle.name, GUILayout.MinHeight(30f), GUILayout.MinWidth(50f)))
                {
                    Save(curStyle);
                }
            }
            rt.x += 122f;
            if (drawButton(rt, "+", bWdith, 20f))
            {
                creatTimeline();
            }
        }
        Rect treeRect=Rect.zero;
        void drawTree(Vector2 pos)
        {
            treeRect.x = pos.x;
            treeRect.y = pos.y;
            Rect sposition = new Rect(pos.x, pos.y, position.width - 5f, position.height - 30f);
            if (!object.Equals(root, null) && root.Length > 0)
            {
                GUIStyle v = "verticalScrollbar";
                v.alignment = TextAnchor.UpperLeft;
                //scrollPos.y = GUI.VerticalScrollbar(new Rect(0, 35, 20f, position.height - 30f), scrollPos.y, 30f, 0, treeRect.height);
                if (sposition.width > 250f)
                    sposition.width = 250f;
                scrollPos = GUI.BeginScrollView(sposition, scrollPos, treeRect);
                //GUI.BeginGroup(rt);
                //float h = cellHeight * allDic.Count;
                treeIndex = 0;
                pos.y -= scrollPos.y+15f;
                treeRect.width = 0;
                levelSize = DrawFileTree(pos, root, 0);
                //treeRect.height = (cellHeight-0f) * treeIndex;
                //GUI.EndGroup();
                GUI.EndScrollView();
            }
        }
        //static GUIStyle nodeStyle = "Label";
        private int DrawFileTree(Vector2 pos, TreeNode node, int level)
        {
            if (object.Equals(node, null) || string.IsNullOrEmpty(node.name))
            {
                return level;
            }
            Rect rect = new Rect(pos.x + 16 * level, pos.y + cellHeight * (treeIndex), node.name.Length * 8f, cellHeight);
            if (treeRect.width < rect.x + rect.width)
                treeRect.width = rect.x + rect.width;
            treeRect.height = rect.y + cellHeight;
            treeIndex++;
            if (node.Length > 0)
            {
                Rect frt = rect;
                frt.width = 15f;
                frt.x -= 14f;
                node.isOpen = EditorGUI.Foldout(frt, node.isOpen, "", false);
                //rect.x += 15f;
            }
            GUIStyle nodeStyle = "Label";
            if (object.ReferenceEquals(node, selectData))
            {
                Rect bgRt = GUILayoutUtility.GetRect(node.content, nodeStyle);
                bgRt.x = rect.x;
                bgRt.height = rect.height;
                EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
            }
            GUI.Label(rect, node.content);
            //if (GUI.Button(rect, node.name, nodeStyle))
            //{
            //    selectData = node;
            //}
            EventType eType = Event.current.type;
            if (eType == EventType.MouseDown)
            {
                if (rect.Contains(Event.current.mousePosition) && !object.Equals(node.parent, null))
                {
                    selectData = node;
                    Event.current.Use();
                }
            }
            else if (eType == EventType.ContextClick)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (node.obj is GTimelineStyle)
                    {
                        ShowTimelineMenu(node);
                    }
                    else if(node.obj is GEventStyle)
                    {
                        ShowTimelineMenu(node);
                        //ShowEventMenu(node);
                    }
                    Event.current.Use();
                }
            }
            if (!node.isOpen || node.Length == 0)
            {
                return level;
            }
            for (int i = 0; i < node.childs.Count; i++)
            {
                DrawFileTree(pos, node.childs[i], level + 1);
            }
            return level;
        }
        bool drawButton(Rect rt,string str, float w, float h)
        {
            return drawButton(rt,str,w,h,null);
        }
        bool drawButton(Rect rt,string str,float w,float h,GUIStyle style)
        {
            rt.width = w;
            rt.height = h;
            if(style == null)
                return GUI.Button(rt, str);
            return GUI.Button(rt, str, style);
        }
        void drawEvent()
        {

        }
        void ShowTimelineMenu(TreeNode node)
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<Type, GEventAttribute> dic = GTimelineFactory.eventAttrDic;
            List<KeyValuePair<Type, GEventAttribute>> list = new List<KeyValuePair<Type, GEventAttribute>>();
            foreach (KeyValuePair<Type, GEventAttribute> kvp in dic)
            {
                list.Add(kvp);
            }
            list.Sort(delegate(KeyValuePair<Type, GEventAttribute> x, KeyValuePair<Type, GEventAttribute> y)
            {
                return x.Value.menu.CompareTo(y.Value.menu);
            });
            foreach (KeyValuePair<Type, GEventAttribute> kvp in dic)
            {
                menu.AddItem(new GUIContent(kvp.Value.menu), false, AddEventMenu, new KeyValuePair<Type, TreeNode>(kvp.Key, node));
            }
            menu.ShowAsContext();
        }
        void AddEventMenu(object param)
        {
            KeyValuePair<Type, TreeNode> kvp = (KeyValuePair<Type, TreeNode>)param;
            GEventStyle tl = kvp.Value.obj as GEventStyle;
            //Undo.RecordObjects(new UnityEngine.Object[] { tl, kvp.Value, this }, "add Event");
            GEventStyle s = tl.AddStyle(kvp.Key);
            kvp.Value.Add(s, s.typeName);
            kvp.Value.isOpen = true;
        }
        void ShowEventMenu(TreeNode node)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, AddEventMenu, new KeyValuePair<Type, TreeNode>(node.obj.GetType(), node.parent));
            menu.AddItem(new GUIContent("Delete"), false, DeleteEvent, node);
            menu.ShowAsContext();
        }
        void DeleteEvent(object param)
        {
            TreeNode node = (TreeNode)param;
            if (object.Equals(node.parent, null))
                return;
            GTimelineStyle tl = node.parent.obj as GTimelineStyle;
            GEventStyle eS = node.obj as GEventStyle;
           // Undo.RecordObjects(new UnityEngine.Object[] { tl,node, this }, "delete Event");
            tl.styles.Remove(eS);
            node.parent.childs.Remove(node);
        }
        class TreeNode:ScriptableObject
        {
            public string name;
            public int indent = 0;
            public GUIContent content;
            public List<TreeNode> childs = new List<TreeNode>();
            public object obj;
            public TreeNode parent;
            public bool isOpen = true;
            public Rect rect;
            public int Length
            {
                get { return childs == null ? 0 : childs.Count; }
            }
            public void Add(List<GTimelineStyle> list)
            {
                for(int i=0;i<list.Count;i++)
                {
                    Add(list[i],0);
                }
            }
            public TreeNode Add(GTimelineStyle style, int depth)
            {
                depth++;
                if (depth > 10)
                {
                    Debug.LogError("超出深度上限=10");
                    return null;
                }
                TreeNode d = Add(style, style.name);
                d.isOpen = false;
                for (int i = 0; i < style.styles.Count; i++)
                {
                    GEventStyle evt = style.styles[i];
                    d.AddEventData(evt, depth);
                }
                return d;
            }
            public TreeNode AddEventData(GEventStyle data, int depth)
            {
                if (data == null)
                    return null;
                TreeNode node = null;
                if (data is GPlayTimelineStyle)
                {
                    string tName = (data as GPlayTimelineStyle).styleRes;
                    if (!object.Equals(this.parent, null) && tName == this.parent.name)
                    {
                        Debug.LogError("错误的名字，timeLineName不能是自己");
                        return null;
                    }
                    if(string.IsNullOrEmpty(tName) || !allDic.ContainsKey(tName))
                    {
                        node = Add(data, data.typeName + "[" + tName + "]");
                        return node;
                    }
                    GTimelineStyle ts = allDic[tName];
                    node = Add(data, data.typeName + "[" + tName + "]");
                    node.Add(ts, depth);
                }
                else
                {
                    node = Add(data, data.typeName);
                }
                for (int i = 0; i < data.styles.Count; i++)
                {
                    GEventStyle evt = data.styles[i];
                    node.AddEventData(evt, depth);
                }
                return node;
            }
            public TreeNode Add(object _obj,string name)
            {
                TreeNode d = ScriptableObject.CreateInstance<TreeNode>();
                d.name = name;
                d.obj = _obj;
                d.content = new GUIContent(name, AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(_obj as UnityEngine.Object)));
                d.parent = this;
                this.childs.Add(d);
                return d;
            }
        }
        public static void Save(GTimelineStyle tls)
        {
            string json = GTimelineFactory.Serialize(tls);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(skillDir + tls.name + Suffix, false, new System.Text.UTF8Encoding(false));
            sw.Write(json);
            sw.Flush();
            sw.Close();
            sw.Dispose();
            AssetDatabase.SaveAssets();
        }
        public static List<GTimelineStyle> LoadAssets(string path, string pattern = "*")
        {
            if (!Directory.Exists(path))
                return null;
            List<GTimelineStyle> list = new List<GTimelineStyle>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] fls = dirInfo.GetFiles(pattern, SearchOption.AllDirectories);
            foreach (var file in fls)
            {
                string json = File.ReadAllText(file.FullName);
                GTimelineStyle ps = GTimelineFactory.DeSerialize(Path.GetFileNameWithoutExtension(file.Name),json);
                if (ps != null)
                    list.Add(ps);
            }
            return list;
        }
    }

}