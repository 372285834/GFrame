using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using System.IO;
using System.Reflection;

using Flux;

namespace GPEditor
{

	public static class FUtility
	{
		#region Preferences
		private static bool _preferencesLoaded = false;

		// keys
		private const string FLUX_KEY = "Flux.";

		private const string TIME_FORMAT_KEY = FLUX_KEY + "TimeFormat";
		private const string FRAME_RATE_KEY = FLUX_KEY + "FrameRate";
		private const string OPEN_SEQUENCE_ON_SELECT_KEY = FLUX_KEY + "OpenSequenceOnSelect";

		// defaults
		private const TimeFormat DEFAULT_TIME_FORMAT = TimeFormat.Frames;
		private const int DEFAULT_FRAME_RATE = FSequence.DEFAULT_FRAMES_PER_SECOND;
		private const bool DEFAULT_OPEN_SEQUENCE_ON_SELECT = false;

		// current values
		private static TimeFormat _timeFormat = DEFAULT_TIME_FORMAT;
		public static TimeFormat TimeFormat { get { return _timeFormat; } }
		private static int _frameRate = DEFAULT_FRAME_RATE;
		public static int FrameRate { get { return _frameRate; } }
		private static bool _openSequenceOnSelect = DEFAULT_OPEN_SEQUENCE_ON_SELECT;
		public static bool OpenSequenceOnSelect { get { return _openSequenceOnSelect; } }

		[PreferenceItem("Flux Editor")]
		public static void PreferencesGUI()
		{
			if( !_preferencesLoaded )
				LoadPreferences();

			EditorGUI.BeginChangeCheck();

			_timeFormat = (TimeFormat)EditorGUILayout.EnumPopup( "Time Format", _timeFormat );
			_frameRate = EditorGUILayout.IntSlider( "Frame Rate", _frameRate, 1, 120 );
			_openSequenceOnSelect = EditorGUILayout.Toggle( "Open Sequence On Select", _openSequenceOnSelect );

			if( EditorGUI.EndChangeCheck() )
				SavePreferences();
		}
		
		public static void LoadPreferences()
		{
			_preferencesLoaded = true;

			_timeFormat = (TimeFormat)EditorPrefs.GetInt( TIME_FORMAT_KEY, (int)DEFAULT_TIME_FORMAT );
			_frameRate = EditorPrefs.GetInt( FRAME_RATE_KEY, DEFAULT_FRAME_RATE );
			_openSequenceOnSelect = EditorPrefs.GetBool( OPEN_SEQUENCE_ON_SELECT_KEY, DEFAULT_OPEN_SEQUENCE_ON_SELECT );
		}

		public static void SavePreferences()
		{
			EditorPrefs.SetInt( TIME_FORMAT_KEY, (int)_timeFormat );
			EditorPrefs.SetInt( FRAME_RATE_KEY, _frameRate );
			EditorPrefs.SetBool( OPEN_SEQUENCE_ON_SELECT_KEY, _openSequenceOnSelect );
		}

		#endregion Preferences

		#region Paths / Resource Loading

		private static string _fluxPath = null;
		private static string _fluxEditorPath = null;
		private static string _fluxSkinPath = null;

		private static EditorWindow _gameView = null;

		public static string FindFluxDirectory()
		{
			string[] directories = Directory.GetDirectories("Assets", "Flux", SearchOption.AllDirectories);
			return directories.Length > 0 ? directories[0] : string.Empty;
		}

		
		public static string GetFluxPath()
		{
			if( _fluxPath == null )
			{
				_fluxPath = FindFluxDirectory()+'/';
				_fluxEditorPath = _fluxPath + "Editor/";
				_fluxSkinPath = _fluxEditorPath + "Skin/";
			}
			return _fluxPath;
		}
		
		public static string GetFluxEditorPath()
		{
			if( _fluxEditorPath == null ) GetFluxPath();
			return _fluxEditorPath;
		}
		
		public static string GetFluxSkinPath()
		{
			if( _fluxSkinPath == null ) GetFluxPath();
			return _fluxSkinPath;
		}

		public static GUISkin GetFluxSkin()
		{
			return (GUISkin)AssetDatabase.LoadAssetAtPath( GetFluxSkinPath()+"FSkin.guiskin", typeof(GUISkin) );
		}

		public static Texture2D GetFluxTexture( string textureFile )
		{
			return (Texture2D)AssetDatabase.LoadAssetAtPath( GetFluxSkinPath() + textureFile, typeof(Texture2D) );
		}

		#endregion Paths / Resource Loading

		#region Events

		public static void Resize( FEvent evt, FrameRange newFrameRange )
		{
			if( evt.FrameRange == newFrameRange || newFrameRange.Start > newFrameRange.End )
				return;
			
			if( newFrameRange.Length < evt.GetMinLength() || newFrameRange.Length > evt.GetMaxLength() )
			{
				Debug.LogError( string.Format("Trying to resize an Event to [{0},{1}] (length: {2}) which isn't a valid length, should be between [{3},{4}]", newFrameRange.Start, newFrameRange.End, newFrameRange.Length, evt.GetMinLength(), evt.GetMaxLength() ), evt );
				return;
			}
			
			bool changedLength = evt.Length != newFrameRange.Length;

			if( !evt.GetTrack().CanAdd( evt, newFrameRange ) )
				return;

			Undo.RecordObject( evt, changedLength ? "Resize Event" : "Move Event" );
			
			evt.Start = newFrameRange.Start;
			evt.End = newFrameRange.End;
			
			if( changedLength )
			{
				if( evt is FPlayAnimationEvent )
				{
					FPlayAnimationEvent animEvt = (FPlayAnimationEvent)evt;
					
					if( animEvt.IsAnimationEditable() )
					{
						FAnimationEventInspector.ScaleAnimationClip( animEvt._animationClip, animEvt.FrameRange );
					}
				}
				else if( evt is FTimescaleEvent )
				{
					ResizeAnimationCurve( (FTimescaleEvent)evt, newFrameRange );
				}
			}
			
			EditorUtility.SetDirty( evt );
			
			if( FSequenceEditorWindow.instance != null )
				FSequenceEditorWindow.instance.Repaint();
		}
		
		public static void Rescale( FEvent evt, FrameRange newFrameRange )
		{
			if( evt.FrameRange == newFrameRange )
				return;
			
			Undo.RecordObject( evt, string.Empty );
			
			evt.Start = newFrameRange.Start;
			evt.End = newFrameRange.End;
			
			if( evt is FPlayAnimationEvent )
			{
				FPlayAnimationEvent animEvt = (FPlayAnimationEvent)evt;
				
				if( animEvt._animationClip != null )
				{
					if( animEvt.IsAnimationEditable() )
					{
						animEvt._animationClip.frameRate = animEvt.Sequence.FrameRate;
						EditorUtility.SetDirty( animEvt._animationClip );
					}
					else if( Mathf.RoundToInt(animEvt._animationClip.frameRate) != animEvt.Sequence.FrameRate )
					{
						Debug.LogError( string.Format("Removed AnimationClip '{0}' ({1} fps) from Animation Event '{2}'", 
						                              animEvt._animationClip.name, 
						                              animEvt._animationClip.frameRate, 
						                              animEvt.name ), animEvt );
						
						animEvt._animationClip = null;
					}
				}
			}
			
			EditorUtility.SetDirty( evt );
		}

		public static void ResizeAnimationCurve( FTimescaleEvent evt, FrameRange frameRange )
		{
			AnimationCurve curve = evt.Curve;
			float oldLength = curve.length == 0 ? 0 : curve.keys[curve.length-1].time;
			float newLength = evt.LengthTime;

			if( oldLength == 0 )
			{
				// handle no curve
				curve.AddKey(0, 1);
				curve.AddKey(newLength, 1);
				return;
			}

			float ratio = newLength / oldLength;

			int start = 0;
			int limit = curve.length;
			int increment = 1;

			if( ratio > 1 )
			{
				start = limit - 1;
				limit = -1;
				increment = -1;
			}

			for( int i = start; i != limit; i += increment )
			{
				Keyframe newKeyframe = new Keyframe( curve.keys[i].time * ratio, curve.keys[i].value, curve.keys[i].inTangent*(1/ratio), curve.keys[i].outTangent*(1/ratio) );
				newKeyframe.tangentMode = curve.keys[i].tangentMode;
				curve.MoveKey(i, newKeyframe );
			}
		}

		#endregion Events


		public static void RepaintGameView()
		{
			if( _gameView == null )
				_gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType( "UnityEditor.GameView" ));

			_gameView.Repaint();
			SceneView.RepaintAll();
//			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}

		public static string GetTime( int frame, int frameRate )
		{
			switch( _timeFormat )
			{
			case TimeFormat.Frames:
				return frame.ToString();
			case TimeFormat.Seconds:
				return ((float)frame / frameRate).ToString("0.##");
			case TimeFormat.SecondsFormatted:
				return string.Format( "{0}:{1}", frame / frameRate, frame % frameRate );
			}

			return frame.ToString();
		}

		#region Audio Utilities

		public static Texture2D GetAudioClipTexture( AudioClip clip, float width, float height )
		{
			if( clip == null )
				return null;

			AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath( AssetDatabase.GetAssetPath( clip ) );

			MethodInfo getWaveForm = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("GetWaveForm", 
			                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod );

			Texture2D[] waveForms = new Texture2D[clip.channels]; 
			for( int channel = 0; channel < clip.channels; ++channel )
				waveForms[channel] = (Texture2D)getWaveForm.Invoke( null, new object[]{clip, importer, channel, width, height / clip.channels});
//				waveForms[channel] = AudioUtil.GetWaveForm(clip, importer, channel, width, height / clip.channels);
			return CombineWaveForms(waveForms);
		}

		public static Texture2D CombineWaveForms( Texture2D[] waveForms )
		{
			if (waveForms.Length == 1) return waveForms[0];
			int width = waveForms[0].width;
			int height = 0;
			foreach (Texture2D texture2D in waveForms)
				height += texture2D.height;
			Texture2D texture2D1 = new Texture2D(width, height, TextureFormat.ARGB32, false); int num = 0;
			foreach (Texture2D texture2D2 in waveForms)
			{
				num += texture2D2.height;
				texture2D1.SetPixels(0, height - num, width, texture2D2.height, texture2D2.GetPixels()); UnityEngine.Object.DestroyImmediate((UnityEngine.Object) texture2D2);
			} texture2D1.Apply(); return texture2D1;
		}

		#endregion
	}
}
