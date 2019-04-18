using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace highlight
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class LoopList : UIBehaviour, IBeginDragHandler, IEndDragHandler, ISerializeField
    {
        /// <summary>
        /// 列
        /// </summary>
        public int XColumn = 1;
        /// <summary>
        /// 行
        /// </summary>
        public int YRow = 1;
        /// <summary>
        /// 左右可以滑动的箭头
        /// </summary>
        public GameObject goArrowLeft;
        public GameObject goArrowRight;
        private Axis axis
        {
            get { return sRect.vertical ? Axis.Verticality : Axis.Horizontality; }
        }
        enum Axis
        {
            Horizontality,
            Verticality,
        }
        int m_PageNum = 0;
        public int pageNum
        {
            get { return m_PageNum; }
        }
        /// <summary>
        /// 滚动一个Index影响的item数量
        /// </summary>
        public int scrollNum
        {
            get
            {
                return this.axis == Axis.Verticality ? XColumn : YRow;
            }
        }
        private Vector2 scrollSize = Vector2.zero;
        int maxIndex = 0;
        public int MaxIndex
        {
            get { return maxIndex; }
        }
        public ScrollRect sRect;
        public MList TempList;
        public GridLayoutGroup.Corner startCorner { get { return TempList.startCorner; } }
        public GridLayoutGroup Grid
        {
            get { return TempList.Grid; }
        }
        // public bool OptimizeScrollIndex = false;
        public Action<bool> dragCall = null;
        //public MItem tempItem = null;
        public RectTransform rectTransform
        {
            get { return this.transform as RectTransform; }
        }
        public RectTransform Content
        {
            get { return this.sRect == null ? null : this.sRect.content; }
        }
        public bool isScroll
        {
            get
            {
                return sRect.velocity != Vector2.zero;
            }
        }
        bool isInit = false;
        void initLoopListData()
        {
            if (isInit)
                return;
            isInit = true;
            if (TempList == null || sRect == null)
            {
                Debug.LogError(string.Format("mTempList == {0} || sRect == {1}", TempList, sRect));
                return;
            }
            m_PageNum = this.YRow * this.XColumn;
            //sRect.onValueChanged.RemoveListener(updatePosition);
            //sRect.onValueChanged.AddListener(updatePosition);
            resetContent();
            TempList.AutoSize = true;
            TempList.FixItemSize = true;
            this.Grid.enabled = false;
            scrollSize = new Vector2(Grid.cellSize.x + Grid.spacing.x, Grid.cellSize.y + Grid.spacing.y);
        }
        private int mAllNum = 0;
        public int AllNum { get { return mAllNum; } }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list">数据表</param>
        /// <param name="aIndeTarget">滚动到目标index位置</param>
        /// <param name="updateFunc">更新函数</param>
        public void Init(IList list, int aIndeTarget = -1, MList.onUpdateEvent updateFunc = null)
        {
            Init(list, updateFunc, null, null, aIndeTarget);
        }
        public void Init(IList list, MList.onUpdateEvent updateFunc, MList.onUpdateEvent clickFunc = null, MList.onFixSizeListFunc fixFunc = null, int aIndeTarget = -1)
        {
            if (TempList == null || sRect == null)
                return;
            int num = list != null ? list.Count : 0;
            this.TempList.SetData(list, updateFunc, clickFunc, fixFunc);
            CreateList(num, aIndeTarget);
        }
        /// <summary>
        /// 初始化列表
        /// </summary>
        /// <param name="num">总数量</param>
        public void CreateList(int num, int aIndeTarget = -1)
        {
            //Debug.LogError(this.name + " num:" + num + ", index:" + index);
            //Profiler.BeginSample("LoopList.Init");
            initLoopListData();
            if (mAllNum > num)
            {
                mAllNum = num;
                resetContent();
            }
            mAllNum = num;
            int listNum = num > this.pageNum + scrollNum ? this.pageNum + scrollNum : num;
            maxIndex = Mathf.CeilToInt((float)num / scrollNum);
            maxIndex -= (axis == Axis.Verticality ? YRow : XColumn);
            maxIndex = maxIndex < 0 ? 0 : maxIndex;

            int newIdx = aIndeTarget < 0 ? this.Index : aIndeTarget / scrollNum;
            bool isChangeIndex = this.setIndex(newIdx);
            this.TempList.OnUpdateIndex = updateIndex;
            this.TempList.SetGridNum(num);
            //Profiler.EndSample();
            this.TempList.CreateList(listNum);
            if (aIndeTarget >= 0)
                this.SetScrollIndex(aIndeTarget);
            else if(isChangeIndex)
                this.SetScrollIndex(this.curIndex);

            bool have = num > 0;
            if (this.goArrowLeft != null)
                this.goArrowLeft.SetActive(have);
            if (this.goArrowRight != null)
                this.goArrowRight.SetActive(have);
            //tempItem = this.TempList.listItem[listNum - 1];
        }
        void updatePosition()
        {
            posIndex = GetIndex(this.Content.anchoredPosition);
            //move = this.curPos - this.rectTransform.anchoredPosition;
            //Debug.Log("------------"+pos+","+move);
            if (posIndex < 0 || posIndex == this.Index)
                return;
            //Debug.Log("num:" + num + ",index:" + this.curIndex);
            //if (curPos.x < move.x || curPos.y < move.y)
            cacheMoveList.Clear();
            if (posIndex > this.Index)
            {
                while (posIndex > this.Index)
                {
                    if (this.Index >= maxIndex - 1)
                    {
                        break;
                    }
                    this.curIndex++;
                    startMove(true);
                }
            }
            else
            {
                while (posIndex < this.Index)
                {
                    if (this.Index <= 0)
                    {
                        break;
                    }
                    this.curIndex--;
                    startMove(false);

                }
            }
            
            if (cacheMoveList.Count > 0)
            {
                updateIndex();
                this.TempList.FixItemPosition();
                for (int i = 0; i < cacheMoveList.Count; i++)
                {
                    this.TempList.UpdateLuaItem(cacheMoveList[i]);
                }
            }
            
        }
        private List<IUIItem> cacheMoveList = new List<IUIItem>(); 
        void startMove(bool _moveFirst = true)
        {
            for (int i = 0; i < scrollNum; i++)
            {
                IUIItem itm = _moveFirst ? moveFirstToLast() : moveLastToFirst();
                if (!cacheMoveList.Contains(itm))
                    cacheMoveList.Add(itm);
            }
        }
        IUIItem moveFirstToLast()
        {
            IUIItem itm = null;
            for (int j = 0; j < this.TempList.listItem.Count;j++)
            {
                IUIItem m = this.TempList.listItem[j];
                if (m.Order == 0)
                {
                    m.Order = this.TempList.listItem.Count - 1;
                    itm = m;
                }
                else
                    m.Order--;
            }
            return itm;
        }
        IUIItem moveLastToFirst()
        {
            IUIItem itm = null;
            for (int j = 0; j < this.TempList.listItem.Count; j++)
            {
                IUIItem m = this.TempList.listItem[j];
                if (m.Order == this.TempList.listItem.Count - 1)
                {
                    m.Order = 0;
                    itm = m;
                }
                else
                    m.Order++;
            }
            return itm;
        }

        int posIndex = 0;
        int curIndex = 0;
        public int Index { get { return curIndex; } }
        private bool isSetToEnd = false;
        bool setIndex(int idx)
        {
            isSetToEnd = idx > maxIndex;
            idx = idx >= maxIndex ? maxIndex - 1 : idx;
            bool isChange = idx != this.curIndex;
            this.curIndex = idx > 0 ? idx : 0;
            return isChange;
        }
        public int GetIndex(Vector2 co)
        {
            //float value = 0f;
            int num = 0;
            if (axis == Axis.Horizontality)
            {
                if (co.x * mDirX >= 0f)
                    return 0;
                if (this.TempList.ItemPosList != null)
                {
                    return this.TempList.GetIndex(co, true,this.IsTrunOver);
                }
                float xx = Mathf.Abs(co.x);
                num = Mathf.FloorToInt(xx / scrollSize.x);
            }
            else
            {
                if (co.y * mDirY >= 0f)
                    return 0;
                if (this.TempList.ItemPosList != null)
                {
                    return this.TempList.GetIndex(co, false, this.IsTrunOver);
                }
                float yy = Mathf.Abs(co.y);
                num = Mathf.FloorToInt(yy / scrollSize.y);
            }
            return num;
        }
        public Vector2 GetPos(int index)
        {
            Vector2 end = Vector2.zero;
            if (maxIndex <= 0 && index <= 0)
                return end;
            int maxIdx = maxIndex;
            
         //   if (OptimizeScrollIndex)
        //    {
                int num = (axis == Axis.Verticality ? YRow : XColumn);
                maxIdx = maxIndex + num;
        //    }
            if (index >= maxIdx || isSetToEnd)
            {
                return EndPostion;
            }
            //if (index == maxIndex)
            //    index = maxIndex - 1;
            //index++;
            if (this.TempList.ItemPosList != null)
            {
                Vector2 max = this.TempList.ItemPosList[index].max;
                end = new Vector2(Mathf.Abs(max.x), Mathf.Abs(max.y));
            }
            else
            {
                end = new Vector2(scrollSize.x * index, scrollSize.y * index);
            }
            //float v = 0f;
            if (axis == Axis.Horizontality)
            {
                //v = scrollSize.x * index;
                //v = v > Size.x ? Size.x : v;
                end.x = -mDirX * (end.x + 0.1f);
                end.y = 0f;
                    //end.x = end.x < -this.Width ? -this.Width : end.x;
            }
            else
            {
                //v = scrollSize.y * index;
                //v = v > Size.y ? Size.y : v;
                end.y = -mDirY * (end.y - 0.1f);
                end.x = 0f;
                    //end.y = end.y > this.Height ? this.Height : end.y;
            }
          //  if (OptimizeScrollIndex)
                end = GetOptimizePos(end,index);
            return end;
        }
        Vector2 GetOptimizePos(Vector2 end,int idx)
        {
            int curIdx = GetIndex(this.Content.anchoredPosition);
            int num = (axis == Axis.Verticality ? YRow : XColumn);
            if (idx < curIdx + num-1)
            {
                return end;
            }
            //Rect rt = InItemRect;
            Vector2 itemSize = scrollSize;
            if (this.TempList.ItemPosList != null)
            {
                itemSize = this.TempList.ItemPosList[idx].size;
            }
            Vector2 off = this.rectTransform.rect.size - itemSize;
            if (axis == Axis.Horizontality)
            {
                end.x += mDirX * Mathf.Abs(off.x);
            }
            else
            {
                end.y += mDirY * Mathf.Abs(off.y);
            }
            return end;
        }
        public Rect InItemRect
        {
            get
            {   
                Vector2 loopPos2ItemPos = this.rectTransform.anchoredPosition - this.Content.anchoredPosition;
                Rect rt = new Rect(loopPos2ItemPos,this.rectTransform.rect.size);
                return rt;
            }
        }
        public Vector2 EndPostion
        {
            get
            {
                Vector2 end = Vector2.zero;
                RectTransform rt = this.transform as RectTransform;
                float size = 0f;
                if (axis == Axis.Horizontality)
                {
                    size = this.TempList.Size.x - rt.rect.width;
                    size = size <= 0f ? 0f : size;
                    end.x = -mDirX * (size + 0.1f);
                    end.y = 0f;

                }
                else
                {
                    size = this.TempList.Size.y - rt.rect.height;
                    size = size <= 0f ? 0f : size;
                    end.x = 0f;
                    end.y = -mDirY * (size - 0.1f);
                }
                return end;
            }

        }
        public bool IsTrunOver
        {
            get { return axis == Axis.Horizontality && mDirX == -1 || axis == Axis.Verticality && mDirY == 1; }
        }
        public virtual bool Contains(GameObject go)
        {
            return this.TempList.Contains(go);
        }
        private int mDirX
        {
            get{ return startCorner == GridLayoutGroup.Corner.UpperLeft || startCorner == GridLayoutGroup.Corner.LowerLeft ? 1 : -1; }
        }
        private int mDirY
        {
            get { return startCorner == GridLayoutGroup.Corner.LowerLeft || startCorner == GridLayoutGroup.Corner.LowerRight ? 1 : -1; }
        }
        public void SetSelect(int itemIdx, bool force = false)
        {
            this.TempList.SetSelect(itemIdx, force);
        }
        int toIndex = -1;
        float moveTime = 1f;
        float curTime = 0f;
        Vector2 start = Vector2.zero;
        Vector2 end = Vector2.zero;
        /// <summary>
        /// 定位列表 行||列
        /// </summary>
        /// <param name="idx">行||列</param>
        /// <param name="_time">耗时</param>
        public void SetScrollIndex(int itemIdx, float _time = 0f)
        {
            int idx = itemIdx / scrollNum;
            toIndex = idx;
            moveTime = _time;
            start = this.Content.anchoredPosition;
            end = GetPos(toIndex);
            curTime = 0f;
            scrollToIndex();
        }
        void scrollToIndex()
        {
            if (toIndex < 0)
                return;
            curTime += Time.deltaTime;
            float time = curTime;
            if (moveTime > 0f)
                time = time / moveTime;
            if (moveTime <= 0f || time >= 1f)
            {
                this.Content.anchoredPosition = end;
                toIndex = -1;
                return;
            }
            Vector2 to = Vector2.zero;
            if (axis == Axis.Horizontality)
            {
                to.x = MTweenEase.easeOutQuad(start.x, end.x, time);
                //if (end.x >= to.x && end.x <= lastPos.x || end.x <= to.x && end.x >= lastPos.x)
                //    isEnd = true;
            }
            else
            {
                to.y = MTweenEase.easeOutQuad(start.y, end.y, time);
                //if (end.y >= to.y && end.y <= lastPos.y || end.y <= to.y && end.y >= lastPos.y)
                //    isEnd = true;
            }
            this.Content.anchoredPosition = to;
        }
        // Update is called once per frame
        void Update()
        {
            //if (!isInit)
            //    return;
            updatePosition();
            scrollToIndex();
            updateArrow();
        }
        void updateIndex()
        {
            for (int i = 0; i < this.TempList.listItem.Count; i++)
            {
                IUIItem it = this.TempList.listItem[i];
                setItemIndex(it, Index * scrollNum + it.Order);
            }
            //this.TempList.FixItemPosition();
        }
        public bool IsEnd { private set; get; }
        public bool IsStart { private set; get; }
        public bool IsDraging { private set; get; }
        void updateArrow()
        {
            Vector2 curPos = this.Content.anchoredPosition;
            Vector2 endPos = this.EndPostion;
            bool isStart = false;
            bool isEnd = false;
            if (axis == Axis.Horizontality)
            {
                isStart = curPos.x >= -1f;
                isEnd = curPos.x <= endPos.x + 1f;
            }
            else
            {
                isStart = curPos.y <= 1f;
                isEnd = curPos.y >= endPos.y - 1f;
            }
            if (goArrowLeft != null)
                goArrowLeft.SetActive(!isStart);
                //goArrowLeft.SetActive(this.Index > 0 && maxIndex > 0);
            if (goArrowRight != null)
                goArrowRight.SetActive(!isEnd);
                //goArrowRight.SetActive(Index < maxIndex - 1 && maxIndex > 0);
            IsStart = isStart;
            IsEnd = isEnd;
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDraging = true;
            if (dragCall != null)
                dragCall(IsDraging);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            IsDraging = false;
            if (dragCall != null)
                dragCall(IsDraging);
        }
        void setItemIndex(IUIItem it,int idx)
        {
            if (idx >= mAllNum)
                return;
            if (IsTrunOver && maxIndex > 0)
            {
                if (maxIndex > this.Index)
                    idx = mAllNum - idx - 1;
                else
                    idx = mAllNum - idx;
            }
            it.Index = idx;
        }
        protected override void OnDestroy()
        {
            sRect = null;
            this.TempList = null;
            this.cacheMoveList.Clear();
            this.cacheMoveList = null;
            if (dragCall != null)
            {
                dragCall = null;
            }
            base.OnDestroy();
        }
        void resetContent()
        {
            this.curIndex = 0;

            if (this.Grid != null)
            {
                Vector2 co = Vector2.up;
                switch (startCorner)
                {
                    case GridLayoutGroup.Corner.UpperLeft:
                        co = Vector2.up;
                        break;
                    case GridLayoutGroup.Corner.UpperRight:
                        co = Vector2.one;
                        break;
                    case GridLayoutGroup.Corner.LowerLeft:
                        co = Vector2.zero;
                        break;
                    case GridLayoutGroup.Corner.LowerRight:
                        co = Vector2.right;
                        break;
                    default:
                        break;
                }
                this.Content.anchorMin = co;
                this.Content.anchorMax = co;
                this.Content.pivot = co;
            }
            this.Content.anchoredPosition = Vector2.zero;

        }
        public void SerializeFieldInfo()
        {
            if (TempList == null && this.transform.childCount > 0)
                TempList = this.transform.GetChild(0).GetComponent<MList>();
            if (sRect == null)
                sRect = this.transform.GetComponent<ScrollRect>();
            if(TempList != null)
            {
                TempList.AutoSize = true;
                TempList.FixItemSize = true;
            }
            resetContent();
            if(this.Grid != null)
            {
                this.Grid.startAxis = sRect.vertical ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
                this.Grid.constraint = sRect.vertical ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
                if (this.Grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                {
                    YRow = this.Grid.constraintCount;
                    XColumn = Mathf.CeilToInt((this.rectTransform.rect.width - Grid.padding.left - Grid.padding.right) / (Grid.cellSize.x + Grid.spacing.x));
                }
                if (this.Grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    YRow = Mathf.CeilToInt((this.rectTransform.rect.height - Grid.padding.top - Grid.padding.bottom) / (Grid.cellSize.y + Grid.spacing.y));
                    XColumn = this.Grid.constraintCount;
                }
            }
        }
    }
}