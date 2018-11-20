using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GP;


namespace GPEditor
{
	public class FEventEditor : FEditor
	{
		public FTrackEditor _trackEditor;
		public FEvent _evt;

		private int _mouseOffsetFrames;

		protected Rect _eventRect;

		public override GTimelineEditor SequenceEditor { get { return _trackEditor.SequenceEditor; } }

		public void Init( FObject obj, FTrackEditor trackEditor )
		{
			_trackEditor = trackEditor;
			Init( obj );
		}

		protected override void Init( FObject obj )
		{
			_evt = (FEvent)obj;
		}

		public override void RefreshRuntimeObject()
		{
			_evt = (FEvent)EditorUtility.InstanceIDToObject(_evt.GetInstanceID());
		}

		// pixelsPerFrame can be calculated from rect and viewRange, but being cached on a higher level
		public void Render (Rect rect, FrameRange viewRange, float pixelsPerFrame, FrameRange validKeyframeRange)
		{
			_rect = rect;

			_eventRect = rect;

			int eventStartFrame = Mathf.Max( _evt.Start, viewRange.Start );
			int eventEndFrame = Mathf.Min( _evt.End, viewRange.End );
			
			_eventRect.xMin += (eventStartFrame-viewRange.Start) * pixelsPerFrame; // first set the start
			_eventRect.xMax = _eventRect.xMin + Mathf.Max( 4, (eventEndFrame-eventStartFrame) * pixelsPerFrame ); // then width


			
			RenderEvent( viewRange, validKeyframeRange );
		}
		
		protected virtual void RenderEvent( FrameRange viewRange, FrameRange validKeyframeRange )
		{

			int leftHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );
			int evtHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );
			int rightHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );

			bool leftHandleVisible = viewRange.Contains( _evt.Start );
			bool rightHandleVisible = viewRange.Contains( _evt.End );

			Rect leftHandleRect = _eventRect; leftHandleRect.width = 4;
			Rect rightHandleRect = _eventRect; rightHandleRect.xMin = rightHandleRect.xMax - 4;

			if( leftHandleVisible && IsSelected() )
				EditorGUIUtility.AddCursorRect( leftHandleRect, MouseCursor.ResizeHorizontal, leftHandleId );

			if( rightHandleVisible && IsSelected() )
				EditorGUIUtility.AddCursorRect( rightHandleRect, MouseCursor.ResizeHorizontal, rightHandleId );

			switch( Event.current.type )
			{
			case EventType.Repaint:
				if( !viewRange.Overlaps(_evt.FrameRange) )
				{
					break;
				}
				
				GUISkin skin = FUtility.GetFluxSkin();
				GUIStyle eventStyle = skin.GetStyle("Event");

				GUI.color = GetColor();

				eventStyle.Draw( _eventRect, _isSelected, _isSelected, false, false );

				GUI.color = Color.white;

				break;

			case EventType.MouseDown:
				if( EditorGUIUtility.hotControl == 0 && IsSelected() && !Event.current.control && !Event.current.shift )
				{
					Vector2 mousePos = Event.current.mousePosition;

					if( rightHandleVisible && rightHandleRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = rightHandleId;
//						keyframeOnSelect = evt.Start;
						Event.current.Use();
					}
					else if( leftHandleVisible && leftHandleRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = leftHandleId;
//						keyframeOnSelect = evt.End;
						Event.current.Use();
					}
					else if( _eventRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = evtHandleId;
						_mouseOffsetFrames = SequenceEditor.GetFrameForX( mousePos.x ) - _evt.Start;

						if( IsSelected() )
						{
							if( Event.current.control )
							{
								SequenceEditor.Deselect( this );
							}
						}
						else
						{
							if( Event.current.shift )
								SequenceEditor.Select( this );
							else if( !Event.current.control )
								SequenceEditor.SelectExclusive( this );
						}
						Event.current.Use();
					}
				}
				break;
				
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl != 0 )
				{
					if( EditorGUIUtility.hotControl == evtHandleId
					   || EditorGUIUtility.hotControl == leftHandleId 
					   || EditorGUIUtility.hotControl == rightHandleId )
					{
						EditorGUIUtility.hotControl = 0;
						Event.current.Use();
						SequenceEditor.Repaint();
					}
				}
				break;
				
			case EventType.MouseDrag:
				if( EditorGUIUtility.hotControl != 0 )
				{
	                if( EditorGUIUtility.hotControl == evtHandleId )
			    	{
					    int t = SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - _mouseOffsetFrames;

					    int delta = t-_evt.Start;

						SequenceEditor.MoveEvents( delta );

					    Event.current.Use();

				    }
	                else if( EditorGUIUtility.hotControl == leftHandleId || EditorGUIUtility.hotControl == rightHandleId )
	                {
						int leftLimit = 0;
						int rightLimit = 0;

						bool draggingStart = EditorGUIUtility.hotControl == leftHandleId;

						if( draggingStart )
						{
							leftLimit = validKeyframeRange.Start;
							rightLimit = _evt.End-1;
						}
						else
						{
							leftLimit = _evt.Start+1;
							rightLimit = validKeyframeRange.End;
						}


						int t = SequenceEditor.GetFrameForX( Event.current.mousePosition.x );

						t = Mathf.Clamp( t, leftLimit, rightLimit );

						int delta = t - (draggingStart ? _evt.Start : _evt.End);

						if( draggingStart )
						{
							int newLength = _evt.Length - delta;
							if( newLength < _evt.GetMinLength() )
							{
								delta += newLength - _evt.GetMinLength();
							}
							if( newLength > _evt.GetMaxLength() )
							{
								delta += newLength - _evt.GetMaxLength();
							}
						}
						else
						{
							int newLength = _evt.Length + delta;
							if( newLength < _evt.GetMinLength() )
							{
								delta -= newLength - _evt.GetMinLength();
							}
							if( newLength > _evt.GetMaxLength() )
							{
								delta -= newLength - _evt.GetMaxLength();
							}
						}

						if( delta != 0 )
						{
							if( draggingStart )
								SequenceEditor.ResizeEventsLeft( delta );
							else
								SequenceEditor.ResizeEventsRight( delta );
						}

						Event.current.Use();
					}
				}
				break;
			}
		}

//		public virtual void UpdateEventEditor( int frame, float time )
//		{
//			_evt.UpdateEvent( frame, time );
//		}


		public override FObject GetRuntimeObject()
		{
			return _evt;
		}

		public virtual Color GetColor()
		{
			return FGUI.GetEventColor();
		}
	}
}
