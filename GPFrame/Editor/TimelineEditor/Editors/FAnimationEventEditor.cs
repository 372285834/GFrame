using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using System;
using System.Collections.Generic;

using Flux;

namespace GPEditor
{
    [FEditor( typeof( FPlayAnimationEvent ) )]
    public class FAnimationEventEditor : FEventEditor
    {

        [SerializeField]
        protected FPlayAnimationEvent _animEvt;
        [SerializeField]
        protected FAnimationTrack _animTrack;

        protected UnityEditor.Animations.AnimatorState _animState;

        protected UnityEditor.Animations.AnimatorTransition _transitionToState;

        private SerializedObject _animEvtSO;
        private SerializedProperty _blendLength;
        private SerializedProperty _startOffset;

        private SerializedObject _transitionSO;
        private SerializedProperty _transitionDuration;
        private SerializedProperty _transitionOffset;

        private SerializedProperty _transitionConditions;

        private static int _mouseDown = int.MinValue;

        //		private UndoPropertyModification[] UndoProperties( UndoPropertyModification[] modifications )
        //		{
        //			Debug.Log ("UndoProperties");
        //			foreach( UndoPropertyModification modification in modifications )
        //			{
        //				Debug.Log ( "obj ref: " + modification.propertyModification.objectReference + "; path: " + modification.propertyModification.propertyPath + " target:" + modification.propertyModification.target + "; value:" + modification.propertyModification.value );
        //			}
        //			return modifications;
        //		}


        protected override void OnEnable()
        {
            base.OnEnable();

            //SceneView.onSceneGUIDelegate += OnSceneGUI;

            //EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void Init( FObject obj )
        {
            base.Init( obj );

            _animEvt = (FPlayAnimationEvent)_evt;

            _animTrack = (FAnimationTrack)_animEvt.GetTrack();


        }

		private void UpdateEventFromController()
		{
			bool isBlending = _animEvt.IsBlending();

			if( isBlending )
			{
//				FPlayAnimationEvent prevAnimEvt = (FPlayAnimationEvent)_animTrack.GetEvents()[_animEvt.GetId() - 1];

				if( _transitionToState == null )
				{
					_transitionToState = FAnimationTrackInspector.GetTransitionTo( _animEvt );
					
					if( _transitionToState == null )
					{
//						if( animTrackEditor.PreviewInSceneView )
//							animTrackEditor.ClearPreview();
						FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)_trackEditor._track );

						_transitionToState = FAnimationTrackInspector.GetTransitionTo( _animEvt );
					}
				}
				
				if(  _transitionSO == null )
				{
					if( _transitionToState != null )
					{
						_transitionSO = new SerializedObject( _transitionToState );
						//					SerializedProperty p = _transitionSO.GetIterator();
						//
						//					while( p.Next( true ) )
						//						Debug.Log (p.propertyPath );
						_transitionDuration = _transitionSO.FindProperty( "m_TransitionDuration" );
						_transitionOffset = _transitionSO.FindProperty( "m_TransitionOffset" );
						_transitionConditions = _transitionSO.FindProperty( "m_Conditions" );
					}
				}
				else if( _transitionSO.targetObject == null )
				{
					_transitionDuration = null;
					_transitionOffset = null;
					_transitionConditions = null;
					_transitionSO = null;
				}
			}
			else
			{
				if( _transitionToState != null || _transitionSO != null )
				{
					_transitionToState = null;
					_transitionSO = null;
					_transitionOffset = null;
					_transitionDuration = null;
					_transitionConditions = null;
				}
			}

			if( _transitionSO != null )
			{
				_transitionSO.Update();
				_animEvtSO.Update();
				
				FPlayAnimationEvent prevAnimEvt = (FPlayAnimationEvent)_animTrack.GetEvents()[_animEvt.GetId() - 1];

				AnimationClip prevAnimEvtClip = prevAnimEvt._animationClip;
				if( prevAnimEvtClip != null )
				{
					float exitTimeNormalized = (prevAnimEvt.Length / prevAnimEvtClip.frameRate) / prevAnimEvtClip.length;
					
					SerializedProperty exitTime = _transitionConditions.GetArrayElementAtIndex( 0 ).FindPropertyRelative( "m_ExitTime" );
					
					if( !Mathf.Approximately( exitTimeNormalized, exitTime.floatValue ) )
					{
						exitTime.floatValue = exitTimeNormalized;
					}
					
					float blendNormalized = (_blendLength.intValue / prevAnimEvtClip.frameRate) / prevAnimEvtClip.length;
					
					if( !Mathf.Approximately( blendNormalized, _transitionDuration.floatValue ) )
					{
						_blendLength.intValue = Mathf.Clamp( Mathf.RoundToInt( _transitionDuration.floatValue * prevAnimEvtClip.length * prevAnimEvtClip.frameRate ), 0, _animEvt.Length);
						
						_transitionDuration.floatValue = (_blendLength.intValue / prevAnimEvtClip.frameRate) / prevAnimEvtClip.length;

						_animEvtSO.ApplyModifiedProperties();
						
					}
				}

				_transitionSO.ApplyModifiedProperties();
			}
		}

        protected override void RenderEvent( FrameRange viewRange, FrameRange validKeyframeRange )
        {
            if( _animEvtSO == null )
            {
                _animEvtSO = new SerializedObject( _animEvt );
                _blendLength = _animEvtSO.FindProperty( "_blendLength" );
                _startOffset = _animEvtSO.FindProperty( "_startOffset" );
            }


			UpdateEventFromController();

			_animEvtSO.Update();

			FAnimationTrackEditor animTrackEditor = (FAnimationTrackEditor)_trackEditor;

            Rect transitionOffsetRect = _eventRect;

            int startOffsetHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );
            int transitionHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );

            bool isBlending = _animEvt.IsBlending();
			bool isAnimEditable = _animEvt.IsAnimationEditable();

			if( isBlending )
			{
				transitionOffsetRect.xMin = SequenceEditor.GetXForFrame( _animEvt.Start + _animEvt._blendLength ) - 3;
				transitionOffsetRect.width = 6;
				transitionOffsetRect.yMin = transitionOffsetRect.yMax - 8;
			}

	        switch( Event.current.type )
	        {
            case EventType.MouseDown:
                if( EditorGUIUtility.hotControl == 0 && Event.current.alt && !isAnimEditable )
                {
                    if( isBlending && transitionOffsetRect.Contains( Event.current.mousePosition ) )
                    {
                        EditorGUIUtility.hotControl = transitionHandleId;

                        if( Selection.activeObject != _transitionToState )
                            Selection.activeObject = _transitionToState;

                        Event.current.Use();
                    }
                    else if( _eventRect.Contains( Event.current.mousePosition ) )
	                {
	                    _mouseDown = SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - _animEvt.Start;

	                    EditorGUIUtility.hotControl = startOffsetHandleId;

	                    Event.current.Use();
	                }
                }
                break;

            case EventType.MouseUp:
                if( EditorGUIUtility.hotControl == transitionHandleId
                   || EditorGUIUtility.hotControl == startOffsetHandleId )
                {
                    EditorGUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;

            case EventType.MouseDrag:
                if( EditorGUIUtility.hotControl == transitionHandleId )
                {
                    int mouseDragPos = Mathf.Clamp( SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - _animEvt.Start, 0, _animEvt.Length );

                    if( _blendLength.intValue != mouseDragPos )
                    {
                        _blendLength.intValue = mouseDragPos;

						FPlayAnimationEvent prevAnimEvt = (FPlayAnimationEvent)animTrackEditor._track.GetEvent( _animEvt.GetId()-1 );

						if( _transitionDuration != null )
                        	_transitionDuration.floatValue = (_blendLength.intValue / prevAnimEvt._animationClip.frameRate) / prevAnimEvt._animationClip.length;

                        Undo.RecordObject( this, "Animation Blending" );
                    }
                    Event.current.Use();
                }
                else if( EditorGUIUtility.hotControl == startOffsetHandleId )
                {
                    int mouseDragPos = Mathf.Clamp( SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - _animEvt.Start, 0, _animEvt.Length );

                    int delta = _mouseDown - mouseDragPos;

                    _mouseDown = mouseDragPos;

                    _startOffset.intValue = Mathf.Clamp( _startOffset.intValue + delta, 0, _animEvt._animationClip.isLooping ? _animEvt.Length : Mathf.RoundToInt( _animEvt._animationClip.length * _animEvt._animationClip.frameRate ) - _animEvt.Length );

					if( _transitionOffset != null )
                   		_transitionOffset.floatValue = (_startOffset.intValue / _animEvt._animationClip.frameRate) / _animEvt._animationClip.length;

                    Undo.RecordObject( this, "Animation Offset" );
                    
                    Event.current.Use();
                }
                break;
	        }

			_animEvtSO.ApplyModifiedProperties();
			if( _transitionSO != null )
				_transitionSO.ApplyModifiedProperties();


            switch( Event.current.type )
            {
	        case EventType.DragUpdated:
	            if( _eventRect.Contains( Event.current.mousePosition ) )
	            {
	                int numAnimationsDragged = FAnimationEventInspector.NumAnimationsDragAndDrop( _evt.Sequence.FrameRate );
	                DragAndDrop.visualMode = numAnimationsDragged > 0 ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
	                Event.current.Use();
	            }
	            break;
	        case EventType.DragPerform:
	            if( _eventRect.Contains( Event.current.mousePosition ) )
	            {
					AnimationClip animationClipDragged = FAnimationEventInspector.GetAnimationClipDragAndDrop( _evt.Sequence.FrameRate );
	                if( animationClipDragged )
	                {
//						animTrackEditor.ClearPreview();

						int animFrameLength = Mathf.RoundToInt(animationClipDragged.length * animationClipDragged.frameRate);
						
	                    FAnimationEventInspector.SetAnimationClip( _animEvt, animationClipDragged );
						FUtility.Resize( _animEvt, new FrameRange( _animEvt.Start, _animEvt.Start+animFrameLength ) );

	                    DragAndDrop.AcceptDrag();
	                    Event.current.Use();
	                }
	                else
	                {
	                    Event.current.Use();
	                }
	            }
	            break;
            }

            FrameRange currentRange = _evt.FrameRange;

            base.RenderEvent( viewRange, validKeyframeRange );

            if( isAnimEditable && currentRange.Length != _evt.FrameRange.Length )
            {
                FAnimationEventInspector.ScaleAnimationClip( _animEvt._animationClip, _evt.FrameRange );
            }

	        if( Event.current.type == EventType.Repaint )
	        {
	            if( isBlending && !isAnimEditable && viewRange.Contains( _animEvt.Start+_animEvt._blendLength ) )
	            {
	                GUISkin skin = FUtility.GetFluxSkin();

	                GUIStyle transitionOffsetStyle = skin.GetStyle( "BlendOffset" );

					Texture2D t = FUtility.GetFluxTexture( "EventBlend.png" );

					Rect r = new Rect( _eventRect.xMin, _eventRect.yMin+1, transitionOffsetRect.center.x - _eventRect.xMin, _eventRect.height-2 );

					GUI.color = new Color(1f, 1f, 1f, 0.3f);

					GUI.DrawTexture( r, t );

					if( Event.current.alt )
						GUI.color = Color.white;

                	transitionOffsetStyle.Draw( transitionOffsetRect, false, false, false, false );
	            }

//				GUI.color = Color.red;

	            if( EditorGUIUtility.hotControl == transitionHandleId )
	            {
	                Rect transitionOffsetTextRect = transitionOffsetRect;
	                transitionOffsetTextRect.y -= 16;
	                transitionOffsetTextRect.height = 20;
	                transitionOffsetTextRect.width += 50;
	                GUI.Label( transitionOffsetTextRect, _animEvt._blendLength.ToString(), EditorStyles.label );
	            }

	            if( EditorGUIUtility.hotControl == startOffsetHandleId )
	            {
	                Rect startOffsetTextRect = _eventRect;
	                GUI.Label( startOffsetTextRect, _animEvt._startOffset.ToString(), EditorStyles.label );
	            }
	        }
        }

		private TransformCurves _transformCurves = null;

		public void CreateTransformCurves()
		{
			if( _transformCurves == null )
			{
				_transformCurves = new TransformCurves( _animEvt.Owner, _animEvt._animationClip );
			}
		}

		public void RenderTransformCurves( int samplesPerSecond )
		{
			if( _transformCurves == null || _transformCurves.clip == null )
				CreateTransformCurves();
			else
				_transformCurves.RefreshCurves();

			float totalTime = _animEvt.LengthTime;
			float timePerSample = totalTime / samplesPerSecond;

			int numSamples = Mathf.RoundToInt( totalTime / timePerSample );

			Vector3[] pts = new Vector3[numSamples];
			float t = 0;

			for( int i = 0; i < numSamples; ++i )
			{
				pts[i] = _transformCurves.GetWorldPosition( t );
				t += timePerSample;
			}

			Handles.DrawPolyLine( pts );
		}

        private void RebuildAnimationTrack()
        {
            FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)_animEvt.GetTrack() );
        }
    }

	public class TransformCurves
	{
		public Transform bone;
		public AnimationClip clip;

		public AnimationCurve xPos;
		public AnimationCurve yPos;
		public AnimationCurve zPos;
		
		public AnimationCurve xRot;
		public AnimationCurve yRot;
		public AnimationCurve zRot;
		public AnimationCurve wRot;
		
		public AnimationCurve xScale;
		public AnimationCurve yScale;
		public AnimationCurve zScale;
		
		public TransformCurves( Transform transform, AnimationClip clip )
		{
			bone = transform;
			this.clip = clip;
			
			RefreshCurves();
		}

		public void RefreshCurves()
		{
			System.Type transformType = typeof(Transform);
			
			AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves( clip, true );
			for( int i = 0; i != curves.Length; ++i )
			{
				if( curves[i].type != transformType )
					continue;
				
				switch( curves[i].propertyName )
				{
				case "m_LocalPosition.x":
					xPos = curves[i].curve;
					break;
				case "m_LocalPosition.y":
					yPos = curves[i].curve;
					break;
				case "m_LocalPosition.z":
					zPos = curves[i].curve;
					break;
				case "m_LocalRotation.x":
					xRot = curves[i].curve;
					break;
				case "m_LocalRotation.y":
					yRot = curves[i].curve;
					break;
				case "m_LocalRotation.z":
					zRot = curves[i].curve;
					break;
				case "m_LocalRotation.w":
					wRot = curves[i].curve;
					break;
				case "m_LocalScale.x":
					xScale = curves[i].curve;
					break;
				case "m_LocalScale.y":
					yScale = curves[i].curve;
					break;
				case "m_LocalScale.z":
					zScale = curves[i].curve;
					break;
				}
			}
		}

		public Vector3 GetWorldPosition( float t )
		{
			Vector3 localPos = GetPosition( t );

			if( bone.parent == null )
				return localPos;

			return bone.parent.TransformPoint( localPos );
		}
		
		public Vector3 GetPosition( float t )
		{
			Vector3 pos = Vector3.zero;
			
			if( xPos != null )
				pos.x = xPos.Evaluate(t);
			if( yPos != null )
				pos.y = yPos.Evaluate(t);
			if( zPos != null )
				pos.z = zPos.Evaluate(t);
			
			return pos;
		}
		
		public Quaternion GetRotation( float t )
		{
			Quaternion rot = Quaternion.identity;
			
			// since the quaternion may not be normalized, we have to do it ourselves
			//				Vector4 rotVec = new Vector4( xRot.Evaluate(t), yRot.Evaluate(t), zRot.Evaluate(t), wRot.Evaluate(t) );
			//				rotVec.Normalize();
			
			//				Quaternion rot = new Quaternion( rotVec.x, rotVec.y, rotVec.z, rotVec.w );
			
			// probably this doesn't make sense to check? You need to have the 4 channels
			if( xRot != null )
				rot.x = xRot.Evaluate( t );
			if( yRot != null )
				rot.y = yRot.Evaluate( t );
			if( zRot != null )
				rot.z = zRot.Evaluate( t );
			if( wRot != null )
				rot.w = wRot.Evaluate( t );
			
			return rot;
		}
		
		public Vector3 GetScale( float t )
		{
			Vector3 scale = Vector3.one;
			
			if( xScale != null )
				scale.x = xScale.Evaluate(t);
			if( yScale != null )
				scale.y = yScale.Evaluate(t);
			if( zScale != null )
				scale.z = zScale.Evaluate(t);
			
			return scale;
		}
	}
}
