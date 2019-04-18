using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace highlight.tl
{
    [CanEditMultipleObjects, CustomEditor(typeof(TimeNode))]
    [ExecuteInEditMode]
    public class TimeNodeEditor : Editor
    {
        private const string FRAMERANGE_START_FIELD_ID = "FrameRange.Start";
        TimeNode mScript;
        TimeStyle style;
        TimeObject obj;
        List<Type> list = new List<Type>();
        List<string> strList = new List<string>();
        public static bool showData = true;
        public static bool showAction = true;
        public override void OnInspectorGUI()
        {
            mScript = target as TimeNode;
            style = mScript.style;
            obj = mScript.obj;
            if (obj == null || style == null)
            {
           //     TimeWindow.ShowEditor();
               // Transform t = this.mScript.transform.parent == null ? this.mScript.transform : this.mScript.transform.root;
              //  GameObject.DestroyImmediate(t.gameObject);
                return;
            }
            if (mScript.Depth != obj.Depth || mScript.index != obj.index)
            {
                Debug.LogError(string.Format("位置错误：Depth【{0},{1}】, index【{2},{3}】", mScript.Depth, obj.Depth, mScript.index, obj.index));
            }
            GUILayout.BeginVertical();
            ShowMenu();
            drawRang();
            string arr2 = showData ? "Hide" : "Show";
            List<ComponentData> comps = mScript.obj.ComponentList;
            if (GUILayout.Button("数据(" + comps.Count + ")" + arr2))
                showData = !showData;
            if (showData)
            {
                for (int i = 0; i < comps.Count; i++)
                {
                    drawTitle(comps[i].style.Attr.name, comps[i], comps.Count - 1);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4f);
                    GUILayout.BeginVertical();

                    DrawSeparator(0, drawComponent, comps[i]);

                    // drawComponent(comps[i]);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
            }
            GUILayout.Space(10f);
            // DrawSeparator(2);
            // GUILayout.Space(5f);
            List<TimeAction> Actions = mScript.obj.ActionList;
            string arr = showAction ? "Hide" : "Show";
            if (GUILayout.Button("行为(" + Actions.Count + ")" + arr))
                showAction = !showAction;
            //showAction = EditorGUILayout.Foldout(showAction, "行为", false);
            if (showAction)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    drawTitle(Actions[i].style.Attr.name, Actions[i], Actions.Count - 1);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4f);
                    GUILayout.BeginVertical();
                    DrawSeparator(0, drawAction, Actions[i]);
                    // drawAction(Actions[i]);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
            }
            GUILayout.EndVertical();
        }
        void ShowMenu()
        {
            if (Event.current != null && Event.current.button == 1 && Event.current.type <= EventType.MouseUp)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                list.Clear();
                GenericMenu menu = new GenericMenu();
                Dictionary<Type, TimeAttribute> compAttrDic = ComponentStyle.compAttrDic;
                foreach (KeyValuePair<Type, TimeAttribute> kv in compAttrDic)
                {
                    if(obj.resData != null && kv.Key == typeof(ResStyle))
                    {
                        continue;
                    }
                    menu.AddItem(new GUIContent(kv.Value.menu), false, excMenuComponent, kv);
                }
                Dictionary<Type, ActionAttribute> actionAttrDic = ActionStyle.actionAttrDic;
                foreach (KeyValuePair<Type, ActionAttribute> kv in actionAttrDic)
                {
                    menu.AddItem(new GUIContent(kv.Value.menu), false, excMenuAction, kv);
                }

                menu.ShowAsContext();
                Event.current.Use();
            }
        }
        void excMenuComponent(object param)
        {
            KeyValuePair<Type, TimeAttribute> kvp = (KeyValuePair<Type, TimeAttribute>)param;
            ComponentStyle style = Activator.CreateInstance(kvp.Key) as ComponentStyle;
            this.mScript.AddComponent(style);
        }
        void excMenuAction(object param)
        {
            KeyValuePair<Type, ActionAttribute> kvp = (KeyValuePair<Type, ActionAttribute>)param;
            ActionStyle style = new ActionStyle(kvp.Key, kvp.Value);
            this.mScript.AddAction(style);
        }
        void drawTitle(string name, Object obj, int maxIdx)
        {
            TimeAction action = obj as TimeAction;
            ComponentData data = obj as ComponentData;
            int index = action != null ? action.index : data.index;

            GUILayout.Space(2f);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.yMin += 5f;
            rect.width = 100f;
            rect.height = 18f;
            GUI.Label(rect, "==" + name + " " + index);
            rect.yMin -= 1f;
            rect.xMin += 105f;
            rect.width = 20f;
            rect.height = 16f;
            if (index > 0)
            {
                if (GUI.Button(rect, "↑"))
                {
                    if (action != null)
                        this.mScript.SetActionIndex(action, index - 1);
                    else
                        this.mScript.SetComponentIndex(data, index - 1);
                    this.Repaint();
                }
                rect.xMin += 21f;
                rect.width = 20f;
            }
            if(index < maxIdx)
            {
                if (GUI.Button(rect, "↓"))
                {
                    if (action != null)
                        this.mScript.SetActionIndex(action, index + 1);
                    else
                        this.mScript.SetComponentIndex(data, index + 1);
                    this.Repaint();
                }
            }
            
            //GUILayout.Label(Actions[i].Attr.name);
            rect.xMin = Screen.width - 35f;
            rect.width = 20f;
            rect.height = 15f;
            if (GUI.Button(rect, "-"))
            {
                if (action != null)
                    this.mScript.RemoveAction(action);
                else
                    this.mScript.RemoveComponent(data);
                this.Repaint();
            }
            GUILayout.Space(20f);
        }
        void drawRang()
        {
            //GUILayout.Label("id:" + style.id);
            FrameRange rang = style.Range;
            FrameRange validRange = mScript.obj.GetMaxFrameRange();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Range    ");
            GUILayout.Label("S:", EditorStyles.label);
            GUI.SetNextControlName(FRAMERANGE_START_FIELD_ID);
            rang.Start = EditorGUILayout.IntField(style.x);
            GUILayout.Label("E:", EditorStyles.label);
            rang.End = EditorGUILayout.IntField(style.y);
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("t:" + mScript.obj.LengthTime + "s", GUILayout.Width(30f));
            GUILayout.Space(20f);//EditorGUIUtility.labelWidth
            GUILayout.Label(validRange.Start.ToString(), GUILayout.Width(30f));
            float sliderStartFrame = rang.Start;
            float sliderEndFrame = rang.End;
            EditorGUILayout.MinMaxSlider(ref sliderStartFrame, ref sliderEndFrame, validRange.Start, validRange.End);
            GUILayout.Label(validRange.End.ToString(), GUILayout.Width(30f));
            EditorGUILayout.EndHorizontal();

            rang = FrameRange.Resize((int)sliderStartFrame, (int)sliderEndFrame, validRange);
            if (rang != style.Range)
            {
                mScript.root.isChange = true;
                if (TimeWindow.Inst == null)
                {
                    TimeWindow.ShowEditor();
                }
                TimeWindow.Inst.Repaint();
            }
            style.Range = rang;
            mScript.obj.OnStyleChange();
        }
        void drawComponent(object obj)
        {
            ComponentData data = obj as ComponentData;
            ComponentStyle comp = data.style;
            MethodInfo method = comp.GetType().GetMethod("OnInspectorGUI");
            if (method != null)
            {
                method.Invoke(comp, null);
            }
        }
        static List<int> tempList = new List<int>();
        static List<string> tempStrList = new List<string>();
        void drawAction(object obj)
        {
            TimeAction action = obj as TimeAction;
            ActionStyle actionComp = action.style;
            FieldInfo[] fls = actionComp.Attr.Infos;
            if (actionComp.Indexs == null || actionComp.Indexs.Length != fls.Length)
            {
                actionComp.Indexs = new int[fls.Length];
            }

            for (int i = 0; i < fls.Length; i++)
            {
                tempList.Clear();
                tempStrList.Clear();
                List<ComponentData> list = mScript.obj.ComponentList;
                for (int j = 0; j < list.Count; j++)
                {
                    ComponentData data = list[j];
                    //if(comp is fls[i].FieldType)
                    if (fls[i].FieldType.IsAssignableFrom(data.GetType()))
                    //    if(fls[i].FieldType.IsInstanceOfType(data))
                    //if (fls[i].FieldType.IsSubclassOf(data.GetType()))
                    {
                        tempStrList.Add(data.style.Attr.name + " " + j);
                        tempList.Add(j);
                    }
                }
                string key = actionComp.Attr.infoDesDic[fls[i]];
                if (tempStrList.Count == 0)
                {
                    actionComp.Indexs[i] = -1;
                    GUILayout.Label(key);
                }
                else
                    actionComp.Indexs[i] = EditorGUILayout.IntPopup(key, actionComp.Indexs[i], tempStrList.ToArray(), tempList.ToArray());
            }
        }


        static public void DrawSeparator(float h, Action<object> ac = null, object param = null)
        {
            GUILayout.Space(3f);

            Rect rect = GUILayoutUtility.GetLastRect();
            if (ac != null)
            {
                ac(param);
                Rect rect2 = GUILayoutUtility.GetLastRect();
                h = rect2.yMax - rect.yMin;
            }
            if (UnityEngine.Event.current.type == UnityEngine.EventType.Repaint)
            {
                Texture2D tex = EditorGUIUtility.whiteTexture;
                Color co = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.1f);
                GUI.DrawTexture(new Rect(5f, rect.yMin + 0f, Screen.width, 2f), tex);
                GUI.DrawTexture(new Rect(5f, rect.yMin + 0f, Screen.width, h + 4f), tex);
                GUI.DrawTexture(new Rect(5f, rect.yMin + h + 0f, Screen.width, 2f), tex);
                GUI.color = co;
            }
        }


    }
}