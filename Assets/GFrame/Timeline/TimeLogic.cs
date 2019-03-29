using System.Collections;
using System.Collections.Generic;


namespace highlight.timeline
{
    public abstract class TimeLogic
    {
        public int index;
        public TimeObject timeObject { private set; get; }
        public TimeStyle time
        {
            get
            {
                return this.timeObject.mStyle;
            }
        }
        public TimeComponent component { private set; get; }
        public List<TimeLogic> GetComponents { get { return timeObject.GetComponents; } }

        #region virtual Function
        public virtual void OnInit() { }
        public virtual void OnDestroy() { }// timeline 销毁

        public virtual void OnTrigger(int framesSinceTrigger, float timeSinceTrigger) { }
        public virtual void OnUpdateEvent(int framesSinceTrigger, float timeSinceTrigger) { }
        public virtual void OnFinish() { } // event完成
        public virtual void OnStop() { } //timeline 完成
        public virtual void OnResume() { }
        public virtual void OnPause() { }
        #endregion


        private readonly static Dictionary<string, ObjectPool> logicPoolDic = new Dictionary<string, ObjectPool>();

        public static TimeLogic Get(TimeComponent comp, TimeObject t)
        {
            if (comp == null)
                return null;
            TimeLogic logic = getPool(comp).Get(comp.Attr.dataType) as TimeLogic;
            logic.timeObject = t;
            logic.component = comp;
            return logic;
        }
        public static void Release(TimeLogic logic)
        {
            if (logic == null)
                return;
            getPool(logic.component).Release(logic);
            logic.timeObject = null;
            logic.component = null;
        }
        static ObjectPool getPool(TimeComponent data)
        {
            ObjectPool pool = null;
            logicPoolDic.TryGetValue(data.TypeName, out pool);
            if (pool == null)
            {
                pool = new ObjectPool();
                logicPoolDic[data.TypeName] = pool;
            }
            return pool;
        }
    }
}

