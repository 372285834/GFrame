using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Reflection;
using Flux;

namespace GPEditor
{
	public class FTrackEditor : FEditor
	{
		public const int KEYFRAME_WIDTH = 4;
		public const int KEYFRAME_HALF_WIDTH = KEYFRAME_WIDTH / 2;
		
		public FTrack _track;
		
		public List<FEventEditor> _eventEditors = new List<FEventEditor>();
		
		private GUIStyle _eventStyle;
		private GUIStyle _eventLeftStyle;
		private GUIStyle _eventRightStyle;
		private GUIStyle _eventOutsideStyle;

		public FTimelineEditor _timelineEditor;

		[SerializeField]
		protected AnimVector3 _offsetAnim = new AnimVector3();

		private Texture2D _previewIcon = null;

		protected override void OnEnable ()
		{
			base.OnEnable ();

			_previewIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(FUtility.GetFluxSkinPath()+"View.png", typeof(Texture2D));
		}

		public void Init( FObject obj, FTimelineEditor timelineEditor )
		{
			_timelineEditor = timelineEditor;
			Init( obj );
#if UNITY_4_6
			_offsetAnim.valueChanged.AddListener( SequenceEditor.Repaint );
#endif
		}

		protected override void Init( FObject obj )
		{
			_track = (FTrack)obj;

			_eventEditors.Clear();

			List<FEvent> events = _track.GetEvents();

			for( int i = 0; i < events.Count; ++i )
			{
				FEvent evt = events[i];
                FEventEditor evtEditor = SequenceEditor.GetEditor<FEventEditor>( evt );
				evtEditor.Init( evt, this );
				_eventEditors.Add( evtEditor );
			}
		}

		public virtual void OnStop()
		{
		}

		public override void RefreshRuntimeObject()
		{
			_track = (FTrack)EditorUtility.InstanceIDToObject(_track.GetInstanceID());
		}

		public void SelectEvents( FrameRange range )
		{
			for( int i = 0; i != _eventEditors.Count; ++i )
			{
				if( range.Overlaps( _eventEditors[i]._evt.FrameRange ) )
					SequenceEditor.Select( _eventEditors[i] );
				else if( _eventEditors[i]._evt.Start > range.End )
					break;
			}
		}

		public void DeselectEvents( FrameRange range )
		{
			for( int i = 0; i != _eventEditors.Count; ++i )
			{
				if( range.Overlaps( _eventEditors[i]._evt.FrameRange ) )
					SequenceEditor.Deselect( _eventEditors[i] );
				else if( _eventEditors[i]._evt.Start > range.End )
					break;
			}
		}

		public virtual void Render( int id, Rect rect, int headerWidth, FrameRange viewRange, float pixelsPerFrame )
		{
			rect.y += _offsetAnim.value.y;

			_rect = rect;

			Rect viewRect = rect;
			viewRect.y += _offsetAnim.value.y;
			viewRect.xMax = headerWidth;
			viewRect.xMin = viewRect.xMax-16;
			viewRect.height = 16;

			if( _track.CanTogglePreview() )
			{
				if( Event.current.type == EventType.MouseDown )
				{
					if( viewRect.Contains( Event.current.mousePosition ) )
					{
						if( Event.current.button == 0 ) // left click?
						{
							_track.IsPreviewing = !_track.IsPreviewing;
							FUtility.RepaintGameView();
							Event.current.Use();
						}
					}
				}
			}

			Rect trackHeaderRect = rect;
			trackHeaderRect.xMax = headerWidth;
			
			bool selected = _isSelected;
			
			if( selected )
			{
				Color c = FGUI.GetSelectionColor();
				GUI.color = c;
				GUI.DrawTexture( trackHeaderRect, EditorGUIUtility.whiteTexture );
				GUI.color = FGUI.GetTextColor();

//				Debug.Log( GUI.color );
			}

			Rect trackLabelRect = trackHeaderRect;
			trackLabelRect.xMin += 10;

			GUI.Label( trackLabelRect, new GUIContent(_track.name), FGUI.GetTrackHeaderStyle() );

			rect.xMin = trackHeaderRect.xMax;

			FrameRange validKeyframeRange = new FrameRange(0, SequenceEditor.GetSequence().Length);

			for( int i = 0; i != _eventEditors.Count; ++i )
			{
				if( i == 0 )
					validKeyframeRange.Start = 0;
				else
					validKeyframeRange.Start = _eventEditors[i-1]._evt.End;

				if( i == _eventEditors.Count-1 )
					validKeyframeRange.End = SequenceEditor.GetSequence().Length;
				else
					validKeyframeRange.End = _eventEditors[i+1]._evt.Start;
				_eventEditors[i].Render( rect, viewRange, pixelsPerFrame, validKeyframeRange );
			}

			switch( Event.current.type )
			{
			case EventType.ContextClick:
				if( trackHeaderRect.Contains(Event.current.mousePosition) )
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem( new GUIContent("Duplicate Track"), false, DuplicateTrack );
					menu.AddItem( new GUIContent("Delete Track"), false, DeleteTrack );
					menu.ShowAsContext();
					Event.current.Use();
				}
				break;
			case EventType.MouseDown:
				if( EditorGUIUtility.hotControl == 0 && trackHeaderRect.Contains(Event.current.mousePosition) )
				{
					if( Event.current.button == 0 ) // selecting
					{
						if( Event.current.control )
						{
							if( IsSelected() )
								SequenceEditor.Deselect( this );
							else
								SequenceEditor.Select( this );
						}
						else if( Event.current.shift )
						{
							SequenceEditor.Select( this );
						}
						else
						{
							SequenceEditor.SelectExclusive( this );
							_timelineEditor.StartTrackDrag( this );
							_offsetAnim.value = _offsetAnim.target = new Vector2( 0, rect.yMin ) - Event.current.mousePosition;
							EditorGUIUtility.hotControl = id;
						}
						Event.current.Use();
					}
				}
				break;
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl == id )
				{
					EditorGUIUtility.hotControl = 0;
					_offsetAnim.value = _offsetAnim.target = Vector2.zero;

					_timelineEditor.StopTrackDrag();

					SequenceEditor.Repaint();
					Event.current.Use();
				}
				break;

			case EventType.MouseDrag:
				if( EditorGUIUtility.hotControl == id )
				{
					SequenceEditor.Repaint();
					Event.current.Use();
				}
				break;
			}

			if( _track.CanTogglePreview() )
			{
				GUI.color = FGUI.GetTextColor();
				
				if( !_track.IsPreviewing )
				{
					Color c = GUI.color;
					c.a = 0.3f;
					GUI.color = c;
				}
				
				GUI.DrawTexture( viewRect, _previewIcon );
				
				GUI.color = Color.white;
			}

#if UNITY_4_5
			if( _offsetAnim.isAnimating )
				SequenceEditor.Repaint();
#endif
		}

		void DuplicateTrack()
		{
			Undo.RecordObjects( new UnityEngine.Object[]{ _timelineEditor, _track.GetTimeline() }, string.Empty );
			GameObject duplicateTrack = (GameObject)Instantiate( _track.gameObject );
			duplicateTrack.name = _track.gameObject.name;
			Undo.SetTransformParent( duplicateTrack.transform, _track.GetTimeline().transform, string.Empty );
			Undo.RegisterCreatedObjectUndo( duplicateTrack, "duplicate Track" );

			if( !SequenceEditor.GetSequence().IsStopped )
				duplicateTrack.GetComponent<FTrack>().Init();
		}

		void DeleteTrack()
		{
			if( _track.IsPreviewing )
				_track.IsPreviewing = false;
			UnityEngine.Object[] objsToSave = new UnityEngine.Object[]{ _track.GetTimeline(), _track };
			Undo.RecordObjects( objsToSave, string.Empty );
			Undo.SetTransformParent( _track.transform, null, string.Empty );
			_track.GetTimeline().RemoveTrack( _track );
			Undo.DestroyObjectImmediate( _track.gameObject );
		}

		public void SetOffset( Vector2 offset, bool instant )
		{
			_offsetAnim.target = offset;
			if( instant )
				_offsetAnim.value = offset;
		}
		
		public void SetOffset( Vector2 offset )
		{
			SetOffset( offset, false );
		}

		public Vector3 GetOffset()
		{
			return _offsetAnim.value;
		}

		public override FObject GetRuntimeObject()
		{
			return _track;
		}

		public override FSequenceEditor SequenceEditor { get { return _timelineEditor.SequenceEditor; } }

		private Type[] _fcEventTypes = null;
		
		public void ShowTrackMenu( FTrack track )
		{
			if( _fcEventTypes == null )
			{
				_fcEventTypes = new Type[0];
				Type[] allTypes = typeof(FEvent).Assembly.GetTypes();
				
				foreach( Type type in allTypes )
				{
					if( type.IsSubclassOf( typeof(FEvent) ) && !type.IsAbstract )
					{
						object[] attributes = type.GetCustomAttributes( typeof(FEventAttribute), false );
						if( attributes.Length == 1 )
						{
							ArrayUtility.Add<Type>( ref _fcEventTypes, type );
						}
					}
				}
			}
			
			GenericMenu menu = new GenericMenu();
			foreach( Type t in _fcEventTypes )
			{
				TimelineMenuData param = new TimelineMenuData();
				param.track = track; param.evtType = t;
				object[] attributes = t.GetCustomAttributes(typeof(FEventAttribute), false);
				menu.AddItem( new GUIContent( ((FEventAttribute)attributes[0]).menu ), false, AddEventToTrack, param );
			}
			menu.ShowAsContext();
		}

		private struct TimelineMenuData
		{
			public FTrack track;
			public Type evtType;
		}
		
		private void AddEventToTrack( object obj )
		{
			TimelineMenuData menuData = (TimelineMenuData)obj;
			GameObject go = new GameObject(menuData.evtType.ToString());
			FEvent evt = (FEvent)go.AddComponent(menuData.evtType);
			menuData.track.Add( evt );
			
			SequenceEditor.Refresh();
		}

		public FrameRange GetValidRange( FEventEditor evtEditor )
		{
			int index = 0;
			for( ; index < _eventEditors.Count; ++index )
			{
				if( _eventEditors[index] == evtEditor )
				{
					break;
				}
			}

			FrameRange range = new FrameRange( 0, SequenceEditor.GetSequence().Length );

			if( index > 0 )
			{
				range.Start = _eventEditors[index-1]._evt.End+1;
			}
			if( index < _eventEditors.Count-1 )
			{
				range.End = _eventEditors[index+1]._evt.Start-1;
			}

			return range;
		}

		public FEvent TryAddEvent( int t )
		{
			FEvent newEvt = null;
			if( _track.CanAddAt( t ) )
			{
				FEvent evtAfterT = _track.GetEventAfter( t );
				int newEventEndT;
				if( evtAfterT == null )
					newEventEndT = SequenceEditor.GetSequence().Length;
				else
					newEventEndT = evtAfterT.Start;

				newEvt = FEvent.Create( _track.GetEventType(), new FrameRange( t, newEventEndT ) );

				Undo.RecordObject( _track, string.Empty );
				Undo.RegisterCreatedObjectUndo( newEvt.gameObject, "create Event" );

				_track.Add( newEvt );
			}
			return newEvt;
		}

		public virtual void OnTrackChanged()
		{
		}

		public virtual void UpdateEventsEditor( int frame, float time )
		{
			_track.UpdateEventsEditor( frame, time );
		}
	}
}