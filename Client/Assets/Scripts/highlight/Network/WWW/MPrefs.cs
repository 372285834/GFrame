using UnityEngine;
using System.Collections;
using System.Text;

public class MPrefs  {	
	
	/// <summary>
	/// Gets the string.
	/// </summary>
	/// <returns>
	/// The string.
	/// </returns>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='sDefault'>
	/// S default.
	/// </param>/
	public static string GetString(string key,string sDefault)
	{		
		string sValue =  PlayerPrefs.GetString(key);
        if (sValue == null || sValue.Length <= 0)
            return sDefault;
		sValue = WWW.UnEscapeURL(sValue);
//		MDebug.Log("Key:"+key+"="+sValue+"-"+sValue.Length);
		if(sValue != null && sValue.Length > 0)
			return sValue;
		return sDefault;
	}

    public static bool GetBool(string key, bool sDefault)
    {        
        string sValue = PlayerPrefs.GetString(key);
        if (sValue == null || sValue.Length <= 0)
            return sDefault;
        sValue = WWW.UnEscapeURL(sValue);
        //		MDebug.Log("Key:"+key+"="+sValue+"-"+sValue.Length);
        if (sValue != null && sValue.Length > 0 && sValue.Equals("1"))
            return true;

        if (sValue != null && sValue.Length > 0 && sValue.Equals("0"))
            return false;

        return sDefault;
    }
	
	/// <summary>
	/// Gets the int.
	/// </summary>
	/// <returns>
	/// The int.
	/// </returns>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='iDefault'>
	/// I default.
	/// </param>
	public static int GetInt(string key,int iDefault)
	{
		string sValue =  PlayerPrefs.GetString(key);
        if (sValue == null || sValue.Length <= 0)
            return iDefault;
		sValue = WWW.UnEscapeURL(sValue);
		if(sValue != null && sValue.Length > 0)
            int.TryParse(sValue, out iDefault);
		return iDefault;
	}
    public static float GetFloat(string key, float iDefault)
    {
        string sValue = PlayerPrefs.GetString(key);
        if (sValue == null || sValue.Length <= 0)
            return iDefault;
        sValue = WWW.UnEscapeURL(sValue);
        if (sValue != null && sValue.Length > 0)
             float.TryParse(sValue,out iDefault);
        return iDefault;
    }
	public static short GetShort(string key,int sDefault)
	{
        short mShort = (short)sDefault;
		string sValue =  PlayerPrefs.GetString(key);
        if (sValue == null || sValue.Length <= 0)
            return mShort;
		sValue = WWW.UnEscapeURL(sValue);
		if(sValue != null && sValue.Length > 0)
            short.TryParse(sValue, out mShort);
        return mShort;
	}
	
	/// <summary>
	/// Sets the string.
	/// </summary>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='sValue'>
	/// S value.
	/// </param>
	public static void SetString(string key,string sValue)
	{		 
		sValue = WWW.EscapeURL(sValue);
		PlayerPrefs.SetString(key,sValue);		
	}
		
	
	public static void SetString(string key,int sValue)
	{	
		MPrefs.SetString(key,sValue.ToString());		
	}
    public static void SetString(string key, float sValue)
    {
        MPrefs.SetString(key, sValue.ToString());
    }
    public static void SetBool(string key, bool sValue)
    {
        MPrefs.SetString(key, sValue?"1":"0");
    }
	
	public static void Save()
	{
		PlayerPrefs.Save();
	}
//#if UNITY_EDITOR
    public static void DeleteAll(){
        PlayerPrefs.DeleteAll();
    }
//#endif
    
}
