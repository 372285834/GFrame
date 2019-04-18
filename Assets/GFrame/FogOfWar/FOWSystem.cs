using UnityEngine;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// 说明：由TasharenFogOfWar插件移植而来，NGUI作者发布
///       对原模块进行了精简和优化，具体有：
///     1）Revealer类改为IFOWRevealer，适应项目扩展，并使用缓存池，避免GC
///     2）去掉高度图HeightMap
///     3）只留下使用radius刷图的方式，其它刷图方式一并移除
///     4）Shader后处理改为网格模型方式
///     5）Shader纹理贴图由原来的两张优化到一张
///     6）增加多个控制选项：系统开关、渲染开关、迷雾开关等
/// 
/// 注意：
///     1）FOWSysetem作为战争迷雾表现的数据模型系统已经足够，扩展时不应该牵涉到游戏逻辑
/// 
/// TODO：可做的优化
///     1）静态视野（如建筑）理论上不需要每次都去刷，一次刷新后可Cache，考虑Cache策略
///     2）同理，不动的角色也可以设置“脏位”决定是否刷新
/// 
/// @by wsh 2017-05-18
/// </summary>

public class FOWSystem : MonoSingleton<FOWSystem>
{
    [System.Serializable]
    public class Setting
    {
        /// <summary>
        /// Size of your world in units. For example, if you have a 256x256 terrain, then just leave this at '256'.
        /// </summary>
        /// 
        public float worldSize = 512f;

        /// <summary>
        /// Size of the fog of war texture. Higher resolution will result in more precise fog of war, at the cost of performance.
        /// </summary>

        public int textureSize = 512;

        /// <summary>
        /// How frequently the visibility checks get performed.
        /// </summary>

        public float updateFrequency = 0.3f;

        /// <summary>
        /// How long it takes for textures to blend from one to another.
        /// </summary>

        public float textureBlendTime = 0.3f;

        /// <summary>
        /// How many blur iterations will be performed. More iterations result in smoother edges.
        /// Blurring happens on a separate thread and does not affect performance.
        /// </summary>

        public int blurIterations = 2;

        /// <summary>
        /// How many offset radius will be adjust. 
        /// </summary>

        public float radiusOffset = 0f;

        /// <summary>
        /// If disable it, the whole system do not work
        /// </summary>

        public bool enableSystem = true;

        /// <summary>
        /// If disable it, the whole rendering do not work
        /// </summary>

        public bool enableRender = true;

        /// <summary>
        /// If disable it, the there only remnant left
        /// </summary>

        public bool enableFog = true;

        /// <summary>
        /// If debugging is enabled, the time it takes to calculate the fog of war will be shown in the log window.
        /// </summary>

        public bool enableDebug = false;
        public bool optimize = true;
        public int blurRadiu = 3;

        public float elapsed;

        // 这里设置战争迷雾颜色
        public Color unexploredColor = new Color(0f, 0f, 0f, 250f / 255f);
        public Color exploredColor = new Color(0f, 0f, 0f, 150f / 255f);
    }
    public enum State
    {
        Blending,
        NeedUpdate,
        UpdateTexture,
    }
    public Setting setting;
    protected Transform mTrans;
	protected Vector3 mOrigin = Vector3.zero;
	protected Vector3 mSize = Vector3.one;

	// Revealers that the thread is currently working with
	static BetterList<IFOWRevealer> mRevealers = new BetterList<IFOWRevealer>();

	// Revealers that have been added since last update
	static BetterList<IFOWRevealer> mAdded = new BetterList<IFOWRevealer>();

	// Revealers that have been removed since last update
	static BetterList<IFOWRevealer> mRemoved = new BetterList<IFOWRevealer>();

	// Color buffers -- prepared on the worker thread.
	protected Color32[] mBuffer0;
	protected Color32[] mBuffer1;
	protected Color32[] mBuffer2;

	// textures -- we'll be blending in the shader
	protected Texture2D mTexture;

	// Whether some color buffer is ready to be uploaded to VRAM
	protected float mBlendFactor = 0f;
	protected float mNextUpdate = 0f;
	protected State mState = State.Blending;

	Thread mThread;
    volatile bool mThreadWork;

    protected int mTextureSizeSqr;
	

	/// <summary>
	/// The fog texture we're blending
	/// </summary>

	public Texture2D texture { get { return mTexture; } }
    
	/// <summary>
	/// Factor used to blend between the texture.
	/// </summary>

	public float blendFactor { get { return mBlendFactor; } }

    /// <summary>
    /// Render immediately
    /// </summary>
    public void RenderImmediately()
    {
        UpdateBuffer();
        mState = State.UpdateTexture;
        UpdateTexture();
        mBlendFactor = 1.0f;
    }

    /// <summary>
    /// Create a new fog revealer.
    /// </summary>

    static public void AddRevealer (IFOWRevealer rev)
	{
        if (rev != null)
        {
            lock (mAdded) mAdded.Add(rev);
        }
	}

	/// <summary>
	/// Delete the specified revealer.
	/// </summary>

	static public void RemoveRevealer (IFOWRevealer rev)
    {
        if (rev != null)
        {
            lock (mRemoved) mRemoved.Add(rev);
        }
    }

    /// <summary>
    /// Generate the height grid.
    /// </summary>

    public void Startup(Setting _set)
	{
        this.setting = _set;
		mTrans = transform;
        // TODO：实际项目中，自己根据场景地图设置位置信息
        // 这里为了简单，战争迷雾Transform直接居中，原点位置为左下角
        float halfw = setting.worldSize * 0.5f;
        mTrans.localPosition = new Vector3(halfw, 0f, halfw);
		mOrigin = mTrans.position;
        mOrigin.x -= halfw;
        mOrigin.z -= halfw;

        mTextureSizeSqr = setting.textureSize * setting.textureSize;
		mBuffer0 = new Color32[mTextureSizeSqr];
		mBuffer1 = new Color32[mTextureSizeSqr];
		mBuffer2 = new Color32[mTextureSizeSqr];
        mRevealers.Clear();
        mRemoved.Clear();
        mAdded.Clear();
        
        // Add a thread update function -- all visibility checks will be done on a separate thread
        mThread = new Thread(ThreadUpdate);
        mThreadWork = true;
        mThread.Start();
    }

    /// <summary>
    /// Ensure that the thread gets terminated.
    /// </summary>

    public override void Dispose()
    {
        if (mThread != null)
        {
            mThreadWork = false;
            mThread.Join();
            mThread = null;
        }

        mBuffer0 = null;
        mBuffer1 = null;
        mBuffer2 = null;

        if (mTexture != null)
        {
            Destroy(mTexture);
            mTexture = null;
        }
    }

	/// <summary>
	/// Is it time to update the visibility? If so, flip the switch.
	/// </summary>

	void Update ()
	{
        if (setting == null || !setting.enableSystem)
        {
            return;
        }

		if (setting.textureBlendTime > 0f)
		{
			mBlendFactor = Mathf.Clamp01(mBlendFactor + Time.deltaTime / setting.textureBlendTime);
		}
		else mBlendFactor = 1f;

		if (mState == State.Blending)
		{
			float time = Time.time;

			if (mNextUpdate < time)
			{
				mNextUpdate = time + setting.updateFrequency;
				mState = State.NeedUpdate;
			}
		}
		else if (mState != State.NeedUpdate)
		{
			UpdateTexture();
		}
	}

    /// <summary>
    /// If it's time to update, do so now.
    /// </summary>

    void ThreadUpdate()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        bool isFirst = true;
        while (mThreadWork)
        {
            if (mState == State.NeedUpdate)
            {
                sw.Reset();
                sw.Start();
                UpdateBuffer(isFirst);
                isFirst = false;
                sw.Stop();
                setting.elapsed = (float)sw.ElapsedMilliseconds;
                mState = State.UpdateTexture;
            }
            Thread.Sleep(1);
        }
#if UNITY_EDITOR
        Debug.Log("FOW thread exit!");
#endif
    }

	/// <summary>
	/// Show the area covered by the fog of war.
	/// </summary>

	void OnDrawGizmosSelected ()
	{
        if (setting == null)
            return;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(setting.worldSize, 0f, setting.worldSize));
	}

    /// <summary>
    /// Update the fog of war's visibility.
    /// </summary>
    public HashSet<int> idxHash = new HashSet<int>();
    private int lastIdxNum = 0;
    int clearDelay = 0;
    //public HashSet<int> lastHash = new HashSet<int>();
    void UpdateBuffer (bool force = false)
	{
		// Add all items scheduled to be added
		if (mAdded.size > 0)
		{
			lock (mAdded)
			{
				while (mAdded.size > 0)
				{
					int index = mAdded.size - 1;
					mRevealers.Add(mAdded.buffer[index]);
					mAdded.RemoveAt(index);
				}
			}
		}

		// Remove all items scheduled for removal
		if (mRemoved.size > 0)
		{
			lock (mRemoved)
			{
				while (mRemoved.size > 0)
				{
					int index = mRemoved.size - 1;
					mRevealers.Remove(mRemoved.buffer[index]);
					mRemoved.RemoveAt(index);
				}
			}
		}

		// Use the texture blend time, thus estimating the time this update will finish
		// Doing so helps avoid visible changes in blending caused by the blended result being X milliseconds behind.
		float factor = (setting.textureBlendTime > 0f) ? Mathf.Clamp01(mBlendFactor + setting.elapsed / setting.textureBlendTime) : 1f;
        bool isOptimize = setting.optimize && !force;
        if (isOptimize)
        {
            foreach (var i in idxHash)
            {
                mBuffer0[i] = Color32.Lerp(mBuffer0[i], mBuffer1[i], factor);
                mBuffer1[i].r = 0;
            }
        }
        else
        {
            // Clear the buffer's red channel (channel used for current visibility -- it's updated right after)
            for (int i = 0, imax = mBuffer0.Length; i < imax; ++i)
            {
                mBuffer0[i] = Color32.Lerp(mBuffer0[i], mBuffer1[i], factor);
                mBuffer1[i].r = 0;
            }
        }
        // For conversion from world coordinates to texture coordinates
        float worldToTex = (float)setting.textureSize / setting.worldSize;

		// Update the visibility buffer, one revealer at a time
		for (int i = 0; i < mRevealers.size; ++i)
		{
			IFOWRevealer rev = mRevealers[i];
            if (rev.IsValid())
            {
                RevealUsingRadius(rev, worldToTex, isOptimize);
            }
        }
        if (isOptimize)
        {
            foreach (var index in idxHash)
            {
                if (mBuffer1[index].g < mBuffer1[index].r)
                {
                    mBuffer1[index].g = mBuffer1[index].r;
                }
                mBuffer0[index].b = mBuffer1[index].r;
                mBuffer0[index].a = mBuffer1[index].g;
            }
        }
        else
        {
            for (int index = 0; index < mTextureSizeSqr; ++index)
            {
                byte r = mBuffer1[index].r;
                if (mBuffer1[index].g < r)
                {
                    mBuffer1[index].g = r;
                }
                mBuffer0[index].b = r;
                mBuffer0[index].a = mBuffer1[index].g;
                // if(mBuffer0[index].r > 0)
                //     idxHash.Add(index);
            }
            // Blur the final visibility data
            //for (int i = 0; i < blurIterations; ++i) BlurVisibility();

            // Reveal the map based on what's currently visible
            //   RevealMap();

            // Merge two buffer to one
            //   MergeBuffer();
        }
        if (factor == 1f && lastIdxNum > 3000 && lastIdxNum == idxHash.Count)
        {
            clearDelay++;
            if (clearDelay >= 10)
            {
                clearDelay = 0;
                //foreach (var index in idxHash)
                //{
                //    mBuffer0[index].r = mBuffer1[index].r;
                //    mBuffer0[index].b = mBuffer1[index].r;
                //    mBuffer0[index].g = mBuffer1[index].g;
                //    mBuffer0[index].a = mBuffer1[index].g;
                //}
                idxHash.Clear();
            }
        }
        else
            clearDelay = 0;
        lastIdxNum = idxHash.Count;
    }

	/// <summary>
	/// The fastest form of visibility updates -- radius-based, no line of sights checks.
	/// </summary>

	void RevealUsingRadius (IFOWRevealer r, float worldToTex,bool isOptimize)
    {
        int size = setting.textureSize;
        // Position relative to the fog of war
        Vector3 pos = (r.GetPosition() - mOrigin) * worldToTex;
        float radius = r.GetRadius() * worldToTex - setting.radiusOffset;

        // Coordinates we'll be dealing with
        int xmin = Mathf.RoundToInt(pos.x - radius);
		int ymin = Mathf.RoundToInt(pos.z - radius);
		int xmax = Mathf.RoundToInt(pos.x + radius);
		int ymax = Mathf.RoundToInt(pos.z + radius);

		int cx = Mathf.RoundToInt(pos.x);
		int cy = Mathf.RoundToInt(pos.z);

		cx = Mathf.Clamp(cx, 0, size - 1);
		cy = Mathf.Clamp(cy, 0, size - 1);

		int radiusSqr = Mathf.RoundToInt(radius * radius);

		for (int y = ymin; y < ymax; ++y)
		{
			if (y > -1 && y < size)
			{
				int yw = y * size;

				for (int x = xmin; x < xmax; ++x)
				{
					if (x > -1 && x < size)
					{
						int xd = x - cx;
						int yd = y - cy;
						int dist = xd * xd + yd * yd;

						// Reveal this pixel
						if (dist < radiusSqr) mBuffer1[x + yw].r = 255;
					}
				}
			}
		}
        xmin = Mathf.Clamp(xmin - setting.blurRadiu, 0, xmin);
        xmax = Mathf.Clamp(xmax + setting.blurRadiu, 0, size - 1);
        ymin = Mathf.Clamp(ymin - setting.blurRadiu, 0, ymin);
        ymax = Mathf.Clamp(ymax + setting.blurRadiu, 0, size - 1);
        for (int i = 0; i < setting.blurIterations; ++i) BlurVisibility(xmin,xmax,ymin,ymax, isOptimize);
    }

	/// <summary>
	/// Blur the visibility data.
	/// </summary>

	void BlurVisibility (int xmin, int xmax, int ymin, int ymax,bool isOptimize)
    {
        Color32 c;
        int size = setting.textureSize;
        for (int y = ymin; y < ymax; ++y)
        {
            int yw = y * size;
            int yw0 = (y - 1);
            if (yw0 < 0) yw0 = 0;
            int yw1 = (y + 1);
            if (yw1 == size) yw1 = y;

            yw0 *= size;
            yw1 *= size;

            for (int x = xmin; x < xmax; ++x)
            {
                int x0 = (x - 1);
                if (x0 < 0) x0 = 0;
                int x1 = (x + 1);
                if (x1 == size) x1 = x;

                int index = x + yw;
                int val = mBuffer1[index].r;

                val += mBuffer1[x0 + yw].r;
                val += mBuffer1[x1 + yw].r;
                val += mBuffer1[x + yw0].r;
                val += mBuffer1[x + yw1].r;

                val += mBuffer1[x0 + yw0].r;
                val += mBuffer1[x1 + yw0].r;
                val += mBuffer1[x0 + yw1].r;
                val += mBuffer1[x1 + yw1].r;

                c = mBuffer2[index];
                c.r = (byte)(val / 9);
                mBuffer2[index] = c;
                if(isOptimize)
                    idxHash.Add(index);
            }
        }

        // Swap the buffer so that the blurred one is used
        Color32[] temp = mBuffer1;
        mBuffer1 = mBuffer2;
        mBuffer2 = temp;
    }

	/// <summary>
	/// Reveal the map by updating the green channel to be the maximum of the red channel.
	/// </summary>

	void RevealMap ()
	{
		for (int index = 0; index < mTextureSizeSqr; ++index)
        {
            if (mBuffer1[index].g < mBuffer1[index].r)
            {
                mBuffer1[index].g = mBuffer1[index].r;
            }
        }
	}

    void MergeBuffer()
    {
        for (int index = 0; index < mTextureSizeSqr; ++index)
        {
            mBuffer0[index].b = mBuffer1[index].r;
            mBuffer0[index].a = mBuffer1[index].g;
        }
    }

    /// <summary>
    /// Update the specified texture with the new color buffer.
    /// </summary>

    void UpdateTexture ()
	{
        if (!setting.enableRender)
        {
            return;
        }

		if (mTexture == null)
        {
            // Native ARGB format is the fastest as it involves no data conversion
            mTexture = new Texture2D(setting.textureSize, setting.textureSize, TextureFormat.ARGB32, false);

			mTexture.wrapMode = TextureWrapMode.Clamp;

            mTexture.SetPixels32(mBuffer0);
            mTexture.Apply();
			mState = State.Blending;
		}
		else if (mState == State.UpdateTexture)
		{
			mTexture.SetPixels32(mBuffer0);
			mTexture.Apply();
			mBlendFactor = 0f;
            mState = State.Blending;
        }
	}

	/// <summary>
	/// Checks to see if the specified position is currently visible.
	/// </summary>

    public bool IsVisible(Vector3 pos)
    {
        // 说明：
        // 原判断式为：mBuffer0[index].r > 0 || mBuffer1[index].r > 0, ===> 缺陷是角色消退得太慢，原因是游戏设定混合周期太长导致
        // 之后修改为：Blend(mBuffer0[index], mBuffer1[index]).r > 0, ===> 结果出现诡异的抖动和飞行物全图乱飞的现象
        // 最后确定为：mBuffer0[index].r > value1 || mBuffer1[index].r > value2, ===> 两个value独立可调，前面一个决定消退延迟，后一个决定出现延迟
        //
        // 第二种方式出现Bug的原因是：mBuffer0.rg总是渐变的，而mBuffer1.rg在计算过程中会出现跳变（见351行）
        // 结果导致当判断Blend.r(混合值) > value，value为非0值时，则IsVisible判断结果可能会出现跳动
        // 更具体来说，可能在连续的3帧内，本来持续可见的角色会被判定为出现这样的现象：可见--->不可见--->可见
        // 以上现象对游戏造成的历史bug有：
        // 1）人物血条闪动---本来全程可见的血条在连续的3帧内，中间帧判断不可见导致一次缓存池回收
        // 2）飞行物凭空消失一帧---同理，中间帧误判为不可见导致飞行物突然隐藏一帧又莫名出现
        // 
        // 注意：===>******（很重要）
        // 1）严格来说，角色跑动时依然可能导致逻辑上的可见性跳动===>不一定在连续的3帧内，但在帧数很短时视觉上就会抖动。
        // 2）所以为了防止表现层抖动，理想的处理方式有两种：
        //    A）如果在逻辑层控制表现，则当角色可见以后，设定一个逻辑延迟（如1秒），在这段时间内强行保持角色的可见性
        //    B）表现层的可见性与逻辑分离，使用本函数判断可见性，并且适当调整上述说明的两个value值来“缓和”抖动现象===>******推荐，简单暴力
        //
        // by wsh @ 2017-07-29
        if (mBuffer0 == null || mBuffer1 == null)
        {
            return false;
        }

        pos -= mOrigin;
        float worldToTex = (float)setting.textureSize / setting.worldSize;
        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, setting.textureSize - 1);
        cy = Mathf.Clamp(cy, 0, setting.textureSize - 1);
        int index = cx + cy * setting.textureSize;
        return mBuffer0[index].r > 64 || mBuffer1[index].r > 0;
    }

    /// <summary>
    /// Checks to see if the specified position has been explored.
    /// </summary>

    public bool IsExplored(Vector3 pos)
    {
        if (mBuffer0 == null)
        {
            return false;
        }
        pos -= mOrigin;

        float worldToTex = (float)setting.textureSize / setting.worldSize;
        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, setting.textureSize - 1);
        cy = Mathf.Clamp(cy, 0, setting.textureSize - 1);
        return mBuffer0[cx + cy * setting.textureSize].g > 0;
    }
}