using UnityEngine;
using UnityEditor;
using GP;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
namespace GPEditor
{
    public class SkillWindow : EditorWindow
    {
        public static SkillWindow Inst
        {
            get
            {
                return _Inst;
            }
        }
        static SkillWindow _Inst;
        static string Suffix = ".tl";
        static string skillDir = "Assets/Arts/Skills/";
        //List<GTimeline> tList = new List<GTimeline>();
        static List<GTimelineStyle> allStyleList;
        static Dictionary<string, GTimelineStyle> allDic = new Dictionary<string, GTimelineStyle>();
        static string[] allStyleNames;
        static int[] allStyleNamesIndexs;
        List<GTimelineStyle> showList = new List<GTimelineStyle>();
        [MenuItem("WindowTools/Skill", false, 1)]
        public static void ShowEditor()
        {
            if (_Inst == null)
            {
                _Inst = EditorWindow.GetWindow<SkillWindow>();
                _Inst.titleContent = new GUIContent("Timeline");
                _Inst.minSize = new Vector2(250f, 200f);
            }
        }
        //[MenuItem("WindowTools/exc", false, 1)]
        //static void exc()
        //{
        //}
        TreeNode root;
        TreeNode m_selectData;
        TreeNode rootNode;
        TreeNode selectData { 
            get
            {
                return m_selectData;
            }
        }
        public void SetSelect(TreeNode node)
        {
            m_selectData = node;
            rootNode = m_selectData.root;
            curStyle = rootNode.style as GTimelineStyle;
            GTimelineFactory.ReleaseTimeline(curTimline);
            curTimline = GTimelineFactory.CreatTimeline(curStyle);
            rootNode.SetEvent(curTimline);
            GEventWindow.Open(m_selectData, curEvent);
        }
        public GTimeline curTimline;
        public GTimelineStyle curStyle;
        public GEvent curEvent { get { return m_selectData.evt; } }
        void OnEnable()
        {
            if (!Directory.Exists(skillDir))
                Directory.CreateDirectory(skillDir);
            LoadData();
        }
        void LoadData()
        {
            curTimline = null;
            GTimelineFactory.Init();
            GTimelineFactory.AutoRelease = false;
            allStyleList = LoadAssets(skillDir, "*" + Suffix);
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
            root = new TreeNode();
            root.name = "root";
            root.Add(allStyleList[curSelectIdx], 0);
            if (root.Length > 0)
                SetSelect(root.childs[0]);
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
        Rect treeRect = Rect.zero;
        void OnGUI()
        {
            _Inst = this;
            Rect rect = Rect.zero;
            rect.x = 1f;
            rect.y = 1f;
            drawTitle(rect);
            pos = new Vector2(2f, 25f);

            treeRect.x = pos.x;
            treeRect.y = pos.y;
            Rect sposition = new Rect(pos.x, pos.y, position.width - 5f, position.height - 30f);
            scrollPos = GUI.BeginScrollView(sposition, scrollPos, treeRect);
            pos = drawTree(pos);
            GUI.EndScrollView();
            drawEvent();
            //treeRect.height += treeRect.height;
        }
        void creatTimeline()
        {
            GTimelineStyle newTL = GTimelineStyle.CreatDefault("skill" + allStyleList.Count);
            if (!Directory.Exists(skillDir))
                Directory.CreateDirectory(skillDir);
            allStyleList.Add(newTL);
            InitStyles();
            curSelectIdx = allStyleList.Count-1;
            UpdateSelectSkill();
           // SetSelect(root.Add(newTL, 0));
            rootNode.isChange = true;
        }
        int curSelectIdx = 0;
        void drawTitle(Rect rt)
        {
            float bWdith = 60f;
            rt.width = 120f;
            rt.height = 20f;
            
            int newIdx = EditorGUI.IntPopup(rt,curSelectIdx, allStyleNames, allStyleNamesIndexs); //(rt, curSelectIdx, allStyleNames);
            if(newIdx != curSelectIdx)
            {
                curSelectIdx = newIdx;
                UpdateSelectSkill();
            }
            rt.x += 122f;
            if (drawButton(rt, "Refresh", bWdith, 20f))//(GUILayout.Button("Refresh", GUILayout.MinHeight(30f), GUILayout.MinWidth(50f)))
            {
                LoadData();
            }
            rt.x += 62f;
            if (drawButton(rt, "+", bWdith, 20f))
            {
                creatTimeline();
            }
            //rt.x += 70f;
            //if (curStyle != null)
            //{
            //    curStyle.Start = 0;
            //    //rt.x += 122f;
            //    rt.width = 150f;
            //    rt.height = 20f;
            //    curStyle.name = EditorGUI.TextField(rt,curStyle.name);
            //    rt.width = 100f;
            //    rt.x += 152f;
            //    curStyle.UpdateMode = (AnimatorUpdateMode)EditorGUI.EnumPopup(rt, curStyle.UpdateMode);
            //    rt.x += 102f;
            //    curStyle.FrameRate = drawInt(rt, "帧率：", curStyle.FrameRate);
            //    rt.x += 120f;
            //    curStyle.Length = drawInt(rt, "长度：", curStyle.Length);
            //    rt.x = this.position.width - 150f;
            //    if (drawButton(rt, "Apply-" + curStyle.name, 150f, 20f))//(GUILayout.Button("Apply-" + curStyle.name, GUILayout.MinHeight(30f), GUILayout.MinWidth(50f)))
            //    {
            //        Save(curStyle);
            //    }
            //}
            //rt.x += 122f;
            //if (drawButton(rt, "test", bWdith, 20f))
            //{
            //    JsonConvert.SerializeObject(Vector3.one);
            //}
        }
        float treeW = 0f;
        int drawInt(Rect rt,string name,int v)
        {
            rt.width = 35f;
            EditorGUI.PrefixLabel(rt, new GUIContent(name));
            rt.x += 35f;
            rt.width = 80f;
            v = EditorGUI.IntField(rt, v);
            return v;
        }
        Vector3 drawTree(Vector2 pos)
        {
            //treeRect.width = sposition.width - 10f;
            if (!object.Equals(root, null) && root.Length > 0)
            {
                //GUIStyle v = "verticalScrollbar";
                //v.alignment = TextAnchor.UpperLeft;
                //scrollPos.y = GUI.VerticalScrollbar(new Rect(0, 35, 20f, position.height - 30f), scrollPos.y, 30f, 0, treeRect.height);
                //if (sposition.width > 250f)
               //     sposition.width = 250f;
                treeW = treeRect.width;
                //GUI.BeginGroup(rt);
                //float h = cellHeight * allDic.Count;
                treeIndex = 0;
                pos.y -= scrollPos.y+15f;
                treeRect.width = 0;
                levelSize = DrawFileTree(pos, root, 0);
                //treeRect.height = (cellHeight-0f) * treeIndex;
                //GUI.EndGroup();
            }
            return pos;
        }
        //static GUIStyle nodeStyle = "Label";
        private int DrawFileTree(Vector2 pos, TreeNode node, int level)
        {
            if (object.Equals(node, null) || string.IsNullOrEmpty(node.name))
            {
                return level;
            }
            Rect rect = new Rect(pos.x + 16 * level, pos.y + cellHeight * (treeIndex), node.name.Length * 8f + 50f, cellHeight);
            Rect lbRect = rect;
            treeRect.height = rect.y + cellHeight;
            //GUIStyle nodeStyle = "Label";
            if (treeRect.width < rect.x + rect.width)
                treeRect.width = rect.x + rect.width;
            treeIndex++;
            rect.x = 0f;
            rect.width = treeW;
            if (object.ReferenceEquals(node, selectData))
            {
                EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
            }
            if (node.Length > 0)
            {
                Rect frt = lbRect;
                frt.width = 15f;
                frt.x -= 14f;
                node.isOpen = EditorGUI.Foldout(frt, node.isOpen, "", false);
                //rect.x += 15f;
            }
            string lb = node.name;
            if(node.Length > 0)
                lb += "(" + node.Length + ")";
            if (node.isChange)
                lb += "*";
            GUI.Label(lbRect, lb);
            EventType eType = Event.current.type;
            if (eType == EventType.MouseDown)
            {
                if (rect.Contains(Event.current.mousePosition) && !object.Equals(node.parent, null))
                {
                    SetSelect(node);
                    Event.current.Use();
                }
            }
            else if (eType == EventType.ContextClick)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (node.style != null)
                    {
                        ShowTimelineMenu(node);
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
            EditorGUILayout.BeginVertical();
            GUILayout.Space(treeRect.y + treeRect.height);
            GUI.enabled = false;
            DrawTreeTimeline(rootNode, 0);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }
        int treeIndex2;
        private int DrawTreeTimeline(GPEditor.SkillWindow.TreeNode node, int level)
        {
            if (object.Equals(node, null) || string.IsNullOrEmpty(node.name))
            {
                return level;
            }
            treeIndex2++;
            EditorGUILayout.BeginHorizontal();
            GEvent evt = node.evt;
            if (evt == curEvent)
            {
                GUI.contentColor = Color.green;
            }
            else
            {
                GUI.contentColor = Color.white;
            }
            GEventStyle tStyle = evt.mStyle;
            FrameRange validRange = evt.GetMaxFrameRange();
            FrameRange rang = tStyle.range;
            float sliderStartFrame = rang.Start;
            float sliderEndFrame = rang.End;
            EditorGUI.BeginChangeCheck();
            float allW = position.width - 150f;
            float evtW = allW * (float)validRange.Length / evt.root.Length;
            float startX = allW * (float)validRange.Start / evt.root.Length;
            GUILayout.Space(startX);
            if (GUILayout.Button(tStyle.Attr.name, GUILayout.Width(60f)))
                SetSelect(node);
            GUILayout.Label(validRange.Start.ToString(), GUILayout.Width(30f));
            EditorGUILayout.MinMaxSlider(ref sliderStartFrame, ref sliderEndFrame, validRange.Start, validRange.End, GUILayout.Width(evtW));
            GUILayout.Label(validRange.End.ToString(), GUILayout.Width(30f));
            if (EditorGUI.EndChangeCheck())
            {
                rang.Start = (int)sliderStartFrame;
                rang.End = (int)sliderEndFrame;
                tStyle.range = rang;
                rootNode.isChange = true;
                evt.OnStyleChange();
                SetSelect(node);
            }
            EditorGUILayout.EndHorizontal();
            EventType eType = Event.current.type;
            if (node.Length == 0)
            {
                return level;
            }
            for (int i = 0; i < node.childs.Count; i++)
            {
                DrawTreeTimeline(node.childs[i], level + 1);
            }
            return level;
        }
        void ShowTimelineMenu(TreeNode node)
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<Type, GEventAttribute> dic = GTimelineFactory.eventAttrDic;
            List<KeyValuePair<Type, GEventAttribute>> list = new List<KeyValuePair<Type, GEventAttribute>>();
            foreach (KeyValuePair<Type, GEventAttribute> kvp in dic)
            {
                if (kvp.Value.dataType == typeof(GTimeline) || kvp.Value.Obsolete)
                    continue;
                list.Add(kvp);
            }
            list.Sort(delegate(KeyValuePair<Type, GEventAttribute> x, KeyValuePair<Type, GEventAttribute> y)
            {
                return x.Value.menu.CompareTo(y.Value.menu);
            });
            foreach (KeyValuePair<Type, GEventAttribute> kvp in list)
            {
                menu.AddItem(new GUIContent(kvp.Value.menu), false, AddEventMenu, new KeyValuePair<Type, TreeNode>(kvp.Key, node));
            }
            if (!(node.style is GTimelineStyle))
            {
                menu.AddItem(new GUIContent("复制"), false, DuplicateMenu, node);
                menu.AddItem(new GUIContent("删除"), false, DeleteEvent, node);
            }
            menu.ShowAsContext();
        }
        void AddEventMenu(object param)
        {
            KeyValuePair<Type, TreeNode> kvp = (KeyValuePair<Type, TreeNode>)param;
            GEventStyle tl = kvp.Value.style;
            kvp.Value.isOpen = true;
            //Undo.RecordObjects(new UnityEngine.Object[] { tl, kvp.Value, this }, "add Event");
            GEventStyle newStyle = tl.CreatStyle(kvp.Key);
            GEvent parent = curTimline.Get(tl);
            parent.AddChild(newStyle);
            SetSelect(kvp.Value.Add(newStyle, newStyle.Attr.name));
            rootNode.isChange = true;
        }
        void DuplicateMenu(object param)
        {
            TreeNode node = (TreeNode)param;
            if (object.Equals(node.parent, null))
                return;
            GEventStyle tl = node.parent.style;
            GEventStyle clone = (node.style).Clone() as GEventStyle;
            GEvent parent = curTimline.Get(tl);
            parent.AddChild(clone);
            SetSelect(node.parent.AddEventData(clone, 0));
            rootNode.isChange = true;
        }
        void DeleteEvent(object param)
        {
            TreeNode node = (TreeNode)param;
            if (object.Equals(node.parent, null))
                return;
            GEventStyle tl = node.parent.style;
            GEvent parent = curTimline.Get(tl);
            parent.Remove(curEvent);
            node.parent.childs.Remove(node);
            SetSelect(node.parent);
            rootNode.isChange = true;
        }
        public class TreeNode:ScriptableObject
        {
            public string name;
            public int indent = 0;
            public GUIContent content;
            public List<TreeNode> childs = new List<TreeNode>();
            public object obj;
            public GEvent evt;
            public GEventStyle style
            {
                get { return obj as GEventStyle; }
            }
            public TreeNode parent;
            public bool isOpen = true;
            public Rect rect;
            public bool isChange = false;
            public TreeNode root
            {
                get
                {
                    if (this.obj == null)
                        return null;
                    if (this.obj is GTimelineStyle)
                        return this;
                    return this.parent.root;
                }
            }
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
                //if (data is GTriggerTimelineStyle)
                //{
                //    string tName = (data as GTriggerTimelineStyle).styleRes;
                //    if (!object.Equals(this.parent, null) && tName == this.parent.name)
                //    {
                //        Debug.LogError("错误的名字，timeLineName不能是自己");
                //        return null;
                //    }
                //    if(string.IsNullOrEmpty(tName) || !allDic.ContainsKey(tName))
                //    {
                //        node = Add(data, data.Attr.name + "[" + tName + "]");
                //        return node;
                //    }
                //    GTimelineStyle ts = allDic[tName];
                //    node = Add(data, data.Attr.name + "[" + tName + "]");
                //    node.Add(ts, depth);
                //}
                //else
                //{
                    node = Add(data, data.Attr.name);
                //}
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

            public void SetEvent(GEvent tl)
            {
                this.evt = tl;
                for(int i=0;i<this.childs.Count;i++)
                {
                    this.childs[i].SetEvent(tl.GetEvents()[i]);
                }
            }
        }
        public static void Save(GTimelineStyle tls)
        {
            string json = JsonConvert.SerializeObject(tls, Newtonsoft.Json.Formatting.Indented, getSetting());
            //string json = GJsonInfo.Serialize(tls);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(skillDir + tls.name + Suffix, false, new System.Text.UTF8Encoding(false));
            sw.Write(json);
            sw.Flush();
            sw.Close();
            sw.Dispose();
            if(Inst != null)
            {
                Inst.rootNode.isChange = false;
                Inst.Repaint();
            }
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
                try
                {
                    GTimelineStyle ps = JsonConvert.DeserializeObject(json, typeof(GTimelineStyle), getSetting()) as GTimelineStyle;// GJsonInfo.DeSerialize(json) as GTimelineStyle;
                    if (ps != null)
                        list.Add(ps);
                    bool b = RefreshStyle(ps, false);
                    if (b)
                    {
                        Save(ps);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                    Debug.LogError(json);
                }
            }
            return list;
        }
        public static bool RefreshStyle(GEventStyle style,bool b)
        {
            for(int i= style.styles.Count-1; i>=0;i--)
            {
                if (style.styles[i].Attr.Obsolete)
                {
                    style.styles.RemoveAt(i);
                    b = true;
                }
                else
                    b |= RefreshStyle(style.styles[i], b);
            }
            return b;
        }
        public static JsonSerializerSettings getSetting()
        {
            JsonSerializerSettings setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore };
            return setting;
        }
    }

}