using highlight.tl;
using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public class Skill : Object
    {
        public Skills ower;
        public Timeline timeline;
        public SceneObject obj { get { return ower.obj; } }
        public TimelineStyle style { get { return timeline != null ? timeline.lStyle : null; } }
        public bool IsStop { get { return timeline != null ? timeline.IsStopped : false; } }
        public void UpdateFrame(int frame)
        {
            if (timeline != null)
                timeline.UpdateFrame(frame);
        }
        private readonly static ObjectPool<Skill> pool = new ObjectPool<Skill>();
        public static Skill Get(Skills _ower, TimelineStyle _style)
        {
            Skill skill = pool.Get();
            skill.ower = _ower;
            if (_style != null)
            {
                skill.timeline = TimelineFactory.Creat(_style);
                skill.timeline.skill = skill;
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
        public SceneObject obj;
        public void AddSkill(TimelineStyle style)
        {
            Skill skill = Skill.Get(this, style);
            base.Add(skill);
        }
        public void RemoveSkill(Skill skill)
        {
            base.Remove(skill);
            Skill.Release(skill);
        }
        static List<Skill> temp = new List<Skill>();
        public void UpdateFrame(int frame)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].UpdateFrame(frame);
                if (this[i].IsStop)
                    temp.Add(this[i]);
            }
            for (int i = 0; i < temp.Count; i++)
            {
                Remove(temp[i]);
            }
            temp.Clear();
        }
        private readonly static ObjectPool<Skills> pool = new ObjectPool<Skills>();
        public static Skills Get(SceneObject _obj)
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
            base.Clear();
            obj = null;
            pool.Release(this);
        }
    }
}