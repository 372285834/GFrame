using GP;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GPEditor
{
    public class GEventWindow : EditorWindow
    {
        private const string FRAMERANGE_START_FIELD_ID = "FrameRange.Start";
        public static GEventWindow Inst = null;
        [MenuItem("WindowTools/GEvent", false, 2)]
        public static void Open()
        {
            if (Inst != null)
                return;
            Inst = EditorWindow.GetWindow<GEventWindow>("GEvent");
            Inst.minSize = new Vector2(300f, 200f);
            Inst.Show();
        }
        static GEvent curEvt;
        static GPEditor.SkillWindow.TreeNode rootNode;
        public static void Open(SkillWindow.TreeNode _node,GEvent e)
        {
            Open();
            rootNode = _node.root;
            curEvt = e;
            Inst.Repaint();
        }
        void drawRoot(GTimelineStyle root)
        {
            string rName = "保存-" + root.name;
            if (rootNode.isChange)
                rName += "*";
            if (GUILayout.Button(rName,GUILayout.MinHeight(30f)))
            {
                SkillWindow.Save(root);
            }
            EditorGUILayout.BeginHorizontal();
            root.name = EditorGUILayout.TextField(root.name);
            root.UpdateMode = (AnimatorUpdateMode)EditorGUILayout.EnumPopup(root.UpdateMode);
            GUILayout.Label("帧率：", EditorStyles.label);
            root.FrameRate = EditorGUILayout.IntField(root.FrameRate);
            GUILayout.Label("总帧数:", EditorStyles.label);
            root.End = EditorGUILayout.IntField(root.End);
            EditorGUILayout.EndHorizontal();
        }
        Vector2 mScrollPos = new Vector2(0, 0);
        void OnGUI()
        {
            if (curEvt == null)
                return;
            //if (SkillWindow.Inst == null)
            //{
            //    this.Close();
            //    return;
            //}
            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
            EditorGUILayout.BeginVertical();
            drawRoot(curEvt.root.lStyle);
            GEventStyle style = curEvt.mStyle;
            GUILayout.Label(style.Attr.name + "    index:" + curEvt.id);
            FrameRange rang = style.range;
            FrameRange validRange = curEvt.GetMaxFrameRange();
            if (!(style is GTimelineStyle))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Range    ");
                GUILayout.Label("S:", EditorStyles.label);
                GUI.SetNextControlName(FRAMERANGE_START_FIELD_ID);
                rang.Start = EditorGUILayout.IntField(style.Start);
                GUILayout.Label("E:", EditorStyles.label);
                rang.End = EditorGUILayout.IntField(style.End);
                EditorGUILayout.EndHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);//EditorGUIUtility.labelWidth
                GUILayout.Label(validRange.Start.ToString(), GUILayout.Width(30f));
                float sliderStartFrame = rang.Start;
                float sliderEndFrame = rang.End;
                EditorGUILayout.MinMaxSlider(ref sliderStartFrame, ref sliderEndFrame, validRange.Start, validRange.End);
                GUILayout.Label(validRange.End.ToString(), GUILayout.Width(30f));
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    rang.Start = (int)sliderStartFrame;
                    rang.End = (int)sliderEndFrame;
                }
                rang = FrameRange.Resize(rang, validRange);
            }
            else
            {
            }
            if (rang != style.range)
                rootNode.isChange = true;
            style.range = rang;
            RanderEvent(style);
            curEvt.OnStyleChange();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void RanderEvent(GEventStyle style)
        {
            if(style is GTargetStyle)
            {
                drawGTargetStyle(style as GTargetStyle);
            }
            if (style is GActionStyle)
            {

            }
            if(style is GEffectStyle)
            {
                if (style is GEffectSequenceStyle)
                {

                }
            }
            if(style is GMissileStyle)
            {
                if (style is GMissileSequenceStyle)
                {

                }
            }
        }
        Locator drawLocator(Locator l,string name)
        {
           // GUILayout.Space(10f);
            DrawSeparator(142f);
            GUILayout.Label(name + "----------------------");
            l.eName = (Locator.eNameType)EditorGUILayout.EnumPopup("挂点名:", l.eName);
            l.type = (Locator.eType)EditorGUILayout.EnumPopup("类型：",l.type);
            l.isFollow = EditorGUILayout.Toggle("跟随：", l.isFollow);
            l.position = EditorGUILayout.Vector3Field("坐标：",l.position);
            l.euler = EditorGUILayout.Vector3Field("旋转：",l.euler);
            GUILayout.Space(3f);
            return l;
        }
        void drawGTargetStyle(GTargetStyle s)
        {
            s.res = EditorGUILayout.TextField("资源名：",s.res);
            s.startLocator = drawLocator(s.startLocator, "开始挂点");
            s.endLocator = drawLocator(s.endLocator, "结束挂点");
        }


        static public void DrawSeparator(float h)
        {
            GUILayout.Space(3f);

            if (UnityEngine.Event.current.type == UnityEngine.EventType.Repaint)
            {
                Texture2D tex = EditorGUIUtility.whiteTexture;
                Rect rect = GUILayoutUtility.GetLastRect();
                Color co = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.15f);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 4f, Screen.width, 2f), tex);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 4f, Screen.width, h), tex);
                GUI.DrawTexture(new Rect(0f, rect.yMin + h + 2f, Screen.width, 2f), tex);
                GUI.color = co;
            }
        }
    }
}