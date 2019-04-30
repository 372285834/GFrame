using highlight.tl;
using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public class Skill : Object
    {
        public int id;
        public Role ower;
        public Timeline timeline;
        public TimelineStyle style { get { return timeline != null ? timeline.lStyle : null; } }
        public bool IsStop { get { return timeline != null ? timeline.IsStopped : false; } }
        public void UpdateFrame(int delta)
        {
            if (timeline != null)
                timeline.UpdateFrame(delta);
        }
        private readonly static ObjectPool<Skill> pool = new ObjectPool<Skill>();
        public static Skill Get(Skills _ower, TimelineStyle _style)
        {
            Skill skill = pool.Get();
            skill.ower = _ower.obj;
            if (_style != null)
            {
                Timeline tl = TimelineFactory.Creat(_style);
                skill.timeline = tl;
                tl.owner = _ower.obj;
                tl.skill = skill;
            }
            return skill;
        }
        public static void Release(Skill skill)
        {
            if (skill == null)
                return;
            if (skill.timeline != null)
            {
                skill.timeline.Destroy();
                skill.timeline = null;
            }
            skill.ower = null;
            pool.Release(skill);
        }
    }
    public class Skills : List<Skill>
    {
        public Role obj;
        private Dictionary<int, Skill> dic = new Dictionary<int, Skill>();
        public Skill GetById(int id)
        {
            Skill sk = null;
            dic.TryGetValue(id, out sk);
            return sk;
        }
        public void AddSkill(TimelineStyle style)
        {
            Skill skill = Skill.Get(this, style);
            base.Add(skill);
            dic[skill.id] = skill;
        }
        public void RemoveSkill(Skill skill)
        {
            base.Remove(skill);
            Skill.Release(skill);
            dic.Remove(skill.id);
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
                RemoveSkill(temp[i]);
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
            pool.Release(this);
        }
    }
}