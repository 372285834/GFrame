namespace highlight
{

    public enum TimerEventType
    {
        // 每帧触发
        TEVE_ALWAYS,
        // 直到指定的时间内每帧都触发
        TEVE_UNTIL,
        // 在指定的时间点触发一次
        TEVE_ONCE,
        // 延迟指定的时间后每帧都触发
        TEVE_DELAY,
        // 在指定的时间间隔点触发
        TEVE_INTERVAL
    }

	public class TimerEvent
	{
        protected TimerEventType mType;

        /// Source value
        protected float mSource;
        /// Destination value
        public float mTotal;
        public float mCurTime;
        public float lastTime = 0;
        public bool unscaled = false;
        /// Function
        public delegate void onEventFunc(TimerEvent evt);
        protected onEventFunc mFunc;
        protected onEventFunc onEndFunc;
        /// Controller is enabled or not
        bool mEnabled;

		bool mIsDestroy;

        protected object mData;
        public float progress;
        /** Usual constructor.
            @remarks
                Requires source and destination values, and a function object. None of these are destroyed
                with the Controller when it is deleted (they can be shared) so you must delete these as appropriate.
        */
        public TimerEvent(TimerEventType type, float src, float dest, onEventFunc func, object data = null) 
        {
            mType = type;
            mSource = src;
            mTotal = dest;
            mFunc = func;
            mData = data;
            mEnabled = true;
			mIsDestroy = false;
        }
 
        /// Sets the input controller value
        public void setSource(float src)
        {
            mSource = src;
        }
        /// Gets the input controller value
        public float getSource()
        {
            return mSource;
        }
        /// Sets the output controller value
        public void setDestination(float dest)
        {
            mTotal = dest;
        }

        /// Gets the output controller value
        public float getDestination()
        {
            return mTotal;
        }

        /// Returns true if this controller is currently enabled
        public bool getEnabled()
        {
            return mEnabled;
        }

        /// Sets whether this controller is enabled
        public void setEnabled(bool enabled)
        {
            mEnabled = enabled;
        }

        /** Sets the function object to be used by this controller.
        */
        public void setFunction(onEventFunc func)
        {
            mFunc = func;
        }

        /** Returns a pointer to the function object used by this controller.
        */
        public onEventFunc getFunction()
        {
            return mFunc;
        }
        public void setDestroy(bool isDestroy,bool callFun = true)
        {
            mIsDestroy = isDestroy;
            if (callFun)
            {
                mFunc(this);
            }
        }
        public bool isDestroy() { return mIsDestroy; }
        public void SetEndFunc(onEventFunc func)
        {
            onEndFunc = func;
        }
        /** Tells this controller to map it's input controller value
            to it's output controller value, via the controller function. 
        @remarks
            This method is called automatically every frame by ControllerManager.
        */
        public void update()
        {
            if (!mEnabled || mIsDestroy)
                return;
            float timeElapsed = unscaled ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;

            mCurTime += timeElapsed;
            switch (mType)
            {
                case TimerEventType.TEVE_ALWAYS:
                    {
                        mFunc(this);
                        break;
                    }
                case TimerEventType.TEVE_UNTIL:
                    {
                        float t = mCurTime;
                        progress = t / mTotal;
                        if (progress >= 1f)
                        {
                            progress = 1f;
                            mIsDestroy = true;
                        }
                        mFunc(this);
                        break;
                    }
                case TimerEventType.TEVE_ONCE:
                    {
                        float t = mCurTime;
                        if (t >= mTotal)
                        {
                            if (mFunc == null)
                                t = 0;

                            mFunc(this);
                            mIsDestroy = true;
                        }

                        break;
                    }

                case TimerEventType.TEVE_DELAY:
                    {
                        float t = mCurTime;
                        if (t >= mTotal)
                            mFunc(this);

                        break;
                    }
                case TimerEventType.TEVE_INTERVAL:
                    {
                        if (mCurTime - lastTime > mTotal)
                        {
                            lastTime = mCurTime;
                            mFunc(this);
                        }
                        break;
                    }
            }    
        }
        public void EndCall()
        {
            if (onEndFunc != null)
                onEndFunc(this);
        }
        public void setData(object data) { mData = data; }
        public object getData() { return mData; }
	}

}