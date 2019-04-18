using UnityEngine;
using System.Collections;

namespace highlight
{
    public enum eColorType
    {
        White,
        Green,
        Blue,
        Purple,
        Orange,
        Golden,
        Red,
    }
    public static class ColorUtil
    {
        public static Color red { get{return GetColor(234, 45, 45);} }
        public static Color green { get { return GetColor(0, 255, 0); } }
        public static Color orange { get { return GetColor(255, 134, 13); } }
        public static Color yellow { get { return GetColor(255, 255, 166); } }
        public static Color white { get { return GetColor(255, 255, 255); } }
        public static Color pink { get { return GetColor(255, 45, 214); } }
        public static Color gray { get { return GetColor(25, 25, 25, 151); } }
        public static Color blue { get { return GetColor(0, 168, 255); } }
        public static Color purple { get { return GetColor(219, 64, 242); } }
        public static Color golden { get { return GetColor(255, 255, 0); } }

        public static Color world = Color.white;
        public static Color camp = Color.white;
        public static Color guild = Color.white;
        public static Color team = Color.white;
        public static Color whisper = Color.white;
        public static Color system = Color.white;
        public static Color lucid { get { return GetColor(255, 255, 255, 0); } }


        public static Color GetColor(uint value, float alpha = 1f)
        {
            int r = (int)(value >> 16);
            int g = (int)((value >> 8) - (value >> 16 << 8));
            int b = (int)(value - (value >> 8 << 8));
            //Debug.Log("r:" + r + ", g:" + g + ", b:" + b);
            return GetColor(r, g, b, alpha);
        }
        public static Color GetColor(int r, int g, int b, float alpha = 1f)
        {
            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }
        public static Color GetColor(string[] str)
        {
            if (str.Length != 3)
                return Color.white;
            return GetColor(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]));
        }

        public static Color GetColor(eColorType type)
        {
            Color c;
            switch (type)
	        {
		        case eColorType.White:
                     c = white;
                     break;
                case eColorType.Green:
                     c = green;
                    break;
                case eColorType.Blue:
                    c = blue;
                    break;
                case eColorType.Purple:
                    c = purple;
                    break;
                case eColorType.Orange:
                    c = orange;
                    break;
                case eColorType.Golden:
                    c = golden;
                    break;
                case eColorType.Red:
                    c = red;
                    break;
                default:
                    c = Color.black;
                    break;
	        }
            return c;
        }
        public static Color StringToColor(string str)
        {
            string[] cos = str.Split(',');
            if (cos.Length != 3)
                return Color.white;
            return GetColor(int.Parse(cos[0]), int.Parse(cos[1]), int.Parse(cos[2]));
        }
        public static string To16(this Color c)
        {
            //return Mathf.RoundToInt(c.r * 255f).ToString("X6") + Mathf.RoundToInt(c.g * 255f).ToString("X6") + Mathf.RoundToInt(c.b * 255f).ToString("X6") + Mathf.RoundToInt(c.a * 255f).ToString("X6");
            int retVal = 0;
            retVal |= Mathf.RoundToInt(c.r * 255) << 24;
            retVal |= Mathf.RoundToInt(c.g * 255) << 16;
            retVal |= Mathf.RoundToInt(c.b * 255) << 8;
            retVal |= Mathf.RoundToInt(c.a * 255f);
            retVal = 0xffffff & (retVal >> 8);
            return retVal.ToString("X6");
        }
    }
}
