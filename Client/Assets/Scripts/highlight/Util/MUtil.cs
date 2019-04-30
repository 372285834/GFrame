using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MUtil
{
    public static int Int(object o)
    {
        return Convert.ToInt32(o);
    }

    public static float Float(object o)
    {
        return (float)Math.Round(Convert.ToSingle(o), 2);
    }

    public static long Long(object o)
    {
        return Convert.ToInt64(o);
    }

    public static int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static float RandomFloat(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static string Uid(string uid)
    {
        int position = uid.LastIndexOf('_');
        return uid.Remove(0, position + 1);
    }

    public static long GetTime()
    {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
        return (long)ts.TotalMilliseconds;
    }

    /// <summary>
    /// 搜索子物体组件-GameObject版
    /// </summary>
    public static T Get<T>(GameObject go, string subnode) where T : Component
    {
        if (go != null)
        {
            Transform sub = go.transform.Find(subnode);
            if (sub != null) return sub.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 搜索子物体组件-Transform版
    /// </summary>
    public static T Get<T>(Transform go, string subnode) where T : Component
    {
        if (go != null)
        {
            Transform sub = go.Find(subnode);
            if (sub != null) return sub.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 搜索子物体组件-Component版
    /// </summary>
    public static T Get<T>(Component go, string subnode) where T : Component
    {
        return go.transform.Find(subnode).GetComponent<T>();
    }

    /// <summary>
    /// 添加组件
    /// </summary>
    public static T Add<T>(GameObject go) where T : Component
    {
        if (go != null)
        {
            T[] ts = go.GetComponents<T>();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] != null) GameObject.Destroy(ts[i]);
            }
            return go.gameObject.AddComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 添加组件
    /// </summary>
    public static T Add<T>(Transform go) where T : Component
    {
        return Add<T>(go.gameObject);
    }

    /// <summary>
    /// 查找子对象
    /// </summary>
    public static GameObject Child(GameObject go, string subnode)
    {
        return Child(go.transform, subnode);
    }

    /// <summary>
    /// 查找子对象
    /// </summary>
    public static GameObject Child(Transform go, string subnode)
    {
        Transform tran = go.Find(subnode);
        if (tran == null) return null;
        return tran.gameObject;
    }

    /// <summary>
    /// 取平级对象
    /// </summary>
    public static GameObject Peer(GameObject go, string subnode)
    {
        return Peer(go.transform, subnode);
    }

    /// <summary>
    /// 取平级对象
    /// </summary>
    public static GameObject Peer(Transform go, string subnode)
    {
        Transform tran = go.parent.Find(subnode);
        if (tran == null) return null;
        return tran.gameObject;
    }

    /// <summary>
    /// 手机震动
    /// </summary>
    public static void Vibrate()
    {
        //int canVibrate = PlayerPrefs.GetInt(ConstData.AppPrefix + "Vibrate", 1);
        //if (canVibrate == 1) iPhoneUtils.Vibrate();
    }

    /// <summary>
    /// Base64编码
    /// </summary>
    public static string Encode(string message)
    {
        byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Base64解码
    /// </summary>
    public static string Decode(string message)
    {
        byte[] bytes = Convert.FromBase64String(message);
        return Encoding.GetEncoding("utf-8").GetString(bytes);
    }

    /// <summary>
    /// 判断数字
    /// </summary>
    public static bool IsNumeric(string str)
    {
        if (str == null || str.Length == 0) return false;
        for (int i = 0; i < str.Length; i++)
        {
            if (!Char.IsNumber(str[i])) { return false; }
        }
        return true;
    }

    /// <summary>
    /// HashToMD5Hex
    /// </summary>
    public static string HashToMD5Hex(string sourceStr)
    {
        byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] result = md5.ComputeHash(Bytes);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
                builder.Append(result[i].ToString("x2"));
            return builder.ToString();
        }
    }

    /// <summary>
    /// 计算字符串的MD5值
    /// </summary>
    public static string md5(string source)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');

        return destString;
    }

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    /// <summary>
    /// DES 加密
    /// </summary>
    /// <param name="sourceString"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string DesEncrypt(string sourceString, string key)
    {
		DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
		byte[] rgbKey = Encoding.UTF8.GetBytes(key);
		byte[] inputByteArray = Encoding.UTF8.GetBytes(sourceString);
		MemoryStream mStream = new MemoryStream();
		CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, new byte[8]), CryptoStreamMode.Write);
		cStream.Write(inputByteArray, 0, inputByteArray.Length);
		cStream.FlushFinalBlock();
		
		return Convert.ToBase64String(mStream.ToArray());
    }

	/// <summary>
	/// 进行DES解密。
	/// </summary>
	/// <param name="pToDecrypt">要解密的以Base64</param>
	/// <param name="sKey">密钥，且必须为8位。</param>
	/// <returns>已解密的字符串。</returns>
	public static string DesDecrypt(string decryptString, string key)
	{
		byte[] rgbKey = Encoding.UTF8.GetBytes(key);
		byte[] inputByteArray = Convert.FromBase64String(decryptString);
		DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
		MemoryStream mStream = new MemoryStream();
		CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, new byte[8]), CryptoStreamMode.Write);
		cStream.Write(inputByteArray, 0, inputByteArray.Length);
		cStream.FlushFinalBlock();
		return Encoding.UTF8.GetString(mStream.ToArray());
	}

    /// <summary>
    /// 清除所有子节点
    /// </summary>
    public static void ClearChild(Transform go)
    {
        if (go == null) return;
        for (int i = go.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(go.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 清理内存
    /// </summary>
    public static void ClearMemory()
    {
        Resources.UnloadUnusedAssets();
#if !UNITY_EDITOR
        GC.Collect(); 
#endif
    }

    /// <summary>
    /// 是否为数字
    /// </summary>
    public static bool IsNumber(string strNumber)
    {
        Regex regex = new Regex("[^0-9]");
        return !regex.IsMatch(strNumber);
    }

    /// <summary>
    /// 取得行文本
    /// </summary>
    public static string GetFileText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// 替换文字
    /// </summary>
    /// <param name="textMesh"></param>
    /// <param name="txt"></param>
    public static void SetMeshText(TextMesh textMesh,string txt)
    {
        textMesh.text = txt;
    }

    public static string GetMeshText(TextMesh textMesh)
    {
        return textMesh.text;
    }


    /// <summary>
    /// 替换图片
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="name"></param>
    public static void SetRenderSprite(SpriteRenderer renderer,string name)
    {
        renderer.sprite.name = name;
    }

    public static string GetRenderSprite(SpriteRenderer renderer)
    {
        return renderer.sprite.name;
    }
    ///
    /// 是否支持3DTouch
    public static bool IsThreeTouch()
    {
        if (Input.touchPressureSupported)
        {
            // Debug.Log("支持3DTouch");
            return true; 
        }
        else
        { 
           // Debug.Log("不支持3DTouch");
            return false;

        }
    }
    //3dtouch的 力度
    public static float ThreeTouchPower()
    {
        if (Input.touchCount <= 0)
        {
            return 0;
        }
       // Debug.Log(" 3DTouch ThreeTouchPower" + Input.GetTouch(0).pressure);
        return Input.GetTouch(0).pressure;
    }
    //3dtouch的 最大力度
    public static float MaximumPossiblePressure()
    {
        if (Input.touchCount <= 0)
        {
            return 0;
        }
       // Debug.Log(" 3DTouch ThreeTouchPower" + Input.GetTouch(0).maximumPossiblePressure);
        return Input.GetTouch(0).maximumPossiblePressure;
    }
    /// <summary>
    /// 网络可用
    /// </summary>
    public static bool NetAvailable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }

    /// <summary>
    /// 是否是无线
    /// </summary>
    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }
    public static GameObject LoadAsset(AssetBundle bundle, string name)
    {
#if UNITY_5
        return bundle.LoadAsset(name, typeof(GameObject)) as GameObject;
#else
        return bundle.LoadAsset(name, typeof(GameObject)) as GameObject;
#endif
    }

    public static Component AddComponent(GameObject go, string assembly, string classname)
    {
        if (assembly.Length > 0)
        {
            Assembly asmb = Assembly.Load(assembly);
            Type t = asmb.GetType(assembly + "." + classname);
            return go.AddComponent(t);
        }

        Type t1 = Type.GetType(classname);
        return go.AddComponent(t1);
    }

    //public static void PushBufferToLua(string funcString, byte[] buffer)
    //{
    //    LuaFunction func = GameManager.uluaMgr.GetLuaFunction(funcString);
    //    if (func == null)
    //    {
    //        Debuger.LogError("PushBufferToLua:lua function " + funcString + " not find");
    //        return;
    //    }
    //    LuaScriptMgr mgr = GameManager.uluaMgr;
    //    int oldTop = func.BeginPCall();
    //    LuaDLL.lua_pushlstring(mgr.lua.L, buffer, buffer.Length);
    //    if (func.PCall(oldTop, 1)) func.EndPCall(oldTop);
    //}

    public static void Activate(Transform t)
    {
        SetActiveSelf(t.gameObject, true);

        for (int i = 0, imax = t.childCount; i < imax; ++i)
        {
            Transform child = t.GetChild(i);
            Activate(child);
        }
    }

    /// <summary>
    /// Deactivate the specified object and all of its children.
    /// </summary>

    public static void Deactivate(Transform t)
    {
        SetActiveSelf(t.gameObject, false);
    }

    static public void SetActiveSelf(GameObject go, bool state)
    {
        go.SetActive(state);
    }

    public static void AddChild(GameObject root, GameObject child)
    {
        if (root == null || child == null)
            return;
        if (child.transform.parent == root.transform)
            return;
        Vector3 vPos = child.transform.localPosition;
        Vector3 vScale = child.transform.localScale;
        child.transform.SetParent(root.transform);
        child.transform.localScale = vScale;
        child.transform.localPosition = vPos;
    }
    public static void AddChildReset(GameObject root, GameObject child,bool isLayer)
    {
        if (root == null || child == null)
            return;
        child.transform.SetParent(root.transform);
        child.transform.localScale = Vector3.one;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localPosition = Vector3.zero;
        if(isLayer)
        {
            SetLayer(child,root.layer);
        }
    }
    public static void AddChildPosition(GameObject root, GameObject child,Vector3 pos)
    {
        if (root == null || child == null)
            return;
        if (child.transform.parent == root.transform)
            return;
        //Vector3 vPos = child.transform.localPosition;
        Vector3 vScale = child.transform.localScale;
        child.transform.SetParent(root.transform);
        child.transform.localScale = vScale;
        child.transform.position = pos;
    }
    static public void SetLayer(GameObject aGo, int aLayer)
    {
        if (aGo.layer == aLayer)
            return;
        aGo.layer = aLayer;
        Renderer[] rends = aGo.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            if(rends[i].gameObject.layer != aLayer)
                rends[i].gameObject.layer = aLayer;
        }
        //Transform t = aGo.transform;
        //for (int i = 0, imax = t.childCount; i < imax; ++i)
        //{
        //    Transform child = t.GetChild(i);
        //    SetLayer(child.gameObject, aLayer);
        //}
    }

    public static string GetSendCode(long nums, int len)
    {
        return GetSendCode(nums.ToString(), len);
    }
    public static string GetSendCode(string str, int len)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            if (len - i > str.Length)
                sb.Append("0");
            else
            {
                sb.Append(str);
                break;
            }
        }
        return sb.ToString();
    }
    public static void ReadTxtFile(string aFullPath, ref List<string> aTxtList)
    {
        StreamReader t_sStreamReader = null;

        t_sStreamReader = File.OpenText(aFullPath);

        string t_sLine;
        while ((t_sLine = t_sStreamReader.ReadLine()) != null)
        {
            aTxtList.Add(t_sLine);
        }
        t_sStreamReader.Close();

        t_sStreamReader.Dispose();
    }

    public static void WriteTxtFile(string aFullPath, List<string> aTxtList)
    {
        StreamWriter t_sStreamWriter;

        FileInfo t_fFileInfo = new FileInfo(aFullPath);

        if (!t_fFileInfo.Exists)
        {
            t_sStreamWriter = t_fFileInfo.CreateText();
        }
        else
        {
            t_sStreamWriter = t_fFileInfo.AppendText();
        }

        for (int i = 0; i < aTxtList.Count; i++)
        {
            t_sStreamWriter.Write(aTxtList[i] + "\n");
        }

        t_sStreamWriter.Flush();
        t_sStreamWriter.Close();
        t_sStreamWriter.Dispose();
    }

    public static Vector3 WorldToViewportPoint(Camera aCamera, Transform aTransform)
    {
        return aCamera.WorldToViewportPoint(aTransform.position);
    }

    public static GameObject CloneGameObject(GameObject aObj)
    {
        return GameObject.Instantiate(aObj) as GameObject;
    }

    public static object Clone(object obj)
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(memoryStream, obj);
        memoryStream.Position = 0;
        return formatter.Deserialize(memoryStream);
    }
    public static Transform RaycastForCoverBlock(Vector3 aOrigin, Vector3 aDirection)
    {
        RaycastHit hit;
        //得到方向
        Debug.DrawLine(aOrigin, aDirection, Color.red);
        bool ret = Physics.Raycast(aOrigin, (aDirection - aOrigin).normalized, out hit);
        if (ret && hit.collider.gameObject.tag == "CoverBlock" )
        {
           return hit.transform;
        }
        return null;
    }
    public static Transform[] Raycast(Vector3 aOrigin, Vector3 aDirection)
    {
        Debug.DrawLine(aOrigin, aDirection, Color.blue);
        RaycastHit[] hit = Physics.RaycastAll(aOrigin, aDirection);
        List<Transform> tmpList = new List<Transform>();
        for (int i = 0; i < hit.Length;i++ )
        {
            tmpList.Add(hit[i].transform);
        }
        return tmpList.ToArray();
    }
    //根据角色半径aRadius、角度aAngle,获得围绕aPos的位置点
    public static Vector3 GetAroundPos(Vector3 aPos, float aRadius, float aAngle)
    {
        return new Vector3(aPos.x + aRadius * Mathf.Cos(aAngle), aPos.y, aPos.z + aRadius * Mathf.Sin(aAngle));
    }
    public static bool IsInScreen(Vector3 pos)
    {
        Camera ca = Camera.main;
        if (ca == null)
            return false;
        Vector3 point = ca.WorldToViewportPoint(pos);
        bool b = point.x >= 0 && point.x <= 1 && point.y >= 0 && point.y <= 1;
        return b;
    }
    public static int mTileLayerMask = 1 << LayerMask.NameToLayer("MapTiles");
    //点击屏幕返回选中的对象
    public static RaycastHit GetScreenPointToRay(Vector3 pos)
    {
        pos.z = 0f;
        RaycastHit hit = new RaycastHit();
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);
            Physics.Raycast(ray, out hit, 1000f, mTileLayerMask);
        }
        //ushort x,y;
        //Frame.PageSecletSceneStyle.mCurStyle.GetXY(hit.point,out x,out y);
        
        //hit.point = new Vector3((float)x,0f,(float)y);
        //Debug.Log(hit.point);
        return hit;
    }
    public static bool IsDown()
    {
        bool isDown = false;
#if UNITY_EDITOR
        isDown = Input.GetMouseButtonDown(0);
#else    
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            isDown = touch.phase == TouchPhase.Began;
        }
#endif
        return isDown;
    }
    public static bool IsUp()
    {
        bool isUp = false;
#if UNITY_EDITOR
        isUp = Input.GetMouseButtonUp(0);
#else    
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            isUp = touch.phase == TouchPhase.Ended;
        }
#endif
        return isUp;
    }
    public static void DestroyImmediateEx(GameObject aGameObject)
    {
        if(aGameObject!=null)
        {
            GameObject.DestroyImmediate(aGameObject,true);
        }
    }

    public static Color NewColor(float r, float g, float b, float a)
    {
        return new Color(r, g, b, a);
    }

    public static FileInfo GetFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            FileInfo info = new FileInfo(fileName);
            return info;
        }
        return null;
    }
	//域名转ip地址，如果传入的就是ip地址那直接返回该地址
	public static string DNSUrl2IP(string url)
	{
        Uri uri = new Uri(url);
        string ip =  DNSHost2IP(uri.Host);
        if (ip == ErrorDNS)
            return ErrorDNS;
        string newUrl = url.Replace(uri.Authority, ip + ":" + uri.Port);
        return newUrl;
	}
    public static string DNSHost2IP(string host)
    {
        try
        {
            System.Net.IPAddress ip;
            if (System.Net.IPAddress.TryParse(host, out ip))
                return ip.ToString();
            else
            {
                System.Net.IPHostEntry ipHost = System.Net.Dns.GetHostEntry(host);
                return ipHost.AddressList[0].ToString();
            }
        }
        catch (Exception)
        {
            if (MUtil.NetAvailable)
                Debug.LogError("解析错误：" + host);
        }
        return ErrorDNS;
    }
    public static string ErrorDNS = "errorhost";
	/// <summary>
	/// 上传数据转UTF-8编码
	/// </summary>
	/// <returns>The URL encode.</returns>
	/// <param name="strCode">String code.</param>
	public static string ToUrlEncode(string strCode)
	{
		StringBuilder sb = new StringBuilder();
		byte[] byStr = System.Text.Encoding.UTF8.GetBytes(strCode); //默认是System.Text.Encoding.Default.GetBytes(str) 
		System.Text.RegularExpressions.Regex regKey = new System.Text.RegularExpressions.Regex("^[A-Za-z0-9]+$");
		for (int i = 0; i < byStr.Length; i++)
		{
			string strBy = Convert.ToChar(byStr[i]).ToString();
			if (regKey.IsMatch(strBy))
			{
				//是字母或者数字则不进行转换  
				sb.Append(strBy);
			}
			else
			{
				sb.Append(@"%" + Convert.ToString(byStr[i], 16));
			}
		}
		return (sb.ToString());
	}
    public static string GetIdef()
    {
        return SystemInfoUtil.deviceUniqueIdentifier;
    }

    public static string GetHighDefinitionUrl(string url)
    {
        string hdUrl = "";
        string path = url.Substring(0, url.LastIndexOf("/") + 1);
        string name = url.Substring(url.LastIndexOf("/") + 1);
        string[] strs = name.Split('.');
        strs[0] = strs[0].Remove(strs[0].Length -2);
        hdUrl = string.Join(".", strs);
        return path + hdUrl;
    }
    public static bool IsDestroy(MonoBehaviour mono)
    {
        return mono == null;
    }


    public static void SetParent(GameObject parent, GameObject mGo)
    {
        Transform t = mGo.transform;
        t.SetParent(parent.transform);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        if (t is RectTransform)
        {
            (t as RectTransform).sizeDelta = Vector2.zero;
        }
    }
    public static void SetParticleScale(GameObject go, float scaleFactor)
    {
        ParticleSystem[] systems = go.GetComponentsInChildren<ParticleSystem>();

        for (int i=0;i<systems.Length;i++)
        {
            ParticleSystem.MainModule main = systems[i].main;
            System.Type type = main.GetType();
            PropertyInfo property = type.GetProperty("startSpeed");
            main.startSpeed = SetMinMaxCurveScale(main.startSpeed, scaleFactor);
            property.SetValue(systems[i].main, main.startSpeed, null);

            property = type.GetProperty("startSize");
            main.startSize = SetMinMaxCurveScale(main.startSize, scaleFactor);
            property.SetValue(systems[i].main, main.startSize, null);

            property = type.GetProperty("gravityModifier");
            main.gravityModifier = SetMinMaxCurveScale(main.gravityModifier, scaleFactor);
            property.SetValue(systems[i].main, main.gravityModifier, null);
        }
    }
    public static UnityEngine.ParticleSystem.MinMaxCurve SetMinMaxCurveScale(UnityEngine.ParticleSystem.MinMaxCurve curve,float scale)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                curve.constant *= scale;
                break;
            case ParticleSystemCurveMode.Curve:
                curve.curveMultiplier *= scale;
                break;
            case ParticleSystemCurveMode.TwoConstants:
                curve.constantMin *= scale;
                curve.constantMax *= scale;
                break;
            case ParticleSystemCurveMode.TwoCurves:
                curve.curveMultiplier *= scale;
                break;
            default:
                break;
        }
        return curve;
    }

	public static float GetTimeNowFloat()
	{        
		return Time.realtimeSinceStartup;
	}

	public static long GetTimeMillons() {
		return System.DateTime.Now.Millisecond;
	}

    /// <summary>
    /// Better than Transform.RotateAround
    /// RotateAround(Camera.main.transform, transform.position, Camera.main.transform.up, -dragDirection.x);
    /// RotateAround(Camera.main.transform, transform.position, Camera.main.transform.right, dragDirection.y);
    /// </summary>
    void RotateAround(Transform transform, Vector3 center, Vector3 axis, float angle)
    {
        Vector3 pos = transform.position;
        Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        Vector3 dir = pos - center; 						// find current direction relative to center
        dir = rot * dir; 									// rotate the direction
        transform.position = center + dir; 					// define new position
        // rotate object to keep looking at the center:
        Quaternion myRot = transform.rotation;
        transform.rotation *= Quaternion.Inverse(myRot) * rot * myRot;
    }

    public static void SetTransformByTarget(Transform source,Transform target)
    {
        source.position = target.position;
        source.rotation = target.rotation;
    }
    public static void SetRotationOptimize(Transform source, Transform target)
    {
        source.rotation = target.rotation;
        Vector3 euler = source.eulerAngles;
        euler.Set(Mathf.Floor(euler.x),Mathf.Floor(euler.y),Mathf.Floor(euler.z));
    }
    public static void SetRotation(Transform source, Transform target)
    {
        source.rotation = target.rotation;
    }
    public static void RotateObject(Transform source, float x, float y, float z)
    {
        source.Rotate(x, y, z);
    }
    public static void  RotateObjectX(Transform source,float v)
    {
        source.Rotate(v, 0f, 0f);
    }
    public static void RotateObjectY(Transform source, float v)
    {
        source.Rotate(0f, v, 0f);
    }
    public static void RotateObjectZ(Transform source, float v)
    {
        source.Rotate(0f, 0f, v);
    }
    public static GameObject CloneGameObejct(GameObject source, GameObject parent)
    {
        if(parent == null)
            return GameObject.Instantiate(source) as GameObject;
        GameObject go = GameObject.Instantiate(source, Vector3.zero, Quaternion.identity, parent.transform) as GameObject;
        go.name = source.name;
        return go;
    }
    public static Component CloneReturnComp(Component comp)
    {
        Transform t = comp.transform;
        GameObject go = GameObject.Instantiate(comp.gameObject, t.position, t.rotation, t.parent) as GameObject;
        //go.transform.SetParent(comp.transform.parent);
        go.name = comp.name;
        go.SetActive(true);
        return go.GetComponent(comp.GetType());
    }
    public static Transform GetTransform(GameObject go, string nodeName)
    {
        Transform[] ts = go.GetComponentsInChildren<Transform>(true);
        for(int i=0;i<ts.Length;i++)
        {
            if (ts[i].name == nodeName)
                return ts[i];
        }
        return null;
    }
    

    public static void StaticCombine(GameObject go)
    {
        if (go == null)
            return;
        StaticBatchingUtility.Combine(go);
    }
    public static void SetShaderLod(int lod)
    {
        Shader.globalMaximumLOD = lod;
    }
    public static float floatTryParse(string str)
    {
        float v = 0;
        float.TryParse(str, out v);
        return v;
    }
    public static ushort ushortTryParse(string str)
    {
        ushort v = 0;
        ushort.TryParse(str, out v);
        return v;
    }
    public static int intTryParse(string str)
    {
        int v = 0;
        int.TryParse(str, out v);
        return v;
    }

    public static byte[] EncodeToPNG(Texture2D tx)
    {
        return tx.EncodeToPNG();
    }
    public static byte[] EncodeToJPG(Texture2D tx)
    {
        return tx.EncodeToJPG();
    }
    public static bool LoadImage(Texture2D tx, byte[] data)
    {        
        return tx.LoadImage(data);
    }


	public static void LoadSences(string sSencesFileName){
		UnityEngine.SceneManagement.SceneManager.LoadScene(sSencesFileName,UnityEngine.SceneManagement.LoadSceneMode.Single);        
	}

    public static FollowTarget SetFollowTarget(GameObject go,GameObject target)
    {
        FollowTarget follow = go.AddComp<FollowTarget>();
        if (target == null)
            follow.target = null;
        else
            follow.target = target.transform;
        return follow;
    }

    public static void AddForceAtPosition(Rigidbody riBody, Vector3 vForce, Vector3 vPos, int fMode)
    {
        riBody.AddForceAtPosition(vForce, vPos, (ForceMode)fMode);
    }

    public static void AddForce(Rigidbody riBody, Vector3 vPos, int fMode)
    {
        riBody.AddForce(vPos, (ForceMode)fMode);
    }

    public static void AddForce(Rigidbody riBody, Vector3 vPos)
    {
        riBody.AddForce(vPos);
    }
    public static bool IsExists(string name, int type)
    {
        if (type == 11)
        {
            return File.Exists("Assets/ArtRes/Audio/" + name + ".wav") || File.Exists("Assets/ArtRes/Audio/" + name + ".mp3");
        }
        return false;
    }

}