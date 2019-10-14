using highlight.tl;
using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public class SkillData
    {
        public int id;
        public string url;
        public string name;
        public string desc;
        public int length;
        public CDData cd;
    }
    public class Skill : Object
    {
        public SkillData data;
        public int id { get { return data.id; } }
        public Role ower;
        public Timeline timeline;
        public TimelineStyle style { get { return timeline != null ? timeline.lStyle : null; } }
        public bool IsStop { get { return timeline != null ? timeline.IsStopped : false; } }
        //public bool DeadDestroy = true;
        public void UpdateFrame(int delta)
        {
            if (timeline != null)
                timeline.UpdateFrame(delta);
        }
        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (timeline != null)
                timeline.OnDrawGizmos();
#endif
        }
        private readonly static ObjectPool<Skill> pool = new ObjectPool<Skill>();
        public static Skill Get(Skills _ower, SkillData data)
        {
            Skill skill = pool.Get();
            skill.ower = _ower.obj;
            Timeline tl = TimelineFactory.Creat(data.url,_ower.obj);
            skill.timeline = tl;
            tl.skill = skill;
            skill.data = data;
            return skill;
        }
        public static void Release(Skill skill)
        {
            if (skill == null)
                return;
            TimelineFactory.Destroy(skill.timeline);
            skill.timeline = null;
            skill.data = null;
            skill.ower = null;
            pool.Release(skill);
        }
    }
    public class Skills : List<Skill>
    {
        public Role obj;
        public List<SkillData> DataList = new List<SkillData>();
        private Dictionary<int, Skill> dic = new Dictionary<int, Skill>();
        public SkillData GetData(int id)
        {
            return DataList.Find(x=>x.id == id);
        }
        public bool CanPlaySkill(SkillData data)
        {
            if (!data.cd.IsComplete)
                return false;
            RoleState state = this.obj.state;
            if (state != RoleState.Idle && state != RoleState.Move && state != RoleState.Hit)
                return false;
            return !obj.attrs.GetBoolV(AttrType.non_skill);
        }
        public Skill GetRunById(int id)
        {
            Skill sk = null;
            dic.TryGetValue(id, out sk);
            return sk;
        }
        public Skill Creat(int id)
        {
            SkillData data = DataList.Find(x => x.id == id);
            if (data == null)
                return null;
            if (!CanPlaySkill(data))
                return null;
            data.cd.Reset();
            Skill skill = Skill.Get(this, data);
            base.Add(skill);
            dic[skill.id] = skill;
            return skill;
        }
        public void StopSkill(Skill skill)
        {
            base.Remove(skill);
            dic.Remove(skill.id);
            Skill.Release(skill);
        }
        static List<Skill> temp = new List<Skill>();
        public void UpdateFrame(int delta)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].UpdateFrame(delta);
                if (this[i].IsStop)
                    temp.Add(this[i]);
            }
            for (int i = 0; i < temp.Count; i++)
            {
                StopSkill(temp[i]);
            }
            temp.Clear();
        }
        private readonly static ObjectPool<Skills> pool = new ObjectPool<Skills>();
        public static Skills Get(Role _obj)
        {
            Skills skills = pool.Get();
            skills.obj = _obj;
            return skills;
        }
        public void Release()
        {
            for (int i = 0; i < this.Count; i++)
            {
                Skill.Release(this[i]);
            }
            this.Clear();
            dic.Clear();
            obj = null;
            DataList.Clear();
            pool.Release(this);
        }
        public void OnDrawGizmos()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].OnDrawGizmos();
            }
        }
    }
}