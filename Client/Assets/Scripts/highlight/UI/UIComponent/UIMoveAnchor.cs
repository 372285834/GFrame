using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIMoveAnchor : MonoBehaviour
{
    public RectTransform rect {get{ return this.transform as RectTransform; }}
    public AnimationCurve pos = AnimationCurve.Linear(0,0,1,1);
   // public AnimationCurve scale;
    public RectTransform target;
   // public float startScale;
   // public float endScale;
    public float totlaTime = 2f;
    public bool autoStop = true;
    public Camera mCamera;
    public float curTime;
    public Vector2 tPos;
    public Vector2 startPos;

    public void Start()
    {
        startPos = this.rect.localPosition;
    }
    public void InitTargetPos()
    {
        Vector2 sPos = RectTransformUtility.WorldToScreenPoint(mCamera, this.target.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform.parent as RectTransform, sPos, mCamera, out tPos);
    }
    public float curProgress {
        get
        {
            return curTime / totlaTime;
        }
    }
    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime;
        float vp = pos.Evaluate(curProgress);
        this.rect.localPosition = Vector2.Lerp(this.startPos, this.tPos, vp);
        
    //    float sp = scale.Evaluate(curProgress);
     //   float sSize = Mathf.Lerp(this.startScale, this.endScale, sp);
    //    this.rect.localScale = new Vector3(sSize, sSize, sSize);


        if (curTime > totlaTime)
        {
            if(autoStop)
            {
                this.Stop();
            }
        }
    }
    public void Play()
    {
        this.curTime = 0f;
        InitTargetPos();
        this.enabled = true;
    }
    public void Stop()
    {
        this.enabled = false;
        this.curTime = 0f;
        this.gameObject.SetActive(false);
    }
    public void Reset()
    {
        this.enabled = false;
        this.curTime = 0f;
        this.gameObject.SetActive(true);
        this.rect.localPosition = this.startPos;
    }
}
