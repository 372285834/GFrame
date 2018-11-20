using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using GP;
using GPEditor;

namespace GPEditor
{
	[CustomEditor(typeof(FPlayAnimationEvent))]
	public class FAnimationEventInspector : FEventInspector
	{
		private FPlayAnimationEvent _animEvent;

		private SerializedProperty _animationClip;

		private SerializedProperty _frameRange;

		private SerializedProperty _startOffset;

		protected override void OnEnable()
		{
			base.OnEnable();

			if( target == null )
			{
				DestroyImmediate( this );
				return;

			}
			_animEvent = (FPlayAnimationEvent)target;
			_animationClip = serializedObject.FindProperty("_animationClip");

			_startOffset = serializedObject.FindProperty("_startOffset");

	        _frameRange = serializedObject.FindProperty("_frameRange");
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			if( _animationClip.objectReferenceValue == null )
			{
				EditorGUILayout.HelpBox("There's no Animation Clip", MessageType.Warning);
				Rect helpBoxRect = GUILayoutUtility.GetLastRect();
				
				float yCenter = helpBoxRect.center.y;
				
				helpBoxRect.xMax -= 3;
				helpBoxRect.xMin = helpBoxRect.xMax - 50;
				helpBoxRect.yMin = yCenter - 12.5f;
				helpBoxRect.height = 25f;
				
				if( GUI.Button( helpBoxRect, "Create" ) )
				{
					CreateAnimationClip( _animEvent );
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( _animationClip );

			Rect animationClipRect = GUILayoutUtility.GetLastRect();

			if( Event.current.type == EventType.DragUpdated && animationClipRect.Contains( Event.current.mousePosition ) )
			{
				int numAnimations = NumAnimationsDragAndDrop( _animEvent.Sequence.FrameRate );
				if( numAnimations > 0 )
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			}
			else if( Event.current.type == EventType.DragPerform && animationClipRect.Contains( Event.current.mousePosition ) )
			{
				if( NumAnimationsDragAndDrop( _animEvent.Sequence.FrameRate ) > 0 )
				{
					DragAndDrop.AcceptDrag();
					AnimationClip clip = GetAnimationClipDragAndDrop( _animEvent.Sequence.FrameRate );
					if( clip != null )
						_animationClip.objectReferenceValue = clip;
				}
			}

			if( EditorGUI.EndChangeCheck() )
			{
				AnimationClip clip = (AnimationClip)_animationClip.objectReferenceValue;
				if( clip )
				{
					if( clip.frameRate != _animEvent.GetTrack().GetTimeline().Sequence.FrameRate )
					{
						Debug.LogError(string.Format("Can't add animation, it has a different frame rate from the sequence ({0} vs {1})", 
						                             clip.frameRate, _animEvent.GetTrack().GetTimeline().Sequence.FrameRate ) );

						_animationClip.objectReferenceValue = null;
					}
					else
					{
						SerializedProperty frameRangeEnd = _frameRange.FindPropertyRelative( "_end" );
	                    FrameRange maxFrameRange = _animEvent.GetMaxFrameRange();
						frameRangeEnd.intValue = Mathf.Min( _animEvent.FrameRange.Start + Mathf.RoundToInt( clip.length * clip.frameRate ), maxFrameRange.End );
					}
				}
			}

			if( !_animEvent.IsAnimationEditable() )
				EditorGUILayout.IntSlider( _startOffset, 0, _animEvent.GetMaxStartOffset() );

			serializedObject.ApplyModifiedProperties();
		}

		public static int NumAnimationsDragAndDrop()
		{
			return NumAnimationsDragAndDrop( -1 );
		}

		public static int NumAnimationsDragAndDrop( int frameRate )
		{
			int numAnimations = 0;
			
			if( DragAndDrop.objectReferences.Length == 0 )
				return numAnimations;
			
			string pathToAsset = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0].GetInstanceID());
			
			if( string.IsNullOrEmpty(pathToAsset) )
				return numAnimations;
			
			if( DragAndDrop.objectReferences[0] is AnimationClip && (frameRate <= 0 || Mathf.Approximately( ((AnimationClip)DragAndDrop.objectReferences[0]).frameRate, frameRate)) )
				++numAnimations;
			
			Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath( pathToAsset );
			
			foreach( Object obj in objs )
			{
				if( obj is AnimationClip && (frameRate <= 0 || Mathf.Approximately(((AnimationClip)obj).frameRate, frameRate)) )
					++numAnimations;
			}
			
			return numAnimations;
		}

		private static AnimationClip _dragAndDropSelectedAnimation = null;

		public static AnimationClip GetAnimationClipDragAndDrop( int frameRate )
		{
			_dragAndDropSelectedAnimation = null;

			if( DragAndDrop.objectReferences.Length == 0 )
				return null;

			string pathToAsset = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0].GetInstanceID());
			
			if( string.IsNullOrEmpty(pathToAsset) )
				return null;

			List<AnimationClip> animationClips = new List<AnimationClip>();

	        if( DragAndDrop.objectReferences[0] is AnimationClip && (frameRate <= 0 || Mathf.Approximately( ((AnimationClip)DragAndDrop.objectReferences[0]).frameRate, frameRate)) )
	            animationClips.Add( (AnimationClip)DragAndDrop.objectReferences[0] );
			else
			{
				Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath( pathToAsset );
				
				foreach( Object obj in objs )
				{
					if( obj is AnimationClip && (frameRate <= 0 || Mathf.Approximately( ((AnimationClip)obj).frameRate, frameRate)) )
		            {
		                animationClips.Add( (AnimationClip)obj );
		            }
				}
			}

			if( animationClips.Count == 0 )
				return null;
			else if( animationClips.Count == 1 )
				return animationClips[0];

			GenericMenu menu = new GenericMenu();
			for( int i = 0; i != animationClips.Count; ++i )
			{
				menu.AddItem( new GUIContent(animationClips[i].name), false, SetAnimationClip, animationClips[i] );
			}

			menu.ShowAsContext();
			
			return _dragAndDropSelectedAnimation;
		}

		private static void SetAnimationClip( object data )
		{
			if( data == null )
				_dragAndDropSelectedAnimation = null;
			else
				_dragAndDropSelectedAnimation = (AnimationClip)data;
		}



		private void SetAnimationClip( AnimationClip animClip )
		{
			_animationClip.objectReferenceValue = animClip;
	        serializedObject.ApplyModifiedProperties();
		}

		public static void SetAnimationClip( FPlayAnimationEvent animEvent, AnimationClip animClip )
		{
			FAnimationEventInspector editor = (FAnimationEventInspector)CreateEditor( animEvent, typeof( FAnimationEventInspector ) );
			editor.SetAnimationClip( animClip );

			DestroyImmediate( editor );
	        FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)animEvent.GetTrack() );
		}

		// animation editing
		public static void ScaleAnimationClip( AnimationClip clip, FrameRange range )
		{
	        if( clip == null )
	            return;
			float oldLength = clip.length;
			float newLength = range.Length / clip.frameRate;

			AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves( clip, true );

			// when scaling, we need to adjust the tangents otherwise the curves will get completely distorted
			float tangentMultiplier = oldLength / newLength;

			for( int i = 0; i != curves.Length; ++i )
			{
				AnimationCurve curve = curves[i].curve;

				if( newLength < oldLength )
				{

					for( int j = 0; j < curve.keys.Length; ++j )
					{
						Keyframe newKeyframe = new Keyframe( (curve.keys[j].time / oldLength) * newLength, curve.keys[j].value, curve.keys[j].inTangent*tangentMultiplier, curve.keys[j].outTangent*tangentMultiplier );
						newKeyframe.tangentMode = curve.keys[j].tangentMode;
						curve.MoveKey( j, newKeyframe );
					}
				}
				else
				{
					for( int j = curve.keys.Length-1; j >= 0; --j )
					{
						Keyframe newKeyframe = new Keyframe( (curve.keys[j].time / oldLength) * newLength, curve.keys[j].value, curve.keys[j].inTangent*tangentMultiplier, curve.keys[j].outTangent*tangentMultiplier );
						newKeyframe.tangentMode = curve.keys[j].tangentMode;
						curve.MoveKey( j, newKeyframe );
					}
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( curves[i].path, curves[i].type, curves[i].propertyName ), curve );
			}

			clip.EnsureQuaternionContinuity();
	        EditorApplication.RepaintAnimationWindow();
			EditorUtility.SetDirty( clip );
		}

		public static AnimationClip CreateAnimationClip( FPlayAnimationEvent animEvent )
		{
			string filePath = EditorUtility.SaveFilePanelInProject( "Create Animation...", animEvent.Owner.name, "anim", "Choose path..." );

			if( string.IsNullOrEmpty(filePath) )
				return null;

			AnimationClip clip = UnityEditor.Animations.AnimatorController.AllocateAnimatorClip( System.IO.Path.GetFileNameWithoutExtension(filePath) );
			clip.frameRate = animEvent.Sequence.FrameRate;

			Transform ownerTransform = animEvent.Owner;

			Vector3 pos = ownerTransform.localPosition;
			Quaternion rot = ownerTransform.localRotation;

			Keyframe[] xPosKeys = new Keyframe[]{ new Keyframe(0, pos.x), new Keyframe(animEvent.LengthTime, pos.x) };
			Keyframe[] yPosKeys = new Keyframe[]{ new Keyframe(0, pos.y), new Keyframe(animEvent.LengthTime, pos.y) };
			Keyframe[] zPosKeys = new Keyframe[]{ new Keyframe(0, pos.z), new Keyframe(animEvent.LengthTime, pos.z) };

			Keyframe[] xRotKeys = new Keyframe[]{ new Keyframe(0, rot.x), new Keyframe(animEvent.LengthTime, rot.x) };
			Keyframe[] yRotKeys = new Keyframe[]{ new Keyframe(0, rot.y), new Keyframe(animEvent.LengthTime, rot.y) };
			Keyframe[] zRotKeys = new Keyframe[]{ new Keyframe(0, rot.z), new Keyframe(animEvent.LengthTime, rot.z) };
			Keyframe[] wRotKeys = new Keyframe[]{ new Keyframe(0, rot.w), new Keyframe(animEvent.LengthTime, rot.w) };

			// set the tangent mode
			int tangentMode = 10; // 10 is unity auto tangent mode

			for( int i = 0; i != xPosKeys.Length; ++i )
			{
				xPosKeys[i].tangentMode = tangentMode;
				yPosKeys[i].tangentMode = tangentMode;
				zPosKeys[i].tangentMode = tangentMode;

				xRotKeys[i].tangentMode = tangentMode;
				yRotKeys[i].tangentMode = tangentMode;
				zRotKeys[i].tangentMode = tangentMode;
				wRotKeys[i].tangentMode = tangentMode;
			}

			AnimationCurve xPos = new AnimationCurve( xPosKeys );
			AnimationCurve yPos = new AnimationCurve( yPosKeys );
			AnimationCurve zPos = new AnimationCurve( zPosKeys );

			AnimationCurve xRot = new AnimationCurve( xRotKeys );
			AnimationCurve yRot = new AnimationCurve( yRotKeys );
			AnimationCurve zRot = new AnimationCurve( zRotKeys );
			AnimationCurve wRot = new AnimationCurve( wRotKeys );

			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalPosition.x"), xPos );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalPosition.y"), yPos );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalPosition.z"), zPos );

			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalRotation.x"), xRot );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalRotation.y"), yRot );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalRotation.z"), zRot );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.PPtrCurve( string.Empty, typeof(Transform), "m_LocalRotation.w"), wRot );

			clip.EnsureQuaternionContinuity();

			AssetDatabase.CreateAsset( clip, filePath );

			FAnimationEventInspector.SetAnimationClip( animEvent, clip );

			return clip;
		}
	}
}