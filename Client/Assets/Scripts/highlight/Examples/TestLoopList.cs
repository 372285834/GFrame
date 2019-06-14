using highlight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLoopList : MonoBehaviour
{
    public LoopList loopList;
    public int size = 100;
    // Start is called before the first frame update
    void Start()
    {
        InitLoopList();
    }
    // Update is called once per frame
    void Update()
    {
        if(size != loopList.AllNum)
        {
            InitLoopList();
        }
    }
    void InitLoopList()
    {
        List<KeyValuePair<int, bool>> list = new List<KeyValuePair<int, bool>>(size);
        for(int i=0;i< size; i++)
        {
            list.Add(new KeyValuePair<int, bool>(i,false));
        }
        loopList.Init(list);
    }
}
