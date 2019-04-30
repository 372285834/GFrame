using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace highlight
{
    /// <summary>
    /// 权重对象
    /// </summary>
    public interface IRandomObject
    {
        /// 权重
        int Weight { get; set; }
    }
    public static class MathX
    {
        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <param name="sum">数组长度</param>
        /// <param name="max">最大随机值</param>
        /// <returns></returns>
        public static int[] CreatRandoms(int sum, int max)
        {
            sum = sum > max ? max : sum;
            int[] arr = new int[sum];
            if(sum == max)
            {
                for (int i = 0; i < sum; i++)
                {
                    arr[i] = i;
                }
                Shuffle(arr);
                return arr;
            }
            int j = 0;
            //表示键和值对的集合。
            Hashtable hashtable = new Hashtable();
            System.Random rm = new System.Random();
            for (int i = 0; hashtable.Count < sum; i++)
            {
                //返回一个小于所指定最大值的非负随机数
                int nValue = rm.Next(max);
                //containsValue(object value)   是否包含特定值
                if (!hashtable.ContainsValue(nValue))// && nValue != 0
                {
                    //把键和值添加到hashtable
                    hashtable.Add(nValue, nValue);
                    //Debug.Log(i);
                    arr[j] = nValue;

                    j++;
                }
            }
            return arr;
        }
        /// <summary>
        /// 数组排序，从小到大排序
        /// </summary>
        /// <param name="arr"></param>
        public static void SortArray(int[] arr)
        {
            int temp;
            //最多做n-1趟排序
            for (int i = 0; i < arr.Length - 1; i++)
            {
                //对当前无序区间score[0......length-i-1]进行排序(j的范围很关键，这个范围是在逐步缩小的)
                for (int j = 0; j < arr.Length - i - 1; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }
        }
        /// <summary>
        /// 数组打乱
        /// </summary>
        /// <param name="intArray"></param>
        public static void Shuffle(int[] intArray)
        {
            for (int i = 0; i < intArray.Length; i++)
            {
                int temp = intArray[i];
                int randomIndex = UnityEngine.Random.Range(0, intArray.Length);
                intArray[i] = intArray[randomIndex];
                intArray[randomIndex] = temp;
            }
            //for (int i = 0; i < intArray.Length; i++)
            //{
            //    Debug.Log("第" + num.ToString() + "次随机：" + intArray[i]);
            //}
        }

        public static Bounds calcBounds(GameObject go)
        {
            Vector3 min = Vector3.zero, max = Vector3.zero;

            Renderer[] renders = go.GetComponentsInChildren<Renderer>();
            if (renders.Length != 0)
            {
                min = renders[0].bounds.min;
                max = renders[0].bounds.max;

                for (int i = 1; i < renders.Length; ++i)
                {
                    if (min.x > renders[i].bounds.min.x)
                        min.x = renders[i].bounds.min.x;

                    if (min.y > renders[i].bounds.min.y)
                        min.y = renders[i].bounds.min.y;

                    if (min.z > renders[i].bounds.min.z)
                        min.z = renders[i].bounds.min.z;

                    if (max.x < renders[i].bounds.max.x)
                        max.x = renders[i].bounds.max.x;

                    if (max.y < renders[i].bounds.max.y)
                        max.y = renders[i].bounds.max.y;

                    if (max.z < renders[i].bounds.max.z)
                        max.z = renders[i].bounds.max.z;
                }
            }

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }


        public static float TimeSpanNow(DateTime dt)
        {
            TimeSpan ts = dt - System.DateTime.Now;
            return (float)ts.TotalSeconds;
        }

        /// <summary>  
        /// 线段与圆的交点  
        /// </summary>  
        /// <param name="ptStart">线段起点</param>  
        /// <param name="ptEnd">线段终点</param>  
        /// <param name="ptCenter">圆心坐标</param>  
        /// <param name="Radius2">圆半径平方</param>  
        /// <param name="ptInter1">交点1(若不存在返回65536)</param>  
        /// <param name="ptInter2">交点2(若不存在返回65536)</param>  
        public static bool LineInterCircle(Vector2 ptStart, Vector2 ptEnd, Vector2 ptCenter, double Radius2, ref Vector2 ptInter1, ref Vector2 ptInter2)
        {
            float EPS = 0.00001f;
            ptInter1.x = ptInter2.x = 65536.0f;
            ptInter1.y = ptInter2.y = 65536.0f;
            float fDis = Vector2.Distance(ptStart, ptEnd);
            Vector2 d = (ptEnd - ptStart)/fDis;
            Vector2 E = ptCenter - ptStart;
            float a = E.x * d.x + E.y * d.y;
            float a2 = a * a;
            float e2 = E.x * E.x + E.y * E.y;
            if ((Radius2 * Radius2 - e2 + a2) < 0)
            {
                return false;
            }
            else
            {
                float f = (float)Math.Sqrt(Radius2 - e2 + a2);
                float t = a - f;
                if (((t - 0.0) > -EPS) && (t - fDis) < EPS)
                {
                    ptInter1.x = ptStart.x + t * d.x;
                    ptInter1.y = ptStart.y + t * d.y;
                }
                t = a + f;
                if (((t - 0.0) > -EPS) && (t - fDis) < EPS)
                {
                    ptInter2.x = ptStart.x + t * d.x;
                    ptInter2.y = ptStart.y + t * d.y;
                }
                return true;
            }
        }
        public static T GetRandomByWidthSample<T>(List<T> list)
        {
            if (list == null || list.Count <= 0)
                return default(T);
            int r = UnityEngine.Random.Range(0, list.Count);
            return list[r];
        }
        /// <summary>
        /// 算法：
        /// 1.每个广告项权重+1命名为w，防止为0情况。
        /// 2.计算出总权重n。
        /// 3.每个广告项权重w加上从0到(n-1)的一个随机数（即总权重以内的随机数），得到新的权重排序值s。
        /// 4.根据得到新的权重排序值s进行排序，取前面s最大几个。
        /// </summary>
        /// <param name="list">原始列表</param>
        /// <param name="count">随机抽取条数</param>
        /// <returns></returns>
        public static T GetDataRandomByWidth<T>(List<T> list,int seed=0) where T : IRandomObject
        {
            int idx = GetRandomByWidth(list, seed);
            return idx >= 0 ? list[idx] : default(T);
        }
        public static int GetRandomByWidth<T>(List<T> list, int seed=0) where T : IRandomObject
        {
            if (list == null || list.Count <= 0)
                return -1;
            if (list.Count == 1)
                return 0;
            if(seed > 0)
            {
                UnityEngine.Random.InitState(seed);
            }
            int totalWeights = 0;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Weight = list[i].Weight == 0 ? 1 : list[i].Weight;
                totalWeights += list[i].Weight;  //权重+1，防止为0情况。
            }
            double stepWeightSum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                stepWeightSum += list[i].Weight;
                float rg = UnityEngine.Random.Range(0f,1f);
                if (rg <= stepWeightSum / totalWeights)
                {
                    return i;
                }
            }
            return 0;
        }
        public static List<T> GetRandomList<T>(List<T> list, int count) where T : IRandomObject
        {
            if (list == null || list.Count <= count || count <= 0)
            {
                return list;
            }
            //计算权重总和
            int totalWeights = 0;
            for (int i = 0; i < list.Count; i++)
            {
                totalWeights += list[i].Weight + 1;  //权重+1，防止为0情况。
            }
            //随机赋值权重
            System.Random ran = new System.Random(GetRandomSeed());  //GetRandomSeed()随机种子，防止快速频繁调用导致随机一样的问题 
            List<KeyValuePair<int, int>> wlist = new List<KeyValuePair<int, int>>();    //第一个int为list下标索引、第一个int为权重排序值
            for (int i = 0; i < list.Count; i++)
            {
                int w = (list[i].Weight + 1) + ran.Next(0, totalWeights);   // （权重+1） + 从0到（总权重-1）的随机数
                wlist.Add(new KeyValuePair<int, int>(i, w));
            }
            //排序
            wlist.Sort(
              delegate(KeyValuePair<int, int> kvp1, KeyValuePair<int, int> kvp2)
              {
                  return kvp2.Value - kvp1.Value;
              });

            //根据实际情况取排在最前面的几个
            List<T> newList = new List<T>();
            for (int i = 0; i < count; i++)
            {
                T entiy = list[wlist[i].Key];
                newList.Add(entiy);
            }
            //随机法则
            return newList;
        }

        /// <summary>
        /// 随机种子值
        /// </summary>
        /// <returns></returns>
        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
