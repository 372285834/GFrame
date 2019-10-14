using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/执行事件指令", typeof(CmdAction))]
    public class CmdAction : TimeAction
    {
        public override TriggerStatus OnTrigger()
        {
            this.owner.mainStateMachine.AddObserver(this.StateChange);
            return base.OnTrigger();
        }
        void StateChange(StateMachineAction machine)
        {
            if(this.owner.state != RoleState.Move)
            {
                RVO.Simulator.Instance.setAgentPrefVelocity(owner.onlyId, RVO.Vector2.Zero);
                RVO.Simulator.Instance.setAgentMaxSpeed(owner.onlyId, 0f);
            }
        }
        public override void OnUpdate()
        {
            // ProfilerTest.BeginSample("PlayEvent");
            //if (!Events.Contains(this.onlyId))
            //{
            //    if (this.control != null && this.control.curClip == "run")
            //        this.PlayClip("wait_1", true);
            //}
            //else
            //{
            Role role = this.owner;
            if (!Events.Contains(this.owner.onlyId))
            {
                if (role.state == RoleState.Move)
                {
                    role.Switch(RoleState.Idle);
                }
                return;
            }
            if (role.attrs.GetBoolV(AttrType.non_control))
                return;
            RoleEvent evts = Events.Get(role);
            if (!role.non_move)
            {
                //if (evts.isMove)
                //{
                //    role.Switch(RoleState.Move);
                //    role.SetPos(evts.pos, false);
                //}
                if (evts.isDir)
                {
                    role.Switch(RoleState.Move);
                    //role.SetForward(evts.dir);
                    Vector3 dir = evts.dir;
                    if (role.non_obstacle)
                    {
                        role.SetPos(role.position + dir, false);
                        role.SetForward(dir, false);
                        RVO.Simulator.Instance.setAgentPosition(role.onlyId, new RVO.Vector2(role.position.x, role.position.z));
                        RVO.Simulator.Instance.setAgentMaxSpeed(owner.onlyId, 0f);
                    }
                    else
                    {
                        RVO.Vector2 vel = new RVO.Vector2(dir.x, dir.z);
                        RVO.Simulator.Instance.setAgentPrefVelocity(owner.onlyId, vel);
                        RVO.Simulator.Instance.setAgentMaxSpeed(owner.onlyId, owner.move_speed);
                    }
                }
            }
            else
            {
               // RVO.Simulator.Instance.setAgentMaxSpeed(owner.onlyId, 0f);
            }
            if (evts.skillId > 0)
            {
                //   Debug.Log("CreatSkill:" + evts.skillId);
                Skill skill = role.skills.Creat(evts.skillId);
                if (skill == null)
                    return;
                if (evts.isSkillPos)
                    skill.timeline.target.setPosition(evts.skillPos);
                skill.timeline.Play(0);
            }
            //}
            //  ProfilerTest.EndSample();
        }

        public override void OnFinish()
        {
            this.owner.mainStateMachine.RemoveObserver(this.StateChange);
            base.OnFinish();
        }
    }
}