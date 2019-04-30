using highlight;
using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    public enum eUIType
    {
        Scene,
        Panel,
        Popup,
        Top,
        View,
    }
    public enum eQueueType
    {
        None,
        Only,
        Queue,
    }
    public class IUIBase : MonoBehaviour, ISerializeField
    {
        public bool Visible
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                gameObject.SetActive(value);
            }
        }
        public RectTransform rectTransform
        {
            get { return this.transform as RectTransform; }
        }
        public Vector2 Size
        {
            get
            {
                return this.rectTransform.sizeDelta;
            }
            set
            {
                this.rectTransform.sizeDelta = value;
            }
        }
        public object param;
        bool isInit = false;
        public void Awake()
        {
            if (isInit)
                return;
            isInit = true;
            OnAwake();
            InitEvent();
        }
        public virtual void OnAwake()
        {

        }
        public virtual void SerializeFieldInfo()
        {
            if (Application.isPlaying && isInit)
                return;
            Type ts = this.GetType();
            Dictionary<string, FieldInfo> tempDic = new Dictionary<string, FieldInfo>();
            FieldInfo[] pis = ts.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int j = 0; j < pis.Length; j++)
            {
                if (pis[j].GetValue(this) != null)
                    continue;
                tempDic[pis[j].Name] = pis[j];
            }
            MonoBehaviour[] monos = this.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < monos.Length; i++)
            {
                FieldInfo pi = null;
                tempDic.TryGetValue(monos[i].name, out pi);
                if (pi != null && pi.FieldType == monos[i].GetType())
                {
                    pi.SetValue(this, monos[i]);
                }
            }
        }
        protected virtual void InitEvent()
        {
            Type ts = this.GetType();
            FieldInfo[] pis = ts.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < pis.Length; i++)
            {
                MonoBehaviour mono = pis[i].GetValue(this) as MonoBehaviour;
                _InitEvent(mono);
            }
        }
        void _InitEvent(MonoBehaviour mono)
        {
            if (mono == null)
                return;
            if (!Application.isPlaying)
                return;
            MList list = mono as MList;
            if (list != null)
                list.onSelectItem = OnSelected;
            if (!(mono is Selectable))
                return;
            Button btn = mono as Button;
            if (btn != null)
            {
                btn.SetClick(delegate () { OnClick(btn); });
                return;
            }
            Toggle to = mono as Toggle;
            if (to != null)
            {
                to.SetToggle(delegate (bool b) { OnToggle(to, b); });
                return;
            }
            TMPro.TMP_Dropdown dpd = mono as TMPro.TMP_Dropdown;
            if (dpd != null)
            {
                dpd.onValueChanged.AddListener(delegate (int v) { OnDropDown(dpd, v); });
                return;
            }
            TMPro.TMP_InputField tmpInput = mono as TMPro.TMP_InputField;
            if (tmpInput != null)
            {
                tmpInput.onEndEdit.AddListener(delegate (string v) { OnEndEdit(tmpInput, v); });
                return;
            }
            InputField input = mono as InputField;
            if (input != null)
            {
                input.onEndEdit.AddListener(delegate (string v) { OnEndEdit(input, v); });
                return;
            }
        }
        public virtual void OnClick(Button btn)
        {
        }
        public virtual void OnToggle(Toggle to,bool b)
        {
        }
        public virtual void OnDropDown(TMPro.TMP_Dropdown dpd,int idx)
        {
        }
        public virtual void OnEndEdit(Selectable input, string v)
        {
        }
        public virtual void OnSelected(MList list,IUIItem item, object param)
        {
        }
    }
    public class IUIView : IUIBase
    {
        public eUIType eType = eUIType.Panel;
        public eQueueType eRankType = eQueueType.None;
        private Dictionary<Observer, AcHandler> obsDic;
        public virtual void OnShow() { }
        public virtual void OnClose() { }
        public virtual void Show(object param)
        {
            this.param = param;
            this.Visible = true;
            OnShow();
        }
        public void AddObserver(Observer obs, AcHandler ac, bool immediately = true)
        {
            if (obsDic == null)
                obsDic = new Dictionary<Observer, AcHandler>();
            obsDic[obs] = ac;
            obs.AddObserver(ac, immediately);
        }
        public void RemoveObserver(Observer obs, AcHandler ac)
        {
            if (obsDic.ContainsKey(obs))
            {
                obs.RemoveObserver(ac);
                obsDic.Remove(obs);
            }
        }
        public virtual void Close()
        {
            if (this.eType != eUIType.View)
            {
                IUIView[] iviews = this.GetComponentsInChildren<IUIView>();
                for (int i = 0; i < iviews.Length; i++)
                {
                    if (iviews[i].eType == eUIType.View)
                        iviews[i].Close();
                }
            }
            OnClose();
            Clear();
            this.Visible = false;
        }
        protected void Clear()
        {
            if (obsDic != null)
            {
                foreach (var obs in obsDic.Keys)
                {
                    obs.RemoveObserver(obsDic[obs]);
                }
                obsDic.Clear();
            }
        }
    }
    public class IUIObject : IUIView
    {
        public Button Close_btn;
        public override void Show(object param)
        {
            if (Close_btn != null)
                Close_btn.SetClick(Close);
            base.Show(param);
        }
        public virtual void OnQueueChange(bool v, IUIObject other)
        {
            this.Visible = v;
            //if (v)
            //    OnShow();
        }
        public override void SerializeFieldInfo()
        {
            base.SerializeFieldInfo();
            ResetRectTransform();
            this.rectTransform.AddDriven(DrivenTransformProperties.All);
        }
        public void ResetRectTransform()
        {
            this.transform.Reset();
            RectTransform rect = this.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
    }
    public class IUIItem : IUIBase
    {
        private bool _interactable = true;
        public bool interactable
        {
            get { return _interactable; }
            set
            {
                _interactable = value;
                ChangeColor();
            }
        }
        public MList mList { get; set; }
        public int Index;
        public int Order;
        [SerializeField]
        private float dCurveCenterOffset = 0.0f;
        public float CenterOffSet
        {
            get { return this.dCurveCenterOffset; }
            set { dCurveCenterOffset = value; }
        }
        public bool IsSelected
        {
            get
            {
                return mList == null ? false : mList.CurIndex == this.Index;
            }
        }
        protected void SetSelect()
        {
            if (mList != null)
                mList.ClickItem(this);
        }
        public virtual void OnUpdate()
        {

        }
        public virtual void ChangeColor(bool instant = true)
        {

        }
        public virtual void OnSelectItem()
        {

        }
        public override void SerializeFieldInfo()
        {
            base.SerializeFieldInfo();
            MList list = this.transform.parent.GetComponent<MList>();
            if(list != null)
            {
                if(list.FixItemSize)
                {
               //     this.rectTransform.AddDriven(DrivenTransformProperties.All);
                }
            }
        }
        // Update Item's status
        // 1. position
        // 2. scale
        // 3. "depth" is 2D or z Position in 3D to set the front and back item
        public void UpdateScrollViewItems(float xValue, float depthCurveValue, int depthFactor, float itemCount, float yValue, float scaleValue)
        {
            Vector3 targetPos = Vector3.zero;
            Vector3 targetScale = Vector3.one;
            // position
            targetPos.x = xValue;
            targetPos.y = yValue;
            // Set the "depth" of item
            if (!Application.isPlaying)
            {
                targetPos.z = depthCurveValue;
            }
            else
            {
                int newDepth = (int)(depthCurveValue * itemCount);
                this.transform.SetSiblingIndex(newDepth);
                //SetItemDepth(depthCurveValue, depthFactor, itemCount);
            }

            this.rectTransform.localPosition = targetPos;
            // scale
            targetScale.x = targetScale.y = scaleValue;
            rectTransform.localScale = targetScale;
        }
    }
    
    public static class UGUIExtends
    {
        public static void SetClick(this Button btn, UnityAction ac)
        {
            if (btn == null)
                return;
            btn.onClick.RemoveListener(ac);
            btn.onClick.AddListener(ac);
        }
        public static void SetToggle(this Toggle to, UnityAction<bool> ac)
        {
            if (to == null)
                return;
            to.onValueChanged.RemoveListener(ac);
            to.onValueChanged.AddListener(ac);
        }
    }

}