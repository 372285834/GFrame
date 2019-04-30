using System.Collections.Generic;
using System.Diagnostics;

namespace highlight
{

    using TimerEventList = List<TimerEvent>;

	public class Timer
	{
        protected TimerEventList mTimerEvents;
        protected Stopwatch mWatch;
        protected float mTimeLastFrame;
        protected float mTimeSinceLastFrame;
        protected TimerEvent mCurrentEvent;

        public Timer()
        {
            mTimerEvents = new TimerEventList();
            mWatch = new Stopwatch();

            reset();
        }

        public virtual void destroy()
		{
			mWatch.Stop();
		}

        /** Resets timer */
        public void reset()
        {
            mWatch.Reset();
			mWatch.Start();

            mTimeLastFrame = 0;
            mTimeSinceLastFrame = 0;
            mCurrentEvent = null;
        }

        /** Returns milliseconds since initialisation or last reset */
        public ulong getMilliseconds()
        {
            return (ulong)mWatch.ElapsedMilliseconds;
        }

        /** Returns milliseconds since initialisation or last reset */
        public float getSeconds()
        {
            return UnityEngine.Time.realtimeSinceStartup;// mWatch.ElapsedMilliseconds * 0.001f;
        }

        /** Elapsed time in seconds since the last event of the same type,
            i.e. time for a complete frame.*/
        public float getTimeSinceLastFrame()
        {
            return mTimeSinceLastFrame;
        }

        public TimerEvent addTimerEvent(TimerEventType type, float second, TimerEvent.onEventFunc func, object mData = null)
        {
            TimerEvent eve = new TimerEvent(type, getSeconds(), second, func, mData);
            mTimerEvents.Add(eve);

            return eve;
        }

        public void removeTimerEvent(TimerEvent eve)
        {
            mTimerEvents.Remove(eve);
        }

        public TimerEvent getCurrentEvent() { return mCurrentEvent; }

        TimerEventList removeList = new TimerEventList();
        public bool update()
        {
            //float timeElapsed = getSeconds();
            //if (timeElapsed - mTimeLastFrame < 0.001)
            //    return false;

            //mTimeSinceLastFrame = timeElapsed - mTimeLastFrame;
            //mTimeLastFrame = timeElapsed;
            int count = mTimerEvents.Count;
            if (count == 0)
                return true;
            removeList.Clear();
            for (int i = 0; i < count; ++i)
            {
                mCurrentEvent = mTimerEvents[i];

                mCurrentEvent.update();

                if (mCurrentEvent.isDestroy())
                    removeList.Add(mCurrentEvent);    
			}

            for (int i = 0; i < removeList.Count; ++i)
            {
                removeList[i].EndCall();
                mTimerEvents.Remove(removeList[i]);
            }
            return true;
		}
	}
	
}
