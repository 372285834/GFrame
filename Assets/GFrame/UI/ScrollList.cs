using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace highlight
{
    [DisallowMultipleComponent]
    public class ScrollList : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ISerializeField
    {
        public enum Axis
        {
            Horizontality,
            Verticality,
        }
        // targets enhance item in scroll view
        public MList TempList;
        public Vector2 itemSize = new Vector2(100f, 100f);
        public Axis axis = Axis.Horizontality;
        // Control the item's scale curve
        public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        // Control the position curve
        public AnimationCurve positionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        // Control the "depth"'s curve(In 3d version just the Z value, in 2D UI you can use the depth(NGUI))
        // NOTE:
        // 1. In NGUI set the widget's depth may cause performance problem
        // 2. If you use 3D UI just set the Item's Z position
        public AnimationCurve depthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        // The start center index

        private float totalHorizontalWidth
        {
            get
            {
                if (this.axis == Axis.Horizontality)
                    return this.TempList.rectTransform.rect.width;
                else
                    return this.TempList.rectTransform.rect.height;
            }
        }
        // vertical fixed position value 
        //public float yFixedPositionValue = 46.0f;

        // Lerp duration
        public float lerpDuration = 0.2f;
        private float mCurrentDuration = 0.0f;
        //private int mCenterIndex = 0;
        public bool enableLerpTween = false;

        // if we can change the target item
        private bool canChangeItem = true;
        private float dFactor = 0.2f;

        // originHorizontalValue Lerp to horizontalTargetValue
        private float originHorizontalValue = 0.1f;
        [Range(0f,1f)]
        public float curValue = 0.5f;

        // "depth" factor (2d widget depth or 3d Z value)
        private int depthFactor = 5;
        // sort to get right index
        private List<IUIItem> listSortedItems = new List<IUIItem>();

        int curIndex = -1;
        public int Index { get { return curIndex; } }
        private bool IsInit = false;
        void Awake()
        {
            if (this.IsInit)
                return;
            IsInit = true;
            if (this.TempList.Grid != null)
                this.TempList.Grid.enabled = false;
            this.TempList.OnClickItem = OnClickItem;
        }
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
            if (TempList == null)
                return;
            int num = list != null ? list.Count : 0;
            this.TempList.SetData(list, updateFunc, clickFunc, fixFunc);
            CreateList(num, aIndeTarget);
        }
        public void CreateList(int num, int idx = -1)
        {
            this.TempList.SetGridNum(0);
            this.TempList.OnUpdateIndex = updateIndex;
            this.TempList.CreateList(num);
            if (num <= 0)
                return;
            listSortedItems = new List<IUIItem>(this.TempList.listItem.ToArray());
            if (idx >= 0)
                curIndex = idx;
            if (curIndex < 0)
                curIndex = 0;
            ScrollCurIndxItem();
        }
        public void ScrollCurIndxItem()
        {
            curValue = 0.5f - this.TempList.listItem[curIndex].CenterOffSet;
            //this.TempList.SetSelect(curIndex);
            LerpTweenToTarget(0f, curValue, false);
        }
        public void SetSelect(int itemIdx, bool force = false)
        {
            this.TempList.SetSelect(itemIdx, force);
        }
        private Vector2 clampSize = Vector2.zero;
        void updateIndex()
        {
            int count = TempList.Count;
            if (count <= 0)
                return;
            canChangeItem = true;
            dFactor = (Mathf.RoundToInt((1f / count) * 100000f)) * 0.00001f;
            int mCenterIndex = count / 2;
            float halfV = dFactor * 0.5f;
            clampSize = new Vector2(halfV, 1 - halfV);
            if (count % 2 == 0)
            {
                mCenterIndex = count / 2 - 1;
                clampSize = new Vector2(dFactor, 1f);
            }
            int index = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                IUIItem item = this.TempList.listItem[i];
                item.Index = i;
                item.CenterOffSet = dFactor * (mCenterIndex - index);
                item.Size = itemSize;
                index++;
            }
        }

        private void LerpTweenToTarget(float originValue, float targetValue, bool needTween = false)
        {
            if (!needTween)
            {
                //SortEnhanceItem();
                originHorizontalValue = targetValue;
                UpdateEnhanceScrollView(targetValue);
                this.OnTweenOver();
                speed = 1f;
            }
            else
            {
                originHorizontalValue = originValue;
                curValue = targetValue;
                if (!enableLoop)
                {
                    curValue = Mathf.Clamp(curValue, clampSize.x, clampSize.y);
                }
                mCurrentDuration = 0.0f;
                speed = 2.5f;
            }
            enableLerpTween = needTween;
        }

        public void DisableLerpTween()
        {
            this.enableLerpTween = false;
        }
        public bool enableLoop = true;
        /// 
        /// Update EnhanceItem state with curve fTime value
        /// 
        public void UpdateEnhanceScrollView(float fValue)
        {
            for (int i = 0; i < this.TempList.Count; i++)
            {
                IUIItem itemScript = this.TempList.listItem[i];
                float xValue = GetXPosValue(fValue, itemScript.CenterOffSet);
                float scaleValue = GetScaleValue(fValue, itemScript.CenterOffSet);
                float depthCurveValue = depthCurve.Evaluate(fValue + itemScript.CenterOffSet);
                if (this.axis == Axis.Horizontality)
                    itemScript.UpdateScrollViewItems(xValue, depthCurveValue, depthFactor, this.TempList.Count, 0f, scaleValue);
                else
                    itemScript.UpdateScrollViewItems(0f, depthCurveValue, depthFactor, this.TempList.Count, xValue, scaleValue);
            }
        }

        void Update()
        {
            if (enableLerpTween)
                TweenViewToTarget();
        }
        bool mIsDrag = false;
        public bool isScroll
        {
            get
            {
                return mIsDrag;
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            mIsDrag = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            mIsDrag = true;
            this.OnDragEnhanceViewMove(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            this.OnDragEnhanceViewEnd();
            mIsDrag = false;
        }
        private void TweenViewToTarget()
        {
            mCurrentDuration += Time.deltaTime * speed;
            if (mCurrentDuration > lerpDuration)
                mCurrentDuration = lerpDuration;

            float percent = mCurrentDuration / lerpDuration;
            float value = Mathf.Lerp(originHorizontalValue, curValue, percent);
            UpdateEnhanceScrollView(value);
            if (mCurrentDuration >= lerpDuration)
            {
                canChangeItem = true;
                enableLerpTween = false;
                OnTweenOver();
            }
        }

        // Get the evaluate value to set item's scale
        private float GetScaleValue(float sliderValue, float added)
        {
            float scaleValue = scaleCurve.Evaluate(sliderValue + added);
            return scaleValue;
        }

        // Get the X value set the Item's position
        private float GetXPosValue(float sliderValue, float added)
        {
            float evaluateValue = positionCurve.Evaluate(sliderValue + added) * totalHorizontalWidth - 0.5f * totalHorizontalWidth;
            return evaluateValue;
        }

        public void SetHorizontalTargetItemIndex(int idx)
        {
            if (!canChangeItem)
                return;
            IUIItem selectItem = this.TempList.listItem[idx];
            IUIItem curItem = this.TempList.mSelected;
            if (curItem == selectItem)
                return;

            // calculate the direction of moving
            //float centerXValue = GetXPosValue(0.5f, 0f);
            //bool isRight = false;
            //if (this.axis == Axis.Horizontality && selectItem.transform.localPosition.x > centerXValue)
            //    isRight = true;
            //else if (selectItem.transform.localPosition.y > centerXValue)
            //    isRight = true;

            //// calculate the offset * dFactor
            //int moveIndexCount = GetMoveCurveFactorCount(this.TempList.mSelected, selectItem);
            //float dvalue = 0.0f;
            //if (isRight)
            //    dvalue = -dFactor * moveIndexCount;
            //else
            //    dvalue = dFactor * moveIndexCount;
            //float originValue = curValue;


            float center = curItem.CenterOffSet;
            float select = selectItem.CenterOffSet;
            float v = 0f;
           // float off = center * select;
            v = center - select;

            //if (!enableLoop)
            //{
            //    if (v <= -0.5f || v > 0.5f)
            //    {
            //        return;
            //    }
            //}
            if ((center <= 0 && select >= 0) || (center >= 0 && select <= 0))
            {
                if (v <= -0.5f)
                    v = v + 1f;
                if (v > 0.5f)
                    v = v - 1f;
            }
            //if (Mathf.Abs(dvalue - v) > 0.001f)
            //{
            //    Debug.LogError(dvalue + "  " + v + "   " + center + "  " + select);
            //}
            this.curIndex = idx;
            canChangeItem = false;
            LerpTweenToTarget(curValue, curValue + v, true);
        }

        public void SetScrollIndex(int idx)
        {
            if (idx < 0 || idx > this.TempList.Count - 1)
                return;
            SetHorizontalTargetItemIndex(idx);
        }
        // Click the right button to select the next item.
        public void OnBtnRightClick()
        {
            int targetIndex = this.TempList.CurIndex + 1;
            if (targetIndex > this.TempList.listItem.Count - 1)
            {
                if (!enableLoop)
                    return;
                targetIndex = 0;
            }
            SetHorizontalTargetItemIndex(targetIndex);
        }

        // Click the left button the select next next item.
        public void OnBtnLeftClick()
        {
            int targetIndex = this.TempList.CurIndex - 1;
            if (targetIndex < 0)
            {
                if (!enableLoop)
                    return;
                targetIndex = this.TempList.listItem.Count - 1;
            }
            SetHorizontalTargetItemIndex(targetIndex);
        }
        public void OnClickItem(IUIItem item)
        {
            if (item == null)
                return;
            SetHorizontalTargetItemIndex(item.Index);
        }
        public float factor = 1f;
        // On Drag Move
        public void OnDragEnhanceViewMove(Vector2 delta)
        {
            float del = delta.x + delta.y;
            // In developing
            if (Mathf.Abs(del) > 0.0f)
            {
                curValue += del * factor * 0.001f;
                LerpTweenToTarget(0.0f, curValue, false);
            }
        }
        private float speed = 1f;
        // On Drag End
        public void OnDragEnhanceViewEnd()
        {
            // find closed item to be centered
            int closestIndex = 0;
            float value = (curValue - (int)curValue);
            float min = float.MaxValue;
            float tmp = 0.5f * (curValue < 0 ? -1 : 1);
            for (int i = 0; i < this.TempList.listItem.Count; i++)
            {
                float dis = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs((tmp - this.TempList.listItem[i].CenterOffSet)));
                if (dis < min)
                {
                    closestIndex = i;
                    min = dis;
                }
            }
            originHorizontalValue = curValue; 
            if (!enableLoop)
            {
                if (curValue <= dFactor)
                    curIndex = this.TempList.Count - 1;
                else if (curValue >= 1f)
                    curIndex = 0;
                else
                    curIndex = closestIndex;
            }
            else
                curIndex = closestIndex;
            float target = ((int)curValue + (tmp - this.TempList.listItem[curIndex].CenterOffSet));
            
            //if (curCenterItem != null)
            //    curCenterItem.SetSelectState(false);
            //preCenterItem = curCenterItem;
            //curCenterItem = this.TempList.listItem[closestIndex];
            LerpTweenToTarget(originHorizontalValue, target, true);
            canChangeItem = false;
        }
        private void OnTweenOver()
        {
            IUIItem curItem = this.TempList.GetItemByIndex(this.curIndex);
            IUIItem lastItem = this.TempList.mSelected;
            if (curItem != null && lastItem == curItem)
                return;
            SetSelect(curIndex);
            //GameManager.LuaCallMethod(onChangeCall, curItem, lastItem);
          //  GameManager.uluaMgr.UpdateScrollList.Call(this, curItem, lastItem);
        }

        public void SerializeFieldInfo()
        {
            if (TempList == null && this.transform.childCount > 0)
                TempList = this.transform.GetChild(0).GetComponent<MList>();

            if (TempList != null)
            {
                TempList.AutoSize = false;
                TempList.FixItemSize = false;
                if (TempList.Grid != null)
                    TempList.Grid.enabled = false;
                editorUpdate(curValue);
            }
        }
        void editorUpdate(float fValue)
        {
            MList list = this.TempList;
            int count = list.transform.childCount;
            dFactor = (Mathf.RoundToInt((1f / count) * 100000f)) * 0.00001f;
            int mCenterIndex = count / 2;
            float halfV = dFactor * 0.5f;
            clampSize = new Vector2(halfV, 1 - halfV);
            if (count % 2 == 0)
            {
                mCenterIndex = count / 2 - 1;
                clampSize = new Vector2(dFactor, 1f);
            }
            if(enableLoop)
            {
                positionCurve.postWrapMode = WrapMode.Loop;
                positionCurve.preWrapMode = WrapMode.Loop;
                scaleCurve.postWrapMode = WrapMode.Loop;
                scaleCurve.preWrapMode = WrapMode.Loop;
                depthCurve.postWrapMode = WrapMode.Loop;
                depthCurve.preWrapMode = WrapMode.Loop;
            }
            for (int i = 0; i < count; i++)
            {
                IUIItem itemScript = list.transform.GetChild(i).GetComponent<IUIItem>();

                itemScript.CenterOffSet = dFactor * (mCenterIndex - i);
                itemScript.Size = itemSize;
                float xValue = GetXPosValue(fValue, itemScript.CenterOffSet);
                float scaleValue = GetScaleValue(fValue, itemScript.CenterOffSet);
                float depthCurveValue = depthCurve.Evaluate(fValue + itemScript.CenterOffSet);
                if (this.axis == Axis.Horizontality)
                    itemScript.UpdateScrollViewItems(xValue, depthCurveValue, depthFactor, count, 0f, scaleValue);
                else
                    itemScript.UpdateScrollViewItems(0f, depthCurveValue, depthFactor, count, xValue, scaleValue);
            }
        }
    }
}