using UnityEngine;
using UnityEditor;

using GP;

namespace GPEditor
{
	public class FSequenceWindowToolbar
	{
		public const int ROW_HEIGHT = 20;
		public const int NUM_ROWS = 1;
		public const int SPACE = 5;
		public const int HEIGHT = ROW_HEIGHT * NUM_ROWS + SPACE*(NUM_ROWS+1);

		public const int BUTTON_WIDTH = 25;
		public const int STOP_BUTTON_WIDTH = 50;
		public const int PLAY_BUTTON_WIDTH = 50;

		public const int FRAME_FIELD_WIDTH = 100;

		private FSequenceEditorWindow _window;

		private Rect _firstFrameButtonRect;
		private Rect _previousFrameButtonRect;
		private Rect _currentFrameFieldRect;
		private Rect _nextFrameButtonRect;
		private Rect _lastFrameButtonRect;

//		private Rect _playBackwardButtonRect;
		private Rect _stopButtonRect;
		private Rect _playForwardButtonRect;

		private GUIContent _firstFrame;
		private GUIContent _previousFrame;
		private GUIContent _nextFrame;
		private GUIContent _lastFrame;
		private GUIContent _playForward;
		private GUIContent _pause;
		private GUIContent _stop;

		private bool _showViewRange;
		private GUIContent _viewRangeLabel;
		private GUIContent _viewRangeDash;

		private Rect _viewRangeLabelRect;
		private Rect _viewRangeDashRect;
		private Rect _viewRangeStartRect;
		private Rect _viewRangeEndRect;

		public FSequenceWindowToolbar( FSequenceEditorWindow window )
		{
			_window = window;


			_firstFrame = new GUIContent( FUtility.GetFluxTexture( "FirstFrame.png" ), "First Frame" );
			_previousFrame = new GUIContent( FUtility.GetFluxTexture( "PreviousFrame.png" ), "Previous Frame" );
			_nextFrame = new GUIContent( FUtility.GetFluxTexture( "NextFrame.png" ), "Next Frame" );
			_lastFrame = new GUIContent( FUtility.GetFluxTexture( "LastFrame.png" ), "Last Frame" );

			_playForward = new GUIContent( FUtility.GetFluxTexture( "Play.png" ), "Play" );
			_pause = new GUIContent( FUtility.GetFluxTexture( "Pause.png" ), "Pause" );
			_stop = new GUIContent( FUtility.GetFluxTexture( "Stop.png" ), "Stop" );

			_viewRangeLabel = new GUIContent( "View Range" );
			_viewRangeDash = new GUIContent( " - " );
		}

		public void RebuildLayout( Rect rect )
		{
			rect.xMin += SPACE;
			rect.yMin += SPACE;
			rect.xMax -= SPACE;
			rect.yMax -= SPACE;

			_firstFrameButtonRect = rect;
			_firstFrameButtonRect.width = BUTTON_WIDTH;

			_previousFrameButtonRect = rect;
			_previousFrameButtonRect.xMin = _firstFrameButtonRect.xMax;
			_previousFrameButtonRect.width = BUTTON_WIDTH;

			_currentFrameFieldRect = rect;
			_currentFrameFieldRect.xMin = _previousFrameButtonRect.xMax;
			_currentFrameFieldRect.width = FRAME_FIELD_WIDTH;

			_nextFrameButtonRect = rect;
			_nextFrameButtonRect.xMin = _currentFrameFieldRect.xMax;
			_nextFrameButtonRect.width = BUTTON_WIDTH;

			_lastFrameButtonRect = rect;
			_lastFrameButtonRect.xMin = _nextFrameButtonRect.xMax;
			_lastFrameButtonRect.width = BUTTON_WIDTH;

//			_playBackwardButtonRect = rect;
//			_playBackwardButtonRect.xMin = _lastFrameButtonRect.xMax + SPACE_BETWEEN_ROWS;
//			_playBackwardButtonRect.width = PLAY_BUTTON_WIDTH;
//
//			_playForwardButtonRect = rect;
//			_playForwardButtonRect.xMin = _playBackwardButtonRect.xMax;
//			_playForwardButtonRect.width = PLAY_BUTTON_WIDTH;

			_stopButtonRect = rect;
			_stopButtonRect.xMin = _lastFrameButtonRect.xMax + SPACE;
			_stopButtonRect.width = STOP_BUTTON_WIDTH;

			_playForwardButtonRect = rect;
			_playForwardButtonRect.xMin = _stopButtonRect.xMax;
			_playForwardButtonRect.width = PLAY_BUTTON_WIDTH;


			float reminderWidth = rect.width - _playForwardButtonRect.xMax + SPACE;

			_viewRangeLabelRect = rect;
			_viewRangeLabelRect.width = EditorStyles.label.CalcSize( _viewRangeLabel ).x + SPACE;

			_viewRangeDashRect = rect;
			_viewRangeDashRect.width = EditorStyles.label.CalcSize( _viewRangeDash ).x;

			_viewRangeStartRect = rect;
			_viewRangeStartRect.width = FRAME_FIELD_WIDTH;
			_viewRangeEndRect = rect;
			_viewRangeEndRect.width = FRAME_FIELD_WIDTH;

			_viewRangeEndRect.x = rect.xMax - _viewRangeEndRect.width;

			_viewRangeDashRect.x = _viewRangeEndRect.xMin - _viewRangeDashRect.width;

			_viewRangeStartRect.x = _viewRangeDashRect.xMin - _viewRangeStartRect.width;

			_viewRangeLabelRect.x = _viewRangeStartRect.xMin - _viewRangeLabelRect.width;

			reminderWidth -= _viewRangeLabelRect.width + _viewRangeStartRect.width + _viewRangeDashRect.width + _viewRangeEndRect.width;

			_showViewRange = reminderWidth >= 0;
		}

		public void OnGUI()
		{
			GTimelineEditor sequence = _window.GetSequenceEditor().GetSequence();
			FrameRange viewRange = _window.GetSequenceEditor().GetViewRange();

			GUI.contentColor = FGUI.GetTextColor();

			bool goToFirst = false;
			bool goToPrevious = false;
			bool goToNext = false;
			bool goToLast = false;

			if( Event.current.type == EventType.KeyDown )
			{
				if( Event.current.keyCode == KeyCode.Comma )
				{
					goToFirst = Event.current.shift;
					goToPrevious = !goToFirst;
					Event.current.Use();
					_window.Repaint();
				}

				if( Event.current.keyCode == KeyCode.Period )
				{
					goToLast = Event.current.shift;
					goToNext = !goToLast;
					Event.current.Use();
					_window.Repaint();
				}
			}

			if( GUI.Button( _firstFrameButtonRect, _firstFrame, EditorStyles.miniButtonLeft ) || goToFirst )
				GoToFrame( viewRange.Start );

			if( GUI.Button( _previousFrameButtonRect, _previousFrame, EditorStyles.miniButtonMid) || goToPrevious )
				GoToFrame( viewRange.Cull(sequence.GetCurrentFrame()-1) );

			GUIStyle numberFieldStyle = new GUIStyle(EditorStyles.numberField);
			numberFieldStyle.alignment = TextAnchor.MiddleCenter;

//			Debug.Log("hot control: " + EditorGUIUtility.hotControl + " keyboard control: " + EditorGUIUtility.keyboardControl );

			if( sequence != null )
			{
				if( sequence.GetCurrentFrame() < 0 )
				{
					EditorGUI.BeginChangeCheck();
					string frameStr = EditorGUI.TextField( _currentFrameFieldRect, string.Empty, numberFieldStyle );
					if( EditorGUI.EndChangeCheck() )
					{
						int newCurrentFrame = 0;
						int.TryParse( frameStr, out newCurrentFrame );
						newCurrentFrame = Mathf.Clamp( newCurrentFrame, 0, sequence.Length );
						_window.GetSequenceEditor().SetCurrentFrame( newCurrentFrame );
					}
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					int newCurrentFrame = Mathf.Clamp( EditorGUI.IntField( _currentFrameFieldRect, sequence.GetCurrentFrame(), numberFieldStyle ), 0, sequence.Length );
					if( EditorGUI.EndChangeCheck() )
						_window.GetSequenceEditor().SetCurrentFrame( newCurrentFrame );
				}
			}

			if( GUI.Button( _nextFrameButtonRect, _nextFrame, EditorStyles.miniButtonMid ) || goToNext )
				GoToFrame( viewRange.Cull( sequence.GetCurrentFrame()+1 ) );

			if( GUI.Button( _lastFrameButtonRect, _lastFrame, EditorStyles.miniButtonRight) || goToLast )
				GoToFrame( viewRange.End );

//			if( GUI.Button( _playBackwardButtonRect, _window.IsPlaying ? "[] Stop" : "< Play", EditorStyles.miniButtonLeft) )
//			{
//				if( _window.IsPlaying )
//				{
//					_window.Stop();
//				}
//				else
//				{
//					_window.Play();
//				}
//			}

			if( GUI.Button( _stopButtonRect, _stop, EditorStyles.miniButtonLeft ) )
				Stop();

			if( GUI.Button( _playForwardButtonRect, _window.IsPlaying ? _pause : _playForward, EditorStyles.miniButtonRight) )
			{
				if( _window.IsPlaying )
					Pause();
				else
					Play();
			}


			if( _showViewRange )
			{
				EditorGUI.PrefixLabel( _viewRangeLabelRect, _viewRangeLabel );

				EditorGUI.BeginChangeCheck();

				viewRange.Start = EditorGUI.IntField( _viewRangeStartRect, viewRange.Start, numberFieldStyle );

				EditorGUI.PrefixLabel( _viewRangeDashRect, _viewRangeDash );

				viewRange.End = EditorGUI.IntField( _viewRangeEndRect, viewRange.End, numberFieldStyle );

				if( EditorGUI.EndChangeCheck() )
					_window.GetSequenceEditor().SetViewRange( viewRange );
			}
		}

		public void GoToFrame( int frame )
		{
			_window.GetSequenceEditor().SetCurrentFrame( frame );
			if( _window.IsPlaying )
				_window.Pause();
			FUtility.RepaintGameView();
		}

		public void Stop()
		{
			_window.Stop();
		}

		public void Play()
		{
			_window.Play( Event.current.shift );
		}

		public void Pause()
		{
			_window.Pause();
		}

//		public static FrameRange FrameRangeField( Rect rect, FrameRange range )
//		{
//			float minWidthNumberField = 100;
//			
//			GUIContent label = new GUIContent( "View Range" );
//			GUIContent dash = new GUIContent( " - " );
//			
//			Vector2 labelSize = EditorStyles.label.CalcSize( label );
//			Vector2 dashSize = EditorStyles.label.CalcSize( dash );
//			
//			float minSize = minWidthNumberField + minWidthNumberField + dashSize.x;
//			
//			float extraSize = rect.width - minSize;
//			
//			labelSize.x = extraSize;
//			
//			Rect labelRect = rect;
//			labelRect.width = labelSize.x;
//			
//			EditorGUI.PrefixLabel( labelRect, label );
//			
//			rect.xMin = labelRect.xMax;
//			
//			Rect startRect = rect;
//			startRect.width = rect.width * 0.5f - dashSize.x * 0.5f;
//			
//			GUIStyle numberStyle = new GUIStyle( EditorStyles.numberField );
//			numberStyle.alignment = TextAnchor.MiddleCenter;
//			
//			EditorGUI.BeginChangeCheck();
//			range.start = EditorGUI.IntField( startRect, range.start, numberStyle );
//			if( EditorGUI.EndChangeCheck() )
//				range.start = Mathf.Min( range.start, range.end );
//			
//			
//			Rect dashRect = startRect;
//			dashRect.x = startRect.xMax;
//			dashRect.width = dashSize.x;
//			
//			GUI.Label( dashRect, dash );
//			
//			Rect endRect = startRect;
//			endRect.x = dashRect.xMax;
//			EditorGUI.BeginChangeCheck();
//			range.end = EditorGUI.IntField( endRect, range.end, numberStyle );
//			if( EditorGUI.EndChangeCheck() )
//				range.end = Mathf.Max( range.start, range.end );
//			
//			return range;
//		}
	}
}