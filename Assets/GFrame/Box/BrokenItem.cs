using highlight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenItem : MonoBehaviour, ISerializeField
{

    public Vector3 center = Vector3.zero;
    public Vector2 powerRang = new Vector2(2, 7);
    public float time = 2f;
    public bool autoDestory = false;
    public Rigidbody[] Rigidbodys;
    private Vector3[] posArr;
    private Vector3[] rotateArr;
    public static BrokenItem GetBroken(GameObject go)
    {
        return go.GetComponent<BrokenItem>();
    }
    public static BrokenItem PlayBroken(GameObject go, float x, float y, float z)
    {
        BrokenItem bi = go.GetComponent<BrokenItem>();
        bi.PlayByPos(x, y, z);
        return bi;
    }
    public void SerializeFieldInfo()
    {
        MeshFilter[] mfs = this.gameObject.GetComponentsInChildren<MeshFilter>();
        Rigidbodys = new Rigidbody[mfs.Length];
        for (int i = 0; i < mfs.Length; i++)
        {
            MeshCollider mc = mfs[i].GetComponent<MeshCollider>();
            if(mc == null)
                mc = mfs[i].gameObject.AddComponent<MeshCollider>();
            mc.convex = true;
            Rigidbody rigid = mfs[i].GetComponent<Rigidbody>();
            if (rigid == null)
                rigid = mfs[i].gameObject.AddComponent<Rigidbody>();
            Rigidbodys[i] = rigid;

            rigid.GetComponent<Collider>().enabled = false;

            rigid.interpolation = RigidbodyInterpolation.Interpolate;
            rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigid.drag = 1f;
            rigid.angularDrag = 0.05f;

            rigid.mass = 0f;
            rigid.isKinematic = true;
            rigid.useGravity = false;
        }
    }
    bool isInit = false;
    void Init()
    {
        if (isInit)
            return;
        isInit = true;
        posArr = new Vector3[Rigidbodys.Length];
        rotateArr = new Vector3[Rigidbodys.Length];
        for (int i = 0; i < Rigidbodys.Length; i++)
        {
            posArr[i] = Rigidbodys[i].transform.localPosition;
            rotateArr[i] = Rigidbodys[i].transform.localEulerAngles;
        }
    }
    private float curTime = -1f;
    public bool IsPlaying { get { return curTime >= 0f; } }
    public void PlayByPos(float x,float y,float z)
    {
        this.center = new Vector3(x, y, z) - this.transform.position;
        this.Play();
    }
    public void Play()
    {
        Init();
        curTime = 0f;
        for (int i=0;i< Rigidbodys.Length;i++)
        {
            Rigidbodys[i].GetComponent<Collider>().enabled = true;
            Rigidbodys[i].isKinematic = false;
            Rigidbodys[i].useGravity = true;
            //Rigidbodys[i].WakeUp();
            // Rigidbodys[i].useGravity = true;
            Vector3 itemPos = Rigidbodys[i].GetComponent<MeshFilter>().sharedMesh.bounds.center;
            Vector3 dir = itemPos + Rigidbodys[i].transform.localPosition - center;
            if (dir.magnitude < 0.01f)
                dir = Vector3.left;
            float force = Random.Range(powerRang.x, powerRang.y);
            Rigidbodys[i].mass = 1f;
            Rigidbodys[i].velocity = dir.normalized * force;
            //Rigidbodys[i].AddForce(dir * force, ForceMode.Force);
        }
    }
    public void Stop()
    {
        curTime = -1f;
        for (int i = 0; i < Rigidbodys.Length; i++)
        {
            Rigidbodys[i].mass = 0f;
           // Rigidbodys[i].Sleep();
            Rigidbodys[i].isKinematic = true;
            //Rigidbodys[i].useGravity = false;
            Rigidbodys[i].GetComponent<Collider>().enabled = false;
        }
        if (autoDestory)
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }
    public void Reset()
    {
        for (int i = 0; i < Rigidbodys.Length; i++)
        {
            Rigidbodys[i].transform.localPosition = posArr[i];
            Rigidbodys[i].transform.localEulerAngles = rotateArr[i];
        }
        Stop();
    }
    // Update is called once per frame
    void Update () {
		if(IsPlaying)
        {
            curTime += Time.deltaTime;
            if(curTime >= time)
            {
                Stop();
            }
        }
	}
}
