using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;

using System;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public class FTimelineEditor : FEditor
	{
		public const int HEADER_HEIGHT = 25;
		public const int TRACK_HEIGHT = 25;

		protected FSequenceEditor _sequenceEditor;

		public FTimeline _timeline;

		public bool _showTracks = true;

		public List<FTrackEditor> _trackEditors = new List<FTrackEditor>();

		[SerializeField]
		private int[] _trackEditorIds = new int[0];

		[SerializeField]
		private AnimVector3 _offsetAnim = new AnimVector3();

		private FTrackEditor _trackDragged = null;

		public void Init( FObject obj, FSequenceEditor sequenceEditor )
		{
            //if (_sequenceEditor == sequenceEditor && obj == _timeline)
            //{
            //    Debug.LogError("´íÎóµÄÐòÁÐ£º" + sequenceEditor.name);
            //    return;
            //}
			_sequenceEditor = sequenceEditor;
			Init( obj );
#if UNITY_4_6
			_offsetAnim.valueChanged.AddListener(  _sequenceEditor.Repaint );
#endif
		}

		protected override void Init( FObject obj )
		{
			_timeline = (FTimeline)obj;

			_trackEditors.Clear();

			List<FTrack> tracks = _timeline.GetTracks();

			for( int i = 0; i < tracks.Count; ++i )
			{
				FTrack track = tracks[i];
				FTrackEditor trackEditor = _sequenceEditor.GetEditor<FTrackEditor>(track);
				trackEditor.Init( track, this );
				_trackEditors.Add( trackEditor );
			}
		}

		public void OnStop()
		{
			for( int i = 0; i != _trackEditors.Count; ++i )
				_trackEditors[i].OnStop();
		}

		public override void RefreshRuntimeObject()
		{
			_timeline = (FTimeline)EditorUtility.InstanceIDToObject(_timeline.GetInstanceID());
		}

		public int GetHeight()
		{
			return _showTracks ? _trackEditors.Count * TRACK_HEIGHT + HEADER_HEIGHT : HEADER_HEIGHT;
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

		public void Render( int id, Rect rect, int hierarchyWidth, FrameRange viewRange, float pixelsPerFrame )
		{
			if( _timeline == null )
			{
				return;
			}

			rect.y += _offsetAnim.value.y;

			_rect = rect;

			float alpha = 1;

			if( EditorGUIUtility.hotControl == id )
			{
				rect.xMin += 5;
				rect.xMax -= 5;
				alpha = 0.7f;
				Color c = GUI.color; c.a = alpha;
				GUI.color = c;
			}

			Rect hierarchyHeaderRect = rect; hierarchyHeaderRect.width = hierarchyWidth; hierarchyHeaderRect.height = HEADER_HEIGHT; 
		
			Rect timelineHeaderRect = rect; timelineHeaderRect.height = HEADER_HEIGHT;

			Rect trackRect = timelineHeaderRect;
			trackRect.yMin = timelineHeaderRect.yMax;
			trackRect.height = TRACK_HEIGHT;

			if( Event.current.type == EventType.Repaint )
			{
				GUI.color = FGUI.GetTimelineColor();
				GUI.DrawTexture( timelineHeaderRect, EditorGUIUtility.whiteTexture );
				GUI.color = new Color(1f, 1f, 1f, alpha);
			}

			if( _showTracks )
			{
				for( int i = 0; i != _trackEditors.Count; ++i )
				{
					Vector3 upperLeft = trackRect.min;
					Handles.color = FGUI.GetLineColor();

					if( _trackDragged != null )
					{
						if( _trackDragged == _trackEditors[i]  )
						{
							Handles.DrawLine( upperLeft, upperLeft + new Vector3(trackRect.width, 0, 0 ) );
							trackRect.y += TRACK_HEIGHT;
							continue;
						}

						if( i < _trackDragged.GetRuntimeObject().GetId() && Event.current.mousePosition.y < trackRect.yMax )
							_trackEditors[i].SetOffset( new Vector2(0, TRACK_HEIGHT) );
						else if( i > _trackDragged.GetRuntimeObject().GetId() && Event.current.mousePosition.y > trackRect.yMin )
							_trackEditors[i].SetOffset( new Vector2(0, -TRACK_HEIGHT) );
						else
							_trackEditors[i].SetOffset( Vector2.zero );
					}

					GUI.color = new Color(0.3f, 0.3f, 0.3f, alpha);

					GUI.color = new Color(1f, 1f, 1f, alpha);
					_trackEditors[i].Render( _trackEditorIds[i], trackRect, hierarchyWidth, viewRange, pixelsPerFrame );

					Handles.DrawLine( upperLeft, upperLeft + new Vector3(trackRect.width, 0, 0 ) );

					trackRect.y += TRACK_HEIGHT;
				}

				if( _trackDragged != null )
				{
					Rect r = trackRect;
					r.y = Event.current.mousePosition.y;
					_trackDragged.Render( _trackEditorIds[_trackDragged.GetRuntimeObject().GetId()], r, hierarchyWidth, viewRange, pixelsPerFrame );
				}


			}

			Rect hierarchyLabelRect = hierarchyHeaderRect;
			hierarchyLabelRect.height = 20;
			hierarchyLabelRect.xMax = hierarchyLabelRect.xMax-23;

			Rect foldoutRect = hierarchyLabelRect;
			foldoutRect.width = 16;
			hierarchyLabelRect.xMin += 16;

			string timelineHeaderName = _timeline.Owner != null ? _timeline.Owner.name : _timeline.name + " (Missing)";

			GUI.Label( hierarchyLabelRect, new GUIContent(timelineHeaderName), FGUI.GetTimelineHeaderStyle() );

			_showTracks = EditorGUI.Foldout( foldoutRect, _showTracks, GUIContent.none );

			switch( Event.current.type )
			{
			case EventType.ContextClick:
				if( hierarchyHeaderRect.Contains( Event.current.mousePosition ) )
				{
					GenericMenu menu = new GenericMenu();

					if( Selection.activeGameObject == null || PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab || PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.ModelPrefab )
						menu.AddDisabledItem( new GUIContent("Change Owner") );
					else
						menu.AddItem( new GUIContent("Change Owner to " + Selection.activeGameObject.name ), false, ChangeOwner );

					menu.AddItem( new GUIContent("Duplicate Timeline"), false, DuplicateTimeline );
                    menu.AddItem(new GUIContent("Delete Timeline"), false, DeleteTimeline);
                    menu.AddItem(new GUIContent("Add Timeline"), false, AddTimeline);
					
					menu.ShowAsContext();
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
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl == id )
				{
					EditorGUIUtility.hotControl = 0;
					_offsetAnim.value = _offsetAnim.target = Vector2.zero;
					SequenceEditor.Repaint();
					
					SequenceEditor.StopTimelineDrag();
					Event.current.Use();
				}
				break;
				
			case EventType.Repaint:
				Handles.color = FGUI.GetLineColor();
				Handles.DrawLine( new Vector3(rect.xMin, rect.yMax, 0), new Vector3( rect.xMax, rect.yMax, 0) );
				break;
				
			case EventType.KeyDown:
				if( EditorGUIUtility.hotControl == id && Event.current.keyCode == KeyCode.Escape )
				{
					EditorGUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			}

			Rect timelineOptionsRect = hierarchyHeaderRect;
			timelineOptionsRect.xMin = hierarchyHeaderRect.xMax - 20;
			timelineOptionsRect.yMin = hierarchyHeaderRect.yMin;
			timelineOptionsRect.width = 14;
			timelineOptionsRect.height = 14;

			if( Event.current.type == EventType.MouseDown && timelineOptionsRect.Contains(Event.current.mousePosition) )
			{
                Event.current.Use();

				GenericMenu menu = new GenericMenu();
				Type[] types = typeof(FEvent).Assembly.GetTypes();
				List<KeyValuePair<Type, FEventAttribute>> validTypeList = new List<KeyValuePair<Type, FEventAttribute>>();

				foreach( Type t in types )
				{
					if( !typeof(FEvent).IsAssignableFrom( t ) )
						continue;

					object[] attributes = t.GetCustomAttributes(typeof(FEventAttribute), false);
					if( attributes.Length == 0 )
						continue;

					validTypeList.Add( new KeyValuePair<Type, FEventAttribute>(t, (FEventAttribute)attributes[0]) );
				}

				validTypeList.Sort( delegate(KeyValuePair<Type, FEventAttribute> x, KeyValuePair<Type, FEventAttribute> y) 
				{
					return x.Value.menu.CompareTo( y.Value.menu );
				});

				foreach( KeyValuePair<Type, FEventAttribute> kvp in validTypeList )
				{
					menu.AddItem( new GUIContent(kvp.Value.menu), false, AddTrackMenu, kvp );
				}

				menu.ShowAsContext();
			}

			GUI.color = FGUI.GetTextColor();

			GUI.DrawTexture( timelineOptionsRect, (Texture2D)AssetDatabase.LoadAssetAtPath(FUtility.GetFluxSkinPath()+"Plus.png", typeof(Texture2D)) );

			if( Event.current.type == EventType.MouseDown && hierarchyHeaderRect.Contains( Event.current.mousePosition ) )
			{
				if( Event.current.button == 0 ) // dragging
				{
					EditorGUIUtility.hotControl = id;

					_offsetAnim.value = _offsetAnim.target = new Vector2( 0, hierarchyHeaderRect.yMin ) - Event.current.mousePosition;

					SequenceEditor.StartTimelineDrag( this );

					Event.current.Use();
				}
			}

#if UNITY_4_5
			if( _offsetAnim.isAnimating )
				_sequenceEditor.Repaint();
#endif
		}

		void AddTrackMenu( object param )
		{
			KeyValuePair<Type, FEventAttribute> kvp = (KeyValuePair<Type, FEventAttribute>)param;

			Undo.RecordObjects( new UnityEngine.Object[]{_timeline, this}, "add Track" );

			FTrack track = (FTrack)typeof(FTimeline).GetMethod("AddTrack").MakeGenericMethod( kvp.Key ).Invoke( _timeline, new object[]{SequenceEditor.GetViewRange()} );

			string evtName = track.gameObject.name;

			int nameStart = 0;
			int nameEnd = evtName.Length;
			if( nameEnd > 2 && evtName[0] == 'F' && char.IsUpper(evtName[1]) )
				nameStart = 1;
			if( evtName.EndsWith("Event") )
				nameEnd = evtName.Length - "Event".Length;
			evtName = evtName.Substring( nameStart, nameEnd - nameStart );

			track.gameObject.name = ObjectNames.NicifyVariableName( evtName );

			if( !_timeline.Sequence.IsStopped )
				track.Init();

			SequenceEditor.Refresh();

			Undo.RegisterCreatedObjectUndo( track.gameObject, string.Empty );

			SequenceEditor.SelectExclusive( SequenceEditor.GetEditor<FEventEditor>( track.GetEvent(0) ) );
		}

		void ChangeOwner()
		{
			Undo.RecordObject( _timeline, "Change Timeline Owner" );
			_timeline.SetOwner( Selection.activeTransform );
			_timeline.name = Selection.activeTransform.name;

			if( !SequenceEditor.GetSequence().IsStopped )
				_timeline.Init();
		}
        void AddTimeline()
        {
            Undo.IncrementCurrentGroup();
            UnityEngine.Object[] objsToSave = new UnityEngine.Object[] { SequenceEditor, SequenceEditor.GetSequence() };
            Undo.RecordObjects(objsToSave, string.Empty);

            GameObject timelineGO = new GameObject("TimeLine");
            FTimeline timeline = timelineGO.AddComponent<Flux.FTimeline>();
            //timeline.SetOwner(((GameObject)obj).transform);
            SequenceEditor.GetSequence().Add(timeline);

            Undo.RegisterCompleteObjectUndo(objsToSave, string.Empty);
            Undo.RegisterCreatedObjectUndo(timeline.gameObject, "create Timeline");
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }
		void DuplicateTimeline()
		{
			UnityEngine.Object[] objsToSave = new UnityEngine.Object[]{ SequenceEditor, SequenceEditor.GetSequence() };
			Undo.RecordObjects( objsToSave, string.Empty );
			GameObject duplicateTimeline = (GameObject)Instantiate( _timeline.gameObject );
			duplicateTimeline.name = _timeline.gameObject.name;
			Undo.SetTransformParent( duplicateTimeline.transform, _timeline.Sequence.TimelineContainer, string.Empty );
			Undo.RegisterCreatedObjectUndo( duplicateTimeline, "duplicate Timeline" );

			if( !SequenceEditor.GetSequence().IsStopped )
				duplicateTimeline.GetComponent<FTimeline>().Init();
		}

		void DeleteTimeline()
		{
			UnityEngine.Object[] objsToSave = new UnityEngine.Object[]{ SequenceEditor.GetSequence(), _timeline };
			Undo.RegisterCompleteObjectUndo( objsToSave, string.Empty );
			Undo.SetTransformParent( _timeline.transform, null, string.Empty );
			SequenceEditor.GetSequence().Remove( _timeline );
			Undo.DestroyObjectImmediate( _timeline.gameObject );
		}

		public void StartTrackDrag( FTrackEditor trackEditor )
		{
			_trackDragged = trackEditor;
			_sequenceEditor.StartTimelineDrag( this );
		}

		public void StopTrackDrag()
		{
			if( _trackDragged == null )
				return;

			Rect trackRect = _rect;
			trackRect.yMin += HEADER_HEIGHT;
			trackRect.height = TRACK_HEIGHT;

			int newPos = -1;

			float mouseY = Event.current.mousePosition.y;

			for( int i = 0; i != _trackEditors.Count; ++i )
			{
				if( mouseY >= trackRect.yMin && mouseY <= trackRect.yMax )
				{
					newPos = i;
					break;
				}

				trackRect.y += TRACK_HEIGHT;
			}

			string undoMoveTrackStr = "move Track";

			if( newPos == -1 )
			{
				Undo.SetTransformParent( _trackDragged._track.transform, _timeline.transform, undoMoveTrackStr );
				_trackEditors.RemoveAt( _trackDragged.GetRuntimeObject().GetId() );
				if( mouseY > trackRect.yMin )
				{
					_trackDragged._track.transform.SetAsLastSibling();
					_trackEditors.Add( _trackDragged );
				}
				else
				{
					_trackDragged._track.transform.SetAsFirstSibling();
					_trackEditors.Insert( 0, _trackDragged );
				}
			}
			else
			{
				if( newPos != _trackDragged._track.GetId() )
				{
					Undo.SetTransformParent( _trackDragged._track.transform, _timeline.transform, undoMoveTrackStr );
					_trackDragged._track.transform.SetSiblingIndex( newPos );

					_trackEditors[_trackDragged.GetRuntimeObject().GetId()] = null;
					_trackEditors.Insert( newPos, _trackDragged );
					_trackEditors.Remove( null );
				}
			}

			_trackDragged = null;

			for( int i = 0; i != _trackEditors.Count; ++i )
				_trackEditors[i].SetOffset( Vector2.zero, true );

			_sequenceEditor.CancelTimelineDrag();
		}

		public void CancelTrackDrag()
		{
			if( _trackDragged == null )
				return;

			_trackDragged = null;

			_sequenceEditor.CancelTimelineDrag();
		}

		public override FSequenceEditor SequenceEditor { get { return _sequenceEditor; } }

		public override FObject GetRuntimeObject()
		{
			return _timeline;
		}

		public void ReserveTrackGuiIds()
		{
			if( _trackEditorIds.Length != _trackEditors.Count )
			{
				_trackEditorIds = new int[_trackEditors.Count];
			}
			
			for( int i = 0; i != _trackEditorIds.Length; ++i )
			{
				_trackEditorIds[i] = EditorGUIUtility.GetControlID( FocusType.Passive );
			}
		}

		public void UpdateTracks( int frame, float time )
		{
			for( int i = 0; i != _trackEditors.Count; ++i )
				_trackEditors[i].UpdateEventsEditor( frame, time );
		}

		public void CreatePreviews()
		{
			for( int i = 0; i != _trackEditors.Count; ++i )
			{
				if( _trackEditors[i]._track.CanTogglePreview() )
					_trackEditors[i]._track.CreatePreview();
//				if( !(_trackEditors[i] is FPreviewableTrackEditor)
//				   && !(_trackEditors[i] is FAnimationTrackEditor) )
//					continue;

//				FPreviewableTrackEditor previewTrackEditor = (FPreviewableTrackEditor)_trackEditors[i];
//				if( previewTrackEditor != null )
//					previewTrackEditor.CreatePreview();

//				FAnimationTrackEditor animTrackEditor = (FAnimationTrackEditor)_trackEditors[i];
//
//				if( animTrackEditor != null )
//					animTrackEditor.PreviewInSceneView = true;
			}
		}

		public void ClearPreviews()
		{
			for( int i = 0; i != _trackEditors.Count; ++i )
			{
				_trackEditors[i]._track.ClearPreview();
//				if( !(_trackEditors[i] is FPreviewableTrackEditor)
//				   && !(_trackEditors[i] is FAnimationTrackEditor) )
//					continue;
				
				//				FPreviewableTrackEditor previewTrackEditor = (FPreviewableTrackEditor)_trackEditors[i];
				//				if( previewTrackEditor != null )
				//					previewTrackEditor.CreatePreview();
				
//				FAnimationTrackEditor animTrackEditor = (FAnimationTrackEditor)_trackEditors[i];
//				
//				if( animTrackEditor != null )
//					animTrackEditor.PreviewInSceneView = false;
			}
		}
	}
}
