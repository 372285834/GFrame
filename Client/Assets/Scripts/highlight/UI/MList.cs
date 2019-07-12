using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
namespace highlight
{
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(GridLayoutGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class MList : MonoBehaviour,ISerializeField
    {
        /// <summary>
        /// 列表的模板
        /// </summary>
        public GameObject goListPre;
        /// <summary>
        /// 列表对象数量
        /// 调用UpodateList 传进来的列表数量
        /// </summary>
        [Range(0,50)]
        public int ListNums;
        /// <summary>
        /// 列表对象GameObject
        /// </summary>
        public List<IUIItem> listItem = new List<IUIItem>(0);
        private IList dataArr;
        public bool AutoSize = false;
        public bool FixItemSize = true;
        public bool IsMultipleSelect = false;
        GridLayoutGroup grid;
        public GridLayoutGroup Grid
        {
            get { return grid ?? (grid = GetComponent<GridLayoutGroup>()); }
        }
        public RectTransform rectTransform
        {
            get { return this.transform as RectTransform; }
        }
        private RectTransform m_parent;
        public RectTransform parentRectTransform
        {
            get { return m_parent ?? (m_parent = this.transform.parent.GetComponent<RectTransform>()); }
        }
        /// <summary>
        /// 当列表为空的时候的提示
        /// </summary>
        public GameObject goNullTip;

        //private string mLuaName = "mlist";                                       //Lua接管文件名

        private bool IsInit = false;
        public Action OnUpdateIndex;
        public Action<IUIItem> OnClickItem;

        public delegate void onUpdateEvent(MList list, IUIItem item, object param);
        public onUpdateEvent onSelectItem = null;
        public onUpdateEvent onUpdateItem = null;
        public delegate Vector2 onFixSizeListFunc(int idx);
        public onFixSizeListFunc onFixSize = null;
        void initListData()
        {
            if (this.IsInit)
                return;
            if (OnUpdateIndex == null)
                OnUpdateIndex = updateIndex;
            IsInit = true;
            this.listItem.Clear();
            //mUIBaseLua = null;
            //luaData = null;
            if (goListPre == null)
            {
                if (this.transform.childCount > 0)
                {
                    goListPre = this.transform.GetChild(0).gameObject;
                }
            }
            if (goListPre != null)
            {
                InitPreGo(goListPre);
            }
            int length = rectTransform.childCount;
            for (int j = 0; j < length; j++)
            {
                AddItem(rectTransform.GetChild(j).gameObject);
            }
        }
        public void InitPreGo(GameObject go)
        {
            //this.goListPre.name
            Transform trans = go.transform;
            if (trans.parent != this.transform)
            {
                return;
            }
            if (FixItemSize)
            {
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.Reset();
                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
        }
        public void AddItem(GameObject go)
        {
            //RectTransform rt = go.transform as RectTransform;
            IUIItem item = go.GetComponent<IUIItem>();
            //IUIItem uiFun = go.GetComponent<IUIItem>();
            //item.Init(uiFun);
            item.name = "item_" + this.listItem.Count;
            item.mList = this;
            item.Order = this.listItem.Count;
            //item.SetSelectForceChange(false);
            item.Visible = false;
            item.Awake();
            this.listItem.Add(item);
            // OnUpdateIndex();
        }
        void updateIndex()
        {
            for (int i = 0; i < this.listItem.Count; i++)
            {
                IUIItem it = this.listItem[i];
                it.Index = it.Order;
            }
            if (this.Grid != null && this.Grid.enabled)
            {
                //MUtil.BeginSample("CalculateLayoutInputHorizontal");
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                // this.Grid.CalculateLayoutInputHorizontal();
                //MUtil.EndSample();
            }
        }
        public void Init(IList list)
        {
            this.Init(list, null);
        }
        public void Init(IList list, onUpdateEvent updateFunc)
        {
            this.Init(list, updateFunc, null, null);
        }
        public void Init(IList list, onUpdateEvent updateFunc, onUpdateEvent clickFunc, onFixSizeListFunc fixFunc)
        {
            int num = list != null ? list.Count : 0;
            SetData(list, updateFunc, clickFunc, fixFunc);
            this.CreateList(num);
        }
        public void SetData(IList list, onUpdateEvent updateFunc, onUpdateEvent clickFunc, onFixSizeListFunc fixFunc)
        {
            int num = list != null ? list.Count : 0;
            this.dataArr = list;
            this.onUpdateItem = updateFunc;
            if (clickFunc != null)
                this.onSelectItem = clickFunc;
            FixSizeListByCustom(num, fixFunc);
        }
        public void FixSizeListByCustom(int Nums, onFixSizeListFunc fixFunc)
        {
            if (fixFunc != null && Nums > 0)
            {
                this.ClearWHList();
                for (int i = 0; i < Nums; i++)
                {
                    Vector2 size = fixFunc(i);
                    this.LuaAddWH(size.x, size.y);
                }
                this.SetWHList();
            }
        }
        /// <summary>
        /// 创建列表
        /// </summary>
        /// <param name="Nums">数量</param>
        public void CreateList(int Nums)
        {
            if (listItem == null)
                return;
            //if (!IsInit)
            //    Debug.LogError("list-----------------" + gameObject.name + ".IsInit:" + this.IsInit);
            initListData();
            //this.Visible = Nums > 0;
            ListNums = Nums;
            //Itemtag = mTag;
            for (int i = this.listItem.Count; i < ListNums; i++)
            {
                GameObject item = Instantiate(goListPre, this.gameObject.transform) as GameObject;
                //item.transform.SetParent(this.gameObject.transform);
                InitPreGo(item);
                AddItem(item);
            }
            OnUpdateIndex();
            Refresh();
            UpdateSize();
        }
        public void Refresh()
        {
            this.gameObject.SetActive(this.ListNums != 0);
            if (this.goNullTip != null)
                this.goNullTip.SetActive(this.ListNums == 0);
            if (this.ListNums == 0)
                return;
            for (int i = 0; i < listItem.Count; i++)
            {
                IUIItem item = listItem[i];
                bool visible = item.Order < ListNums;
                item.Visible = visible;
                if (visible)
                {
                    UpdateLuaItem(item);
                }
            }
        }
        public void UpdateLuaItem(IUIItem item)
        {
            if (item == null)
                return;
            try
            {
                if(dataArr != null)
                    item.param = dataArr[item.Index];
                if (onUpdateItem != null)
                    onUpdateItem(this, item, item.param);
                else
                    item.OnUpdate();
                item.ChangeColor();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
            //UnityEngine.Profiling.Profiler.BeginSample("mlist.UpdateLuaItem");
            // LuaDelegate.UpdateListItem.Call(this, item);
            // GameManager.uluaMgr.UpdateLuaItem.Call(this, item);
            //UnityEngine.Profiling.Profiler.EndSample();
        }
        public List<Rect> ItemPosList { get { return mItemPosList; } }
        private List<Rect> mItemPosList = null;
        private List<Vector2> mLuaWHList = new List<Vector2>();
        public void LuaAddWH(float x, float y)
        {
            float fxCustom = x;
            float fyCustom = y;
            if (x <= 0)
                fxCustom = this.Grid.cellSize.x;
            if (y <= 0)
                fyCustom = this.Grid.cellSize.y;
            mLuaWHList.Add(new Vector2(fxCustom, fyCustom));
        }
        public void ClearWHList() { mLuaWHList.Clear(); }
        public void SetWHList()
        {
            if (this.Grid == null)
                return;
            //mwhList = list;
            mSize = new Vector2(Grid.padding.left, Grid.padding.top);
            if (mLuaWHList != null && mLuaWHList.Count > 0)
            {
                if (mItemPosList == null)
                    mItemPosList = new List<Rect>();
                mItemPosList.Clear();
                for (int i = 0; i < mLuaWHList.Count; i++)
                {

                    Vector2 wh = mLuaWHList[i];
                    if (this.Grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                    {
                        //mSize.y = wh.y;
                        mSize.x += wh.x;
                        if (i > 0)
                            mSize.x += grid.spacing.x;
                    }
                    if (this.Grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                    {
                        //mSize.x = wh.x;
                        mSize.y += wh.y;
                        if (i > 0)
                            mSize.y += grid.spacing.y;
                    }
                    Vector2 pos = new Vector2(mSize.x, -mSize.y);
                    //ip.Position =  new Vector2(mSize.x - 0.5f * wh.x, -mSize.y + 0.5f * wh.y);
                    //ip.Size = wh;
                    Rect ip = new Rect(pos, wh);
                    //ip.Min = new Vector2(ip.Position.x - 0.5f * ip.Size.x, ip.Position.y + 0.5f * ip.Size.y);
                    mItemPosList.Add(ip);
                }
            }
        }
        public int GetIndex(Vector2 pos, bool isHorizontality, bool isTrunOver = false)
        {
            if (this.ItemPosList == null)
                return 0;
            int num = 0;
            Vector2 si = Vector2.zero;
            pos.x = Mathf.Abs(pos.x);
            pos.y = Mathf.Abs(pos.y);
            if (isTrunOver)
                pos = this.Size - pos;
            int count = this.ItemPosList.Count;
            for (int i = 0; i < count; i++)
            {

                if (isHorizontality)
                {
                    if (pos.x > Mathf.Abs(this.ItemPosList[i].xMax))
                        num++;
                    else
                        break;
                }
                else
                {
                    if (pos.y > Mathf.Abs(this.ItemPosList[i].yMin))
                        num++;
                    else
                        break;
                }
            }
            num = isTrunOver ? count - num - 1 : num;
            return num;
        }
        public void UpdateSize()
        {
            if (AutoSize)
                this.rectTransform.sizeDelta = this.Size;
            FixItemPosition();
        }
        public void FixItemPosition()
        {
            if (!FixItemSize)
                return;

            for (int i = 0; i < this.listItem.Count; i++)
            {
                IUIItem it = this.listItem[i];
                if (!it.Visible)
                    continue;
                it.Size = this.CellSize(it.Index);
                it.GetComponent<RectTransform>().anchoredPosition = this.CellPosition(it.Index);
            }
        }
        int mGridNum = 0;
        public void SetGridNum(int num)
        {
            mGridNum = num;
        }
        private Vector2 mSize = Vector2.zero;
        public Vector2 Size
        {
            get
            {
                Vector2 size = this.rectTransform.rect.size;
                if (ItemPosList != null)
                {
                    return mSize;
                }
                if (this.Grid != null)
                {
                    int num = mGridNum > 0 ? mGridNum : this.ListNums;
                    size.x = this.Grid.GetWidth(num);
                    size.y = this.Grid.GetHeight(num);
                }
                return size;
            }
        }
        public Vector2 CellSize(int idx = -1)
        {
            if (this.ItemPosList != null && idx >= 0 && idx < this.ItemPosList.Count)
            {
                return this.ItemPosList[idx].size;
            }
            if (this.Grid != null)
                return this.Grid.cellSize;

            return Vector2.zero;
        }
        public Vector2 CellPosition(int idx = 0)
        {
            if (this.ItemPosList != null && idx >= 0 && idx < this.ItemPosList.Count)
            {
                return this.ItemPosList[idx].center;
            }
            if (this.Grid != null)
                return this.Grid.GetPosByIndex(idx);// this.Grid.GetPosByIndex(idx);

            return Vector2.zero;
        }

        public virtual bool Contains(GameObject go)
        {
            return go.transform.parent == this.transform;
        }
        public bool IsInParentRect(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            Vector3 pos = rt.localPosition + this.rectTransform.localPosition;
            //Vector2 pos = this.parentRectTransform.GetTargetLocalPoiont(ui.rectTransform);
            //Vector3 pos = this.rectTransform.localPosition + ui.rectTransform.localPosition;

            Vector3 min = parentRectTransform.rect.min + rt.rect.min;
            Vector3 max = parentRectTransform.rect.max + rt.rect.max;
            if (pos.x > max.x || pos.y > max.y || pos.x < min.x || pos.y < min.y)
            {
                return false;
            }
            return true;
        }
        private int mCurIndex = -1;
        public int CurIndex { get { return mCurIndex; } }
        public IUIItem mSelected
        {
            get
            {
                if (IsMultipleSelect)
                    return null;
                IUIItem item = GetItemByIndex(mCurIndex);
                return item != null && item.Visible ? item : null;
            }
        }
        private int mLastIndex = -1;
        public IUIItem lastItem
        {
            get
            {
                if (IsMultipleSelect)
                    return null;
                IUIItem item = GetItemByIndex(mLastIndex);
                return item != null && item.Visible ? item : null;
            }
        }
        public int Count { get { return this.ListNums; } }
        public void ClickItem(IUIItem item)
        {
            if (OnClickItem != null)
                OnClickItem(item);
            else
                SetSelect(item.Index, true);
        }
        public bool SetSelect(int idx, bool force = false)
        {
            try
            {
                IUIItem _lastItem = mSelected;
                IUIItem item = GetItemByIndex(idx);
                //SelectItem(item, sendEvent);
                mLastIndex = mCurIndex;
                mCurIndex = idx;
                bool isUpdateNew = force;
                if (_lastItem != item)
                {
                    if(!IsMultipleSelect)
                        UpdateLuaItem(_lastItem);
                    isUpdateNew = true;
                }
                if (isUpdateNew && item != null)
                {
                    if (!IsMultipleSelect)
                        UpdateLuaItem(item);
                    if (onSelectItem != null)
                    {
                        onSelectItem(this, item, item.param);
                    }
                    else
                    {
                        item.OnSelectItem();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
            return true;
        }
        public IUIItem GetItemByIndex(int idx)
        {
            return this.listItem.Find(x => x.Index == idx);
        }
        public IUIItem GetItemByOrder(int order)
        {
            return this.listItem.Find(x => x.Index == order);
        }
        public IUIItem FindItem(Transform trans)
        {
            return this.listItem.Find(x => x.transform == trans);
        }
        public IUIItem FindItem(Predicate<IUIItem> match)
        {
            return this.listItem.Find(match);
        }
        public int FindIndex(Transform trans)
        {
            return this.listItem.FindIndex(x => x.transform == trans);
        }
        //private bool visible;
        public virtual bool Visible
        {
            get
            {
                return this.gameObject.activeInHierarchy;
            }
            set
            {
                //visible = value;
                if (this.gameObject.activeInHierarchy == value) return;
                this.gameObject.SetActive(value);
            }
        }

        public int nextIdx
        {
            get
            {
                int idx = this.CurIndex + 1;
                if (idx >= this.Count)
                    idx = 0;
                return idx;
            }
        }
        public int lastIdx
        {
            get
            {
                int idx = this.CurIndex - 1;
                if (idx < 0)
                    idx = this.Count - 1;
                return idx;
            }
        }
        public int GetIndexByRect(Vector2 screenPos,Camera ca,float dis)
        {
            Vector2 lt = Vector2.zero;
            float cur = Int32.MaxValue;
            int result = -1;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, screenPos, ca, out lt);
            for (int i=0;i<this.listItem.Count;i++)
            {
                IUIItem item = this.listItem[i];
                float value = Vector2.Distance(lt, item.rectTransform.localPosition);
                if(cur > value && dis > value)
                {
                    cur = value;
                    result = i;
                }
            }
            if(result >= 0)
            {
               // Debug.Log("result:" + result + "," + cur);
            }
            return result;
        }
        protected virtual void OnDestroy()
        {
            if (listItem != null)
            {
                listItem.Clear();
                listItem = null;
            }
            if (mItemPosList != null)
            {
                mItemPosList.Clear();
                mItemPosList = null;
            }
            mLuaWHList.Clear();
            OnUpdateIndex = null;
            grid = null;
            //Itemtag = null;
            goNullTip = null;
            // mLuaName = null;
            //ShowItemObj = null;
            m_parent = null;
            goListPre = null;
        }
        public GridLayoutGroup.Corner startCorner { get { return Grid.startCorner; } }
        public void SerializeFieldInfo()
        {
            if (goListPre == null)
            {
                if (this.transform.childCount > 0)
                {
                    goListPre = this.transform.GetChild(0).gameObject;
                    InitPreGo(goListPre);
                }
            }
            mItemPosList = null;
            this.grid = GetComponent<GridLayoutGroup>();
            if(this.transform.parent != null)
            {
                ISerializeField iserial = this.transform.parent.GetComponent<ISerializeField>();
                if (iserial != null)
                    iserial.SerializeFieldInfo();
            }
            
            //  resetContent();
            if (goListPre != null)// && FixItemSize
            {
                if (ListNums <= 0)
                    ListNums = 1;
                if (goListPre.transform.parent == this.transform)
                    goListPre.transform.SetAsFirstSibling();
                for (int i = this.transform.childCount; i < ListNums; i++)
                {
                    GameObject item = Instantiate(goListPre, this.gameObject.transform) as GameObject;
                    //item.transform.SetParent(this.gameObject.transform);
                    InitPreGo(item);
                    item.hideFlags = HideFlags.DontSave;
                }
                for (int i = this.transform.childCount - 1; i >= 0; i--)
                {
                    Transform trans = this.transform.GetChild(i);
                    if (trans.hideFlags == HideFlags.DontSave || trans.hideFlags == HideFlags.HideAndDontSave)
                    {
                        //trans.gameObject.SetActive(i < ListNums);
                        if (i >= ListNums)
                            GameObject.DestroyImmediate(trans.gameObject);
                    }
                }
                //for (int i = 0; i < this.transform.childCount; i++)
                //{
                //    RectTransform rect = this.transform.GetChild(i) as RectTransform;
                //    rect.sizeDelta = this.CellSize(i);
                //    rect.anchoredPosition = this.CellPosition(i);
                //}
            }
            this.UpdateSize();
            if (FixItemSize)
            {
                for (int i = 0; i < this.transform.childCount; i++)
                {
                    IUIItem it = this.transform.GetChild(i).GetComponent<IUIItem>();
                    it.Size = this.CellSize(i);
                    it.GetComponent<RectTransform>().anchoredPosition = this.CellPosition(i);
                }
            }
            
            //DrivenTransformProperties driven = DrivenTransformProperties.AnchoredPositionZ | DrivenTransformProperties.Rotation | DrivenTransformProperties.Scale;// | DrivenTransformProperties.Anchors | DrivenTransformProperties.Pivot;
            DrivenTransformProperties driven = DrivenTransformProperties.None;
            if (this.AutoSize)
            {
                driven = DrivenTransformProperties.SizeDelta;
            }
            this.rectTransform.AddDriven(driven);
        }
    }

    //public struct ItemPosition
    //{
    //    public Vector2 Size;
    //    public Vector2 Position;
    //    public Vector2 Min;
}