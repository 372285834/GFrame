using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestItem1 : UITabItem
{
    public Text Text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnUpdate()
    {
        KeyValuePair<int, bool> kv = (KeyValuePair<int, bool>)this.param;
        this.Text.text = kv.Key.ToString();
        this.IsSelected = kv.Value;
    }
    public override void OnSelectItem()
    {
        this.IsSelected = !this.IsSelected;
    }
}
