using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace highlight
{
    public class UIManager : MonoBehaviour
    {
        public UIStyle mStyle;
        public Camera mCamera;
        public EventSystem eventSystem;
        public Canvas SceneCanvas;
        public Canvas PanelCanvas;
        public Canvas PopCanvas;
        public Canvas EnableCanvas;
        public Canvas TopCanvas;
        public class UIData
        {
            public UIData(UINameType type, IUIObject sPanel)
            {
                eName = type;
                name = eName.ToString();
                source = sPanel;
            }
            public eUIType eType { get { return source.eType; } }
            public eQueueType eRankType { get { return source.eRankType; } }
            public UINameType eName;
            public string name;
            public IUIObject source;
            public IUIObject panel;
            public IUIObject CreatPanel(Transform parent)
            {
                GameObject go = GameObject.Instantiate(this.source.gameObject);
                go.name = this.source.name;
                this.panel = go.GetComponent<IUIObject>();
                if (parent == null)
                {
                    switch (eType)
                    {
                        case eUIType.Scene:
                            parent = Inst.SceneCanvas.transform;
                            break;
                        case eUIType.Panel:
                            parent = Inst.PanelCanvas.transform;
                            break;
                        case eUIType.Popup:
                            parent = Inst.PopCanvas.transform;
                            break;
                        case eUIType.Top:
                            parent = Inst.TopCanvas.transform;
                            break;
                        case eUIType.View:
                            break;
                        default:
                            break;
                    }
                }
                if (parent != null)
                    go.transform.SetParent(parent);
                this.panel.ResetRectTransform();
                return panel;
            }
        }
        public static Dictionary<UINameType, UIData> UIDic = new Dictionary<UINameType, UIData>();
        public static UIManager Inst;
        //public static Dictionary<string, Transform> nodeDic = new Dictionary<string, Transform>();
        public void Awake()
        {
            Inst = this;
            //for(int i=0;i<this.transform.childCount;i++)
            //{
            //    Transform node = this.transform.GetChild(i);
            //    nodeDic[node.name] = node;
            //}
            for (int i = 0; i < mStyle.prefabs.Length; i++)
            {
                if (mStyle.prefabs[i] != null)
                {
                    try
                    {
                        UINameType t = (UINameType)Enum.Parse(typeof(UINameType), mStyle.prefabs[i].name);
                        UIDic[t] = new UIData(t, mStyle.prefabs[i].GetComponent<IUIObject>());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("InitUIDataError: " + e.Message + ",     name:" + mStyle.prefabs[i].name);
                    }

                }
            }
        }
        public static UIData GetData(UINameType t)
        {
            UIData data = null;
            UIDic.TryGetValue(t, out data);
            return data;
        }
        public static Queue<IUIObject> SceneQueue = new Queue<IUIObject>();
        public static Queue<IUIObject> PanelQueue = new Queue<IUIObject>();
        public static IUIObject CurPanel;
        public static IUIObject CurScene;
        public static IUIObject Show(UINameType t, object param = null)
        {
            UIData data = GetData(t);
            if (data == null)
                return null;
            if (data.panel == null)
            {
                if (data.source == null)
                    return null;
                data.CreatPanel(null);
            }
            if (data.panel != null)
            {
                if (data.eRankType == eQueueType.Queue)
                {
                    IUIObject last = null;
                    if (data.eType == eUIType.Panel)
                    {
                        last = PanelQueue.Count > 0 ? PanelQueue.Peek() : null;
                        PanelQueue.Enqueue(data.panel);
                    }
                    else if (data.eType == eUIType.Scene)
                    {
                        last = SceneQueue.Count > 0 ? SceneQueue.Peek() : null;
                        SceneQueue.Enqueue(data.panel);
                    }
                    if (last != null)
                        last.OnQueueChange(false, data.panel);
                }
                if (data.eType == eUIType.Panel)
                {
                    if (data.panel.eRankType == eQueueType.Only)
                        CurPanel = SetUIObject(CurPanel, data.panel);

                }
                else if (data.eType == eUIType.Scene)
                {
                    if (data.panel.eRankType == eQueueType.Only)
                        CurScene = SetUIObject(CurScene, data.panel);
                }
                data.panel.Show(param);
            }
            return data.panel;
        }

        public static IUIObject SetUIObject(IUIObject cur, IUIObject to)
        {
            if (cur != null)
            {
                cur.Close();
            }
            return to;
        }
        public static void Close(UINameType t)
        {
            UIData data = GetData(t);
            if (data == null)
                return;
            if (data.panel == null || !data.panel.Visible)
            {
                return;
            }
            data.panel.Close();
            if (data.eRankType == eQueueType.Queue)
            {
                IUIObject last = null;
                if (data.eType == eUIType.Panel)
                {
                    PanelQueue.Dequeue();
                    last = PanelQueue.Count > 0 ? PanelQueue.Peek() : null;
                }
                else if (data.eType == eUIType.Scene)
                {
                    SceneQueue.Dequeue();
                    last = SceneQueue.Count > 0 ? SceneQueue.Peek() : null;
                }
                if (last != null)
                    last.OnQueueChange(true, data.panel);
            }
            else if (data.eRankType == eQueueType.Only)
            {
                if (data.eType == eUIType.Panel)
                {
                    CurPanel = null;
                }
                else if (data.eType == eUIType.Scene)
                {
                    CurScene = null;
                }
            }
        }
        public static void MouseEnable(bool b)
        {
            Inst.eventSystem.gameObject.SetActive(b);
        }
        public static IUIObject Get(UINameType t)
        {
            UIData data = GetData(t);
            if (data != null)
                return data.panel;
            return null;
        }
        public static T Get<T>(UINameType t) where T : IUIObject
        {
            UIData data = GetData(t);
            if (data != null)
                return data.panel as T;
            return null;
        }
        public static IUIObject CreatView(UINameType t, Transform parent)
        {
            UIData data = GetData(t);
            if (data != null)
                return data.CreatPanel(parent);
            return null;
        }

#if UNITY_EDITOR
        public static void EditorUpdate()
        {

        }
#endif
    }
}