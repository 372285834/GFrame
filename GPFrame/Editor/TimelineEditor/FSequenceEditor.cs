using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Flux;

namespace GPEditor
{
	[System.Serializable]
	public class FSequenceEditor : ScriptableObject
	{
		private const int VERTICAL_SCROLLER_WIDTH = 16;

		public const int RIGHT_BORDER = 20;

		private const int FRAME_RANGE_SCROLLER_HEIGHT = 16;

		private const int MINIMUM_HEADER_WIDTH = 150;

		private const int SCROLL_WHEEL_SPEED = 5;

		[SerializeField]
		private Editor _renderingOnEditor;
		[SerializeField]
		private EditorWindow _renderingOnEditorWindow;

		// current view range of the sequence
		[SerializeField]
		private FrameRange _viewRange;

		// how many pixels do we have to render per frame
		private float _pixelsPerFrame;

		// editor cache
		[SerializeField]
		private FEditorCache _editorCache;

		// sequence being edited
		[SerializeField]
		private FSequence _sequence;

//		[SerializeField]
//		private int _sequenceInstanceID;

//		private int _t;
		
		public List<FTrackEditor> _selectedTracks = new List<FTrackEditor>();
		public List<FEventEditor> _selectedEvents = new List<FEventEditor>();


		private bool _isDragSelecting = false;

		private Vector2 _dragSelectingStartPos;

//		private bool _isDraggingTimelines = false;

		private FTimelineEditor _timelineDragged = null;

		#region Cached rects to speed up render and not use GUILayout
		// full rect where it will be displayed
		[SerializeField]
		private Rect _rect;
		public Rect GetRect(){ return _rect; }

		// rect for the scroll bar on the left
		[SerializeField]
		private Rect _verticalScrollerRect;

		// rect where the timelines will be displayed
		[SerializeField]
		private Rect _viewRect;

		// rect for the view range bar
		[SerializeField]
		private Rect _viewRangeRect;

		// rect for the time scrubber
		[SerializeField]
		private Rect _timeScrubberRect;

		// origin-starting rect for the sequence timelines, to be used
		// inside the GUI.BeginGroup
		[SerializeField]
		private Rect _sequenceRect;

		// rect of the area where the timeline headers are
		[SerializeField]
		private Rect _timelineHeaderResizerRect;

		// width of the timeline header
		[SerializeField]
		private int _timelineHeaderWidth;
	
		// timeline scroll bar pos, only handles Y
		[SerializeField]
		private Vector2 _scrollPos;

		// editors for the timelines
		[SerializeField]
		private List<FTimelineEditor> _timelineEditors = new List<FTimelineEditor>();
		public List<FTimelineEditor> GetTimelineEditors() { return _timelineEditors; }
		#endregion

		#region GUI cached vars
		// stores the GUI control id for the timeline editors
		[SerializeField]
		private int[] _timelineEditorIds = new int[0];

		// stores the height of the timeline editor render
		[SerializeField]
		private float[] _timelineEditorHeights = new float[0];
		#endregion


		void OnEnable()
		{
			hideFlags = HideFlags.DontSave;
			EditorApplication.hierarchyWindowChanged += Refresh;
		}
		void OnDestroy()
		{
			_selectedEvents.Clear();
			_selectedTracks.Clear();

			if( _editorCache != null )
			{
				_editorCache.Clear();
				DestroyImmediate( _editorCache );
			}

//			EditorApplication.playmodeStateChanged -= OnPlayModeChanged;
			EditorApplication.hierarchyWindowChanged -= Refresh;
		}

		public void Init( Editor editor )
		{
			Init( editor, null );
		}

		public void Init( EditorWindow editorWindow )
		{
			Init( null, editorWindow );
		}

		private void Init( Editor editor, EditorWindow editorWindow )
		{
			_renderingOnEditor = editor;
			_renderingOnEditorWindow = editorWindow;

			_editorCache = CreateInstance<FEditorCache>();
		}

		public void Repaint()
		{
			if( _renderingOnEditor )
				_renderingOnEditor.Repaint();
			else if( _renderingOnEditorWindow )
				_renderingOnEditorWindow.Repaint();
		}

		public void Refresh()
		{
            if (_sequence == null)
                return;
			OpenSequence( _sequence );
		}

		public T GetEditor<T>( FObject obj ) where T : FEditor
		{
			return _editorCache.GetEditor<T>( obj );
		}

		public void OnUndo()
		{
			_editorCache.Refresh();

				FInspectorWindow.SetEvents( _selectedEvents );
				FInspectorWindow.SetTracks( _selectedTracks );
			

		}

		public void OnPlaymodeChanged()
		{
			Stop();

//			Debug.LogWarning("OnPlaymodeChanged: " + EditorApplication.isPlayingOrWillChangePlaymode );

//			Debug.LogWarning( "is playing: " + EditorApplication.isPlaying + " isPlayingOrWillChangePlaymode: " + EditorApplication.isPlayingOrWillChangePlaymode + " app is playing: " + Application.isPlaying );

			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				for( int j = 0; j != _timelineEditors[i]._trackEditors.Count; ++j )
				{
					if( _timelineEditors[i]._trackEditors[j]._track.CanTogglePreview() )
					{
						if( _timelineEditors[i]._trackEditors[j]._track )//&& !EditorApplication.isPlayingOrWillChangePlaymode ) // coming out of play mode
						{
							if( EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode )
								_timelineEditors[i]._trackEditors[j]._track.ClearPreview();
							else
								_timelineEditors[i]._trackEditors[j]._track.CreatePreview();
						}
					}
				}
			}
		}

		public FSequence GetSequence()
		{
			return _sequence;
		}

		public int GetHeaderWidth()
		{
			return _timelineHeaderWidth;
		}

		public FrameRange GetViewRange()
		{
			return _viewRange;
		}

		public void SetViewRange( FrameRange viewRange )
		{
			FrameRange sequenceRange = new FrameRange(0, _sequence.Length);
			viewRange.Start = sequenceRange.Cull( viewRange.Start );
			viewRange.End = sequenceRange.Cull( viewRange.End );

			if( viewRange.End <= viewRange.Start )
			{
				if( viewRange.Start == _sequence.Length )
				{
					viewRange.End = _sequence.Length;
					viewRange.Start = viewRange.End-1;
				}
				else
				{
					viewRange.End = viewRange.Start+1;
				}
			}

			_viewRange = viewRange;
		}

		public float GetXForFrame( int frame )
		{
			return _timelineHeaderWidth + ((frame - _viewRange.Start) * _pixelsPerFrame);
		}
		
		public int GetFrameForX( float x )
		{
			return _viewRange.Start + Mathf.RoundToInt( ((x - _timelineHeaderWidth) / _pixelsPerFrame) );
		}


		public void RebuildLayout( Rect rect )
		{
			_rect = rect;

			_viewRect = _rect;
			_viewRect.xMin += VERTICAL_SCROLLER_WIDTH;
			_viewRect.yMax -= (FRAME_RANGE_SCROLLER_HEIGHT + FGUI.TIMELINE_SCRUBBER_HEIGHT);

			_sequenceRect = _viewRect;
			_sequenceRect.xMin -= VERTICAL_SCROLLER_WIDTH;
			_sequenceRect.xMax -= VERTICAL_SCROLLER_WIDTH + RIGHT_BORDER;

//			_sequenceViewRect = _sequenceRect; // height needs to be adjusted at runtime

			_timelineHeaderWidth = Mathf.Max(MINIMUM_HEADER_WIDTH, _timelineHeaderWidth);

			_timeScrubberRect = _viewRect;
//			_timeScrubberRect.yMin = _viewRect.yMax;
			_timeScrubberRect.xMin += _timelineHeaderWidth;
			_timeScrubberRect.xMax -= RIGHT_BORDER;
			_timeScrubberRect.yMax += FGUI.TIMELINE_SCRUBBER_HEIGHT;

			_timelineHeaderResizerRect = _timeScrubberRect;
			_timelineHeaderResizerRect.xMin = 0;
			_timelineHeaderResizerRect.xMax = _timeScrubberRect.xMin;
			_timelineHeaderResizerRect.yMin = _timelineHeaderResizerRect.yMax-FGUI.TIMELINE_SCRUBBER_HEIGHT;

			_viewRangeRect = _rect;
			_viewRangeRect.yMin = _viewRangeRect.yMax - FRAME_RANGE_SCROLLER_HEIGHT;


			_verticalScrollerRect = _rect;
			_verticalScrollerRect.width = VERTICAL_SCROLLER_WIDTH;
			_verticalScrollerRect.yMax -= (FRAME_RANGE_SCROLLER_HEIGHT + FGUI.TIMELINE_SCRUBBER_HEIGHT);
		}

		public void OpenSequence( FSequence sequence )
		{
#if FLUX_DEBUG
			Debug.Log ( "Opening sequence: " + sequence );
#endif
			if( sequence == null )
			{
                Debug.LogError("sequence == null");
				if( !object.Equals( sequence, null ) )
					sequence = (FSequence)EditorUtility.InstanceIDToObject( sequence.GetInstanceID() );
			}

			bool sequenceChanged = _sequence != sequence && (object.Equals(_sequence, null) || object.Equals(sequence, null) || _sequence.GetInstanceID() != sequence.GetInstanceID());

//			Debug.Log ("selected sequence! Changed? " + sequenceChanged );

			if( sequenceChanged )
			{
				if( _sequence != null )
					Stop();
				_editorCache.Clear();
				_selectedEvents.Clear();
				_selectedTracks.Clear();
			}
			else
				_editorCache.Refresh();


			if( sequence != null )
			{
//				_sequenceInstanceID = sequence.GetInstanceID();
				if( _viewRange.Length == 0 )
					_viewRange = new FrameRange(0, sequence.Length);

				if( !EditorApplication.isPlaying )
					sequence.Rebuild();

				List<FTimeline> timelines = sequence.GetTimelines();
				
				_timelineEditors.Clear();

				for( int i = 0; i < timelines.Count; ++i )
				{
					FTimeline timeline = timelines[i];
					FTimelineEditor timelineEditor = GetEditor<FTimelineEditor>( timeline );
					timelineEditor.Init( timeline, this );
					_timelineEditors.Add( timelineEditor );
				}

				if( _viewRange.Length == 0 )
				{
					_viewRange = new FrameRange(0, sequence.Length);
				}
			}
			else
			{
//				_sequenceInstanceID = int.MinValue;
			}

			_sequence = sequence;
			
			Repaint();
		}

//		public void SetTimelineEditors( List<CTimelineEditor> timelineEditors )
//		{
//			_timelineEditors = timelineEditors;
//		}

		public void AddEvent( int t )
		{
			List<ISelectableElement> newEvents = new List<ISelectableElement>();

			foreach( FTimelineEditor timelineEditor in _timelineEditors )
			{
				foreach( FTrackEditor trackEditor in timelineEditor._trackEditors )
				{
					if( !trackEditor.IsSelected() )
						continue;
					FEvent evt = trackEditor.TryAddEvent( t );
					if( evt )
					{
						FEventEditor evtEditor = GetEditor<FEventEditor>( evt );
						evtEditor.Init( evt, trackEditor );
						newEvents.Add( evtEditor );
					}
				}
			}
			if( newEvents.Count > 0 )
			{
				DeselectAll();
				Select( newEvents );
			}
			else
				EditorApplication.Beep();
		}

		public void DestroyEvents( List<FEventEditor> events )
		{
			Object[] objs = new Object[_selectedEvents.Count+_selectedEvents.Count+_selectedTracks.Count+_selectedTracks.Count+1];
			
			int i = 0;
			
			for( int j = 0; j != _selectedTracks.Count; ++j, ++i )
			{
				objs[i] = _selectedTracks[j];
			}
			
			for( int j = 0; j != _selectedTracks.Count; ++j, ++i )
			{
				objs[i] = _selectedTracks[j]._track;
			}
			
			for( int j = 0; j != events.Count; ++j, ++i )
			{
				objs[i] = events[j];
			}
			
			for( int j = 0; j != events.Count; ++j, ++i )
			{
				objs[i] = events[j]._evt;
			}
			
			objs[i] = this; // save the list
			
//			Undo.RegisterCompleteObjectUndo( objs, "delete Events" );
			Undo.RecordObjects( objs, "delete Events");
			
			for( int j = 0; j != events.Count; ++j )
			{
				_editorCache.Remove( events[j] );

				events[j]._trackEditor._eventEditors.Remove( events[j] );
				Undo.SetTransformParent( events[j]._evt.transform, null, string.Empty );
				events[j]._evt.GetTrack().Remove( events[j]._evt );
				
				GameObject evtGO = events[j]._evt.gameObject;
				Undo.DestroyObjectImmediate( events[j] );
				Undo.DestroyObjectImmediate( evtGO );
			}
			
			_selectedEvents.Clear();

//			for( int j = 0; j != _selectedTracks.Count; ++j )
//			{
//				_selectedTracks[j].OnTrackChanged();
//			}
		}

		private FTimelineEditor GetTimeline( Vector2 pos, out bool isOnHeader )
		{
			isOnHeader = false;
			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				Rect timelineRect = _timelineEditors[i].GetRect();
				
				if( timelineRect.Contains( pos ) )
				{
					isOnHeader = pos.y <= timelineRect.yMin + FTimelineEditor.HEADER_HEIGHT;
					return _timelineEditors[i];
				}
			}
			
			return null;
		}
		
		
		private FTrackEditor GetTrack( Vector2 pos )
		{
			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				if( !_timelineEditors[i].GetRect().Contains( pos ) )
					continue;
				
				List<FTrackEditor> tracks = _timelineEditors[i]._trackEditors;
				for( int j = 0; j != tracks.Count; ++j )
				{
					if( tracks[j].GetRect().Contains( pos ) )
						return tracks[j];
				}
				//				if( timelineList[i].GetRect().Contains( pos ) )
				//				{
				//
				//				}
			}
			
			return null;
		}

		public void OnGUI()
		{
			if( _timelineEditors == null )
				return;

			if( _timelineEditors.Count == 0 )
			{
				if( _renderingOnEditorWindow )
					_renderingOnEditorWindow.ShowNotification( new GUIContent("Drag GameObjects Here") );
			}

			_pixelsPerFrame = (_sequenceRect.width - _timelineHeaderWidth) / _viewRange.Length;

			if( _timelineEditorIds.Length != _timelineEditors.Count )
			{
				_timelineEditorIds = new int[_timelineEditors.Count];
				_timelineEditorHeights = new float[_timelineEditors.Count];
			}

			int timelineHeaderResizerId = EditorGUIUtility.GetControlID(FocusType.Passive);

			float sequenceViewHeight = 0;
			
			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				_timelineEditorIds[i] = EditorGUIUtility.GetControlID(FocusType.Passive);
				_timelineEditorHeights[i] = _timelineEditors[i].GetHeight();
				sequenceViewHeight += _timelineEditorHeights[i];

				_timelineEditors[i].ReserveTrackGuiIds();
			}

			_scrollPos.y = GUI.VerticalScrollbar( _verticalScrollerRect, _scrollPos.y, Mathf.Min( _sequenceRect.height, sequenceViewHeight ), 0, sequenceViewHeight );

			Rect scrolledViewRect = _viewRect;
//			scrolledViewRect.yMin -= _scrollPos.y;

			GUI.BeginGroup( scrolledViewRect );

			Rect timelineRect = _sequenceRect;
			timelineRect.y = -_scrollPos.y;
			timelineRect.height = 0;

			Rect timelineDraggedRect = new Rect();

//			Debug.Log( "sequence: " + _sequenceRect + " scrubber: " + _timeScrubberRect + " width: " + _timelineHeaderWidth );

			Handles.color = FGUI.GetLineColor();

			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				timelineRect.yMin = timelineRect.yMax;
				timelineRect.height = _timelineEditors[i].GetHeight();

				if( _timelineDragged != null )
				{
					if( _timelineDragged == _timelineEditors[i] )
					{
						timelineDraggedRect = timelineRect;
						continue;
					}
					else if( EditorGUIUtility.hotControl == _timelineEditorIds[_timelineDragged.GetRuntimeObject().GetId()] )
					{
						if( i < _timelineDragged.GetRuntimeObject().GetId() && Event.current.mousePosition.y < timelineRect.yMax )
							_timelineEditors[i].SetOffset( new Vector2(0, _timelineDragged.GetHeight()) );
						else if( i > _timelineDragged.GetRuntimeObject().GetId() && Event.current.mousePosition.y > timelineRect.yMin )
							_timelineEditors[i].SetOffset( new Vector2(0, -_timelineDragged.GetHeight()) );
						else
							_timelineEditors[i].SetOffset( Vector2.zero );
					}
				}

				_timelineEditors[i].Render( _timelineEditorIds[i], timelineRect, _timelineHeaderWidth, _viewRange, _pixelsPerFrame );
			}

			if( _timelineDragged != null )
			{
				if( EditorGUIUtility.hotControl == _timelineEditorIds[_timelineDragged.GetRuntimeObject().GetId()] )
					timelineDraggedRect.y = Event.current.mousePosition.y;
//				timelineRect.yMin = Event.current.mousePosition.y;
//				timelineRect.height = _timelineDragged.GetHeight();
				_timelineDragged.Render( _timelineEditorIds[_timelineDragged.GetRuntimeObject().GetId()], timelineDraggedRect, _timelineHeaderWidth, _viewRange, _pixelsPerFrame );
			}

			switch( Event.current.type )
			{
			case EventType.MouseDown:
				
//				bool middleClick = Event.current.button == 2 || (Event.current.button == 0 && Event.current.alt);
//				
//				if( middleClick ) // middle button
				if( Event.current.button == 0 )
				{
					StartDragSelecting( Event.current.mousePosition );
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if( _isDragSelecting )
				{
					StopDragSelecting( Event.current.mousePosition );
					Event.current.Use();
				}
				break;
			case EventType.Ignore:
				if( _isDragSelecting )
					StopDragSelecting( Event.current.mousePosition );
				break;
			case EventType.Repaint:
				if( _isDragSelecting )
					OnDragSelecting( Event.current.mousePosition );

				break;
			}

			GUI.EndGroup();

			if( _viewRange.End > _sequence.Length )
			{
				_viewRange.Start = 0;
				_viewRange.End = _sequence.Length;
			}

			int newT = FGUI.TimeScrubber( _timeScrubberRect, _sequence.GetCurrentFrame(), _sequence.FrameRate, _viewRange );

			if( newT != _sequence.GetCurrentFrame() )
			{
//				_sequence.SetCurrentFrameEditor( newT );
				SetCurrentFrame( newT );

				if( Application.isPlaying )
				{
					Play( false );
					Pause();
				}
//				Repaint();
//				SceneView.RepaintAll();
//				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
//				FUtility.RepaintGameView();
			}

			_viewRange = FGUI.ViewRangeBar( _viewRangeRect, _viewRange, _sequence.Length );

			if( _timelineHeaderResizerRect.Contains(Event.current.mousePosition) )
				EditorGUIUtility.AddCursorRect( _timelineHeaderResizerRect, MouseCursor.ResizeHorizontal );

			switch( Event.current.type )
			{
			case EventType.MouseDown:

				bool leftClick = Event.current.button == 0 && !Event.current.alt;
//				bool middleClick = Event.current.button == 2 || (Event.current.button == 0 && Event.current.alt);

				if( leftClick ) // left button
				{
					if( EditorGUIUtility.hotControl == 0 && _timelineHeaderResizerRect.Contains(Event.current.mousePosition) )
					{
						EditorGUIUtility.hotControl = timelineHeaderResizerId;
						Event.current.Use();
					}
					else if( _rect.Contains( Event.current.mousePosition ) ) 
					{
						DeselectAll();
						Event.current.Use();
					}
				}
//				else if( middleClick ) // middle button
//				{
//					StartDragSelecting( Event.current.mousePosition );
//					Event.current.Use();
//				}
				break;
			case EventType.MouseDrag:
				if( EditorGUIUtility.hotControl == timelineHeaderResizerId )
				{
					_timelineHeaderWidth = (int)Mathf.Max( MINIMUM_HEADER_WIDTH, _timelineHeaderWidth+Event.current.delta.x );

					RebuildLayout( _rect );
					EditorWindow.GetWindow<FSequenceEditorWindow>().Repaint();
					Event.current.Use();
				}

				if( _isDragSelecting )
					Repaint();

				break;
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl == timelineHeaderResizerId )
				{
					EditorGUIUtility.hotControl = 0;
					Event.current.Use();
				}
//				if( _isDragSelecting )
//				{
//					StopDragSelecting( Event.current.mousePosition );
//					Event.current.Use();
//				}
				break;
			case EventType.Repaint:
				Rect dragArea = _timelineHeaderResizerRect;
				dragArea.xMax -= 10;
				dragArea.xMin = dragArea.xMax - 16;
				GUIStyle dragStyle = FUtility.GetFluxSkin().GetStyle( "HorizontalPanelSeparatorHandle" );
				dragStyle.Draw( dragArea, GUIContent.none, 0 );
//				GUI.DrawTexture( dragArea, EditorGUIUtility.whiteTexture );
				Handles.color = FGUI.GetLineColor();// new Color(0.8f, 0.8f, 0.8f, 0.2f);
				Handles.DrawLine( new Vector3(_viewRect.xMin-16, _sequenceRect.yMax, 0), new Vector3(_viewRect.xMax-RIGHT_BORDER, _sequenceRect.yMax, 0) );
//				Handles.color = Color.black;
				break;
			case EventType.ScrollWheel:
				if( _viewRect.Contains(Event.current.mousePosition) )
				{
					_scrollPos.y += Event.current.delta.y * SCROLL_WHEEL_SPEED;
					Event.current.Use();
				}
				break;
			}

#if FLUX_TRIAL
			GUIStyle watermarkLabel = new GUIStyle( GUI.skin.label );
			watermarkLabel.fontSize = 24;
			GUIContent watermark = new GUIContent("..::FLUX TRIAL::..");
			Vector2 watermarkSize = watermarkLabel.CalcSize( watermark );
			Rect watermarkRect = new Rect( _rect.width*0.5f-watermarkSize.x*0.5f, _rect.height*0.5f - watermarkSize.y*0.5f, watermarkSize.x, watermarkSize.y );

			GUI.color = new Color( 1f, 1f, 1f, 0.4f );
			GUI.Label( watermarkRect, watermark, watermarkLabel );
#endif
		}

		public void StartTimelineDrag( FTimelineEditor timelineEditor )
		{
			_timelineDragged = timelineEditor;
		}

		public void StopTimelineDrag()
		{
			Rect timelineRect = _sequenceRect;
			timelineRect.y = -_scrollPos.y;
			timelineRect.height = 0;

			int newPos = -1;

			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				timelineRect.yMin = timelineRect.yMax;
				timelineRect.height = _timelineEditorHeights[i];

				if( Event.current.mousePosition.y > timelineRect.yMin && Event.current.mousePosition.y < timelineRect.yMax )
				{
					newPos = i;
					break;
				}
			}

			string undoMoveTimelineStr = "move Timeline";

			if( newPos == -1 ) // moved it to the end of the list
			{
				Undo.SetTransformParent( _timelineDragged._timeline.transform, _sequence.TimelineContainer, undoMoveTimelineStr );
				_timelineDragged._timeline.transform.SetAsLastSibling();

				_timelineEditors.RemoveAt( _timelineDragged.GetRuntimeObject().GetId() );
				_timelineEditors.Add( _timelineDragged );
			}
			else 
			{
				if( newPos != _timelineDragged._timeline.GetId() )
				{
					Undo.SetTransformParent( _timelineDragged._timeline.transform, _sequence.TimelineContainer, undoMoveTimelineStr );
					_timelineDragged._timeline.transform.SetSiblingIndex( newPos );

					_timelineEditors[_timelineDragged.GetRuntimeObject().GetId()] = null;
					_timelineEditors.Insert( newPos, _timelineDragged );
					_timelineEditors.Remove( null );
				}
			}

			_timelineDragged = null;

//			_sequence.Rebuild();

			// move where?
			for( int i = 0; i != _timelineEditors.Count; ++i )
				_timelineEditors[i].SetOffset( Vector2.zero, true );
		}

		public void CancelTimelineDrag()
		{
			_timelineDragged = null;
		}

		private void StartDragSelecting( Vector2 startPos )
		{
			_isDragSelecting = true;
			_dragSelectingStartPos = startPos;
		}

		private void StopDragSelecting( Vector2 mousePos )
		{
			_isDragSelecting = false;

			FrameRange selectedRange = new FrameRange();
			bool isSelectingTimelines;

			Rect selectionRect = GetDragSelectionRect( _dragSelectingStartPos, mousePos, out selectedRange, out isSelectingTimelines );

			if( !Event.current.shift && !Event.current.control )
				DeselectAll();

			for( int i = 0; i != _timelineEditors.Count; ++i )
			{
				Rect timelineRect = _timelineEditors[i].GetRect();
				if( timelineRect.yMin >= selectionRect.yMax )
					break;

				if( timelineRect.yMax <= selectionRect.yMin )
					continue;

				for( int j = 0; j != _timelineEditors[i]._trackEditors.Count; ++j )
				{
					Rect trackRect = _timelineEditors[i]._trackEditors[j].GetRect();

					if( trackRect.yMin >= selectionRect.yMax )
						break;

					if( trackRect.yMax <= selectionRect.yMin )
						continue;

					if( Event.current.control )
					{
						_timelineEditors[i]._trackEditors[j].DeselectEvents( selectedRange );
					}
					else
					{
						_timelineEditors[i]._trackEditors[j].SelectEvents( selectedRange );
					}
				}
			}
		}


		private void OnDragSelecting( Vector2 mousePos )
		{
			if( !_isDragSelecting )
				return;

			if( Event.current.shift )
			{
				EditorGUIUtility.AddCursorRect( _rect, MouseCursor.ArrowPlus );
			}
			else if( Event.current.control )
			{
				EditorGUIUtility.AddCursorRect( _rect, MouseCursor.ArrowMinus );
			}

			FrameRange selectedRange = new FrameRange();
			bool isSelectingTimelines;

			Rect selectionRect = GetDragSelectionRect( _dragSelectingStartPos, mousePos, out selectedRange, out isSelectingTimelines );
			if( selectionRect.width == 0 )
				selectionRect.width = 1;
			GUI.color = FGUI.GetSelectionColor();
			GUI.DrawTexture( selectionRect, EditorGUIUtility.whiteTexture );
		}

		private Rect GetDragSelectionRect( Vector2 rawStartPos, Vector2 rawEndPos, out FrameRange selectedRange, out bool isSelectingTimelines )
		{
			int startFrame = GetFrameForX( rawStartPos.x );
			int endFrame = GetFrameForX( rawEndPos.x );

			if( startFrame > endFrame )
			{
				int temp = startFrame;
				startFrame = endFrame;
				endFrame = temp;
			}

			selectedRange = new FrameRange(startFrame, endFrame);

			Rect rect = new Rect();

			Vector2 startPos = new Vector2( GetXForFrame( startFrame ), rawStartPos.y );
			Vector2 endPos = new Vector2( GetXForFrame( endFrame ), rawEndPos.y );

			bool isStartOnHeader;
			bool isEndOnHeader;
			
			FTimelineEditor startTimeline = GetTimeline( startPos, out isStartOnHeader );

			isSelectingTimelines = isStartOnHeader;

			if( startTimeline != null )
			{
				FTrackEditor startTrack = GetTrack( startPos );
				FTrackEditor endTrack;
				
				FTimelineEditor endTimeline = GetTimeline( endPos, out isEndOnHeader );
				if( endTimeline == null )
				{
					endTimeline = startTimeline;
					isEndOnHeader = isStartOnHeader;
					endTrack = startTrack;
				}
				else
					endTrack = GetTrack( endPos );
				
				float xStart = Mathf.Min( startPos.x, endPos.x );
				float width = Mathf.Abs( startPos.x - endPos.x );
				float yStart;
				float height;
				
				
				if( startPos.y <= endPos.y )
				{
					yStart = isStartOnHeader ? startTimeline.GetRect().yMin : startTrack.GetRect().yMin;
					height = (isStartOnHeader ? endTimeline.GetRect().yMax : (isEndOnHeader ? endTimeline.GetRect().yMin + FTimelineEditor.HEADER_HEIGHT : endTrack.GetRect().yMax)) - yStart;
					//					yStart = isStartOnHeader ? startTimeline.GetRect().yMin : startTrack.GetRect().yMin;
					//					height = (isEndOnHeader ? endTimeline._trackEditors[endTimeline._trackEditors.Count-1].GetRect().yMax : endTrack.GetRect().yMax) - yStart;
				}
				else
				{
					yStart = isStartOnHeader || isEndOnHeader ? endTimeline.GetRect().yMin : endTrack.GetRect().yMin;
					height = (isStartOnHeader ? startTimeline.GetRect().yMax : startTrack.GetRect().yMax) - yStart;
					//					yStart = isEndOnHeader ? endTimeline.GetRect().yMin : endTrack.GetRect().yMin;
					//					height = Mathf.Max( (isStartOnHeader ? startTimeline._trackEditors[startTimeline._trackEditors.Count-1].GetRect().yMax : startTrack.GetRect().yMax),
					//					                   (isEndOnHeader ? endTimeline._trackEditors[endTimeline._trackEditors.Count-1].GetRect().yMax : endTrack.GetRect().yMax) ) - yStart;
				}

				rect.x = xStart;
				rect.y = yStart;
				rect.width = width;
				rect.height = height;
			}

			return rect;
		}

		public void MoveEvents( int deltaFrames )
		{
			bool movingLeft = deltaFrames < 0;

			int howMuchCanMove = int.MaxValue;

			for( int i = 0; i != _selectedTracks.Count; ++i )
			{
				if( movingLeft )
				{
					for( int j = 0; j != _selectedTracks[i]._eventEditors.Count; ++j )
					{
						FEventEditor evtEditor = _selectedTracks[i]._eventEditors[j];
						if( evtEditor.IsSelected() )
						{
							if( j == 0 )
								howMuchCanMove = Mathf.Min( howMuchCanMove, evtEditor._evt.Start );
							else if( !_selectedTracks[i]._eventEditors[j-1].IsSelected() )
								howMuchCanMove =  Mathf.Min( howMuchCanMove , evtEditor._evt.Start - _selectedTracks[i]._eventEditors[j-1]._evt.End );
						}
					}
				}
				else
				{
					int lastElementIndex = _selectedTracks[i]._eventEditors.Count-1;
					for( int j = lastElementIndex; j != -1; --j )
					{
						FEventEditor evtEditor = _selectedTracks[i]._eventEditors[j];
						if( evtEditor.IsSelected() )
						{
							if( j == lastElementIndex )
								howMuchCanMove = Mathf.Min( howMuchCanMove, _sequence.Length - evtEditor._evt.End );
							else if( !_selectedTracks[i]._eventEditors[j+1].IsSelected() )
								howMuchCanMove = Mathf.Min( howMuchCanMove, _selectedTracks[i]._eventEditors[j+1]._evt.Start - evtEditor._evt.End );
						}
					}
				}
			}

			if( movingLeft )
			{
				howMuchCanMove = -howMuchCanMove;
				deltaFrames = Mathf.Clamp( deltaFrames, howMuchCanMove, 0 );
			}
			else
			{
				deltaFrames = Mathf.Clamp( deltaFrames, 0, howMuchCanMove );
			}

			if( deltaFrames != 0 )
			{
				for( int i = 0; i != _selectedEvents.Count; ++i )
				{
					Undo.RecordObject( _selectedEvents[i]._evt, "move Event" );
					_selectedEvents[i]._evt.Start += deltaFrames;
					_selectedEvents[i]._evt.End += deltaFrames;
					EditorUtility.SetDirty( _selectedEvents[i]._evt );
				}
			}
		}

		public void ResizeEventsLeft( int delta )
		{
			int howMuchCanResize = int.MaxValue;

			// making them bigger?
			if( delta < 0 )
			{
				for( int i = 0; i != _selectedEvents.Count; ++i )
				{
					int evtId = _selectedEvents[i].GetRuntimeObject().GetId ();
					int howMuchCanEvtResize = _selectedEvents[i]._evt.Start;
					if( evtId > 0 )
						howMuchCanEvtResize -= _selectedEvents[i]._evt.GetTrack().GetEvent( evtId - 1 ).End;

					if( howMuchCanResize > howMuchCanEvtResize )
						howMuchCanResize = howMuchCanEvtResize;
				}

				delta = Mathf.Clamp( delta, -howMuchCanResize, 0 );
			}
			else // making them smaller
			{
				for( int i = 0; i != _selectedEvents.Count; ++i )
				{
					int howMuchCanEvtResize = _selectedEvents[i]._evt.Length-_selectedEvents[i]._evt.GetMinLength();
					if( howMuchCanResize > howMuchCanEvtResize )
						howMuchCanResize = howMuchCanEvtResize;
				}

				delta = Mathf.Clamp( delta, 0, howMuchCanResize );
			}

			for( int i = 0; i != _selectedEvents.Count; ++i )
			{
				FrameRange evtRange = _selectedEvents[i]._evt.FrameRange;
				evtRange.Start += delta;
				FUtility.Resize( _selectedEvents[i]._evt, evtRange );
			}
		}

		public void ResizeEventsRight( int delta )
		{
			int howMuchCanResize = int.MaxValue;
			
			// making them bigger?
			if( delta > 0 )
			{
				for( int i = 0; i != _selectedEvents.Count; ++i )
				{
					int evtId = _selectedEvents[i].GetRuntimeObject().GetId ();
					int howMuchCanEvtResize = _selectedEvents[i]._evt.IsLastEvent() ? _sequence.Length : _selectedEvents[i]._evt.GetTrack().GetEvent( evtId + 1 ).Start;

					howMuchCanEvtResize -= _selectedEvents[i]._evt.End;

					if( howMuchCanResize > howMuchCanEvtResize )
						howMuchCanResize = howMuchCanEvtResize;
				}
				
				delta = Mathf.Clamp( delta, 0, howMuchCanResize );
			}
			else // making them smaller
			{
				for( int i = 0; i != _selectedEvents.Count; ++i )
				{
					int howMuchCanEvtResize = _selectedEvents[i]._evt.Length-_selectedEvents[i]._evt.GetMinLength();
					if( howMuchCanResize > howMuchCanEvtResize )
						howMuchCanResize = howMuchCanEvtResize;
				}
				
				delta = Mathf.Clamp( delta, -howMuchCanResize, 0 );
			}
			
			for( int i = 0; i != _selectedEvents.Count; ++i )
			{
				FrameRange evtRange = _selectedEvents[i]._evt.FrameRange;
				evtRange.End += delta;
				FUtility.Resize( _selectedEvents[i]._evt, evtRange );
			}
		}


		public void Select( ISelectableElement e )
		{
			if( e.IsSelected() )
				return;
			
			if( e is FTrackEditor )
				SelectTrack( (FTrackEditor)e );
			else if( e is FEventEditor )
				SelectEvent( (FEventEditor)e );
			else if( e is FTimelineEditor )
				SelectTimeline( (FTimelineEditor)e );
			
			Repaint();
		}
		
		public void SelectExclusive( ISelectableElement e )
		{
			if( e.IsSelected() )
				return;
			
			//			Undo.IncrementCurrentGroup();
			DeselectAll();
			
			if( e is FTrackEditor )
				SelectTrack( (FTrackEditor)e );
			else if( e is FEventEditor )
				SelectEvent( (FEventEditor)e );
			
			//			Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
			
			Repaint();
		}
		
		public void Select( List<ISelectableElement> elements )
		{
			Undo.IncrementCurrentGroup();
			
			foreach( ISelectableElement e in elements )
			{
				if( !e.IsSelected() )
					Select( e );
			}
			
			Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
			
			Repaint();
		}
		
		public void Deselect( List<ISelectableElement> elements )
		{
			Undo.IncrementCurrentGroup();
			
			foreach( ISelectableElement e in elements )
			{
				if( e.IsSelected() )
					Deselect( e );
			}
			
			Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
			
			Repaint();
		}
		
		public void Deselect( ISelectableElement e )
		{
			//			Debug.Log( "deselect " + e.GetType().ToString() );
			if( e is FTrackEditor )
				DeselectTrack( (FTrackEditor) e );
			else if( e is FEventEditor )
				DeselectEvent( (FEventEditor)e );
			
			Repaint();
		}
		
		private void SelectTrack( FTrackEditor trackEditor )
		{
			if( trackEditor == null || trackEditor.IsSelected() )
				return;
			
			Undo.RegisterCompleteObjectUndo( new Object[]{ trackEditor, this }, "select Track");
			
			_selectedTracks.Add( trackEditor );
			trackEditor.OnSelect();

				FInspectorWindow.SetTracks( _selectedTracks );
		}
		
		private void DeselectTrack( FTrackEditor trackEditor )
		{
			if( trackEditor == null || !trackEditor.IsSelected() )
				return;
			
			Undo.RegisterCompleteObjectUndo( new Object[]{ trackEditor, this }, "deselect Track");
			
			_selectedTracks.Remove( trackEditor );
			trackEditor.OnDeselect();
		}
		
		private void DeselectAll()
		{
			int numEvents = _selectedEvents.Count;
			int numTracks = _selectedTracks.Count;
			
			int totalSelected = numEvents + numTracks;
			
			if( totalSelected == 0 )
				return;
			
			// tracks + events + window
			Object[] objsToSave = new Object[totalSelected+1];
			
			int i = 0;
			
			for( int j = 0; j != numEvents; ++i, ++j )
				objsToSave[i] = _selectedEvents[j];
			
			for( int j = 0; j != numTracks; ++i, ++j )
				objsToSave[i] = _selectedTracks[j];
			
			objsToSave[totalSelected] = this;
			
			Undo.RegisterCompleteObjectUndo( objsToSave, "deselect all" );
			
			for( int j = 0; j != numEvents; ++j )
				_selectedEvents[j].OnDeselect();
			
			for( int j = 0; j != numTracks; ++j )
				_selectedTracks[j].OnDeselect();
			
			_selectedEvents.Clear();
			_selectedTracks.Clear ();
			
				FInspectorWindow.SetEvents( null );
				FInspectorWindow.SetTracks( null );
			
		}
		
		private void SelectEvent( FEventEditor eventEditor )
		{
			if( eventEditor == null || eventEditor.IsSelected() )
				return;
			
			SelectTrack( eventEditor._trackEditor );
			Undo.RegisterCompleteObjectUndo( new Object[]{ eventEditor, this }, "select Event" );
			
			_selectedEvents.Add( eventEditor );
				FInspectorWindow.SetEvents( _selectedEvents );
			
			eventEditor.OnSelect();
		}
		
		private void DeselectEvent( FEventEditor eventEditor )
		{
			if( eventEditor == null || !eventEditor.IsSelected() )
				return;
			
			Undo.RegisterCompleteObjectUndo( new Object[]{ eventEditor, this }, "deselect Event" );
			
			_selectedEvents.Remove( eventEditor );
				FInspectorWindow.SetEvents( _selectedEvents );
			
			eventEditor.OnDeselect();
		}

		private void SelectTimeline( FTimelineEditor timelineEditor )
		{
			if( timelineEditor == null || timelineEditor.IsSelected() )
				return;

			Undo.RecordObject( timelineEditor, "select Timeline" );

			timelineEditor.OnSelect();
		}

		#region Playback functions

		private bool _isPlaying = false;
		public bool IsPlaying { get { return _isPlaying; } }

		private double _timeStartedPlaying = 0;

		public void Play()
		{
			Play( true );
		}

		public void Play( bool restart )
		{
			if( !_sequence.IsStopped && restart )
				_sequence.Stop();



			int frame = _viewRange.Cull( _sequence.GetCurrentFrame() );

			_sequence.Play( frame );

			_timeStartedPlaying = EditorApplication.timeSinceStartup - (frame - _viewRange.Start)*_sequence.InverseFrameRate;

			SetCurrentFrame( frame );

			_isPlaying = true;

			FUtility.RepaintGameView();
		}

		public void Stop()
		{
			if( !object.Equals(_sequence, null) )
			{ 
				if( !_sequence.IsStopped )
				{
					_sequence.Stop( true );

					for( int i = 0; i != _timelineEditors.Count; ++i )
						_timelineEditors[i].OnStop();
				}

			}
			_isPlaying = false;

			FUtility.RepaintGameView();
		}

		public void Pause()
		{
//			Debug.Log ("Pause");
			_sequence.Pause();

			_isPlaying = false;

			FUtility.RepaintGameView();
		}

		public void SetCurrentFrame( int frame )
		{
			if( !_sequence.IsInit )
				_sequence.Init();

			_sequence.SetCurrentFrameEditor( frame );

			frame = _sequence.GetCurrentFrame();
			float time = frame * _sequence.InverseFrameRate;

			for( int i = 0; i != _timelineEditors.Count; ++i )
				_timelineEditors[i].UpdateTracks( frame, time );

			FUtility.RepaintGameView();
		}

		public void Update()
		{
			if( !_isPlaying )
				return;

			int t = _viewRange.Start + Mathf.RoundToInt(((float)(EditorApplication.timeSinceStartup - _timeStartedPlaying)*_sequence.FrameRate));

			if( t > _viewRange.End )
			{
				Play();
//				_sequence.Stop();
//				t = _timelineView.GetViewRange().Start;
//				timeStartPlaying = EditorApplication.timeSinceStartup;
			}
			else
				SetCurrentFrame( t );
			//				sequence.SetCurrentFrame( t );
//			_sequence.SetCurrentFrameEditor( t );
			
			//				UpdateTimer();
			
			//				Repaint();
			//				SceneView.RepaintAll();
			Repaint();
//			FUtility.RepaintGameView();

		}

		#endregion
	}
}
