using highlight.tl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    public class SceneObject : Object
    {
        public Timeline ai;
        public Buffs buffs;
        public Skills skills;
        public RoleAttrs attrs;
        public bool isClear = false;
        public Transform transform;
        public Animator animator;
        public Dictionary<string, Transform> LocatorDic = new Dictionary<string, Transform>();
        //public AnimationBox aniBox;
        public void Init(GameObject go)
        {
            isClear = false;
            this.transform = go.transform;
            this.animator = go.GetComponentInChildren<Animator>(true);
            Transform[] tfs = go.GetComponentsInChildren<Transform>(true);
            for(int i=0;i<tfs.Length;i++)
            {
                LocatorDic[tfs[i].name] = tfs[i];
            }
        }
        public Vector3 getPosition()
        {
            return transform.position;
        }
        public Transform getLocator(string name)
        {
            return null;
        }
        public void PlayAction()
        {

        }
        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }
        public void SetLocalPos(Vector3 pos)
        {
            transform.localPosition = pos;
        }
        public void SetParent(Transform t)
        {
            transform.SetParent(t);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
        public virtual void UpdateFrame(int frame)
        {
            if (ai != null)
                ai.UpdateFrame(frame);
            if(attrs != null)
                attrs.UpdateFrame(frame);
            if (skills != null)
                skills.UpdateFrame(frame);
            if (buffs != null)
                buffs.UpdateFrame(frame);
        }
        public virtual void UpdateRender()
        {

        }
        public virtual void Clear()
        {
            if (skills != null)
                skills.Release();
            if (buffs != null)
                buffs.Release();
            if (attrs != null)
                attrs.Release();
            if (ai != null)
                ai.Destroy();
            skills = null;
            buffs = null;
            ai = null;
            LocatorDic.Clear();
            animator = null;
            transform = null;
            isClear = true;
        }
    }
}