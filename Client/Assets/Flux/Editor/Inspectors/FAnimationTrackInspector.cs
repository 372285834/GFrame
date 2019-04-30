using UnityEngine;
using UnityEditor;

using UnityEditorInternal;

using System.Collections.Generic;

using Flux;
using FluxEditor;

namespace FluxEditor
{
	[CustomEditor(typeof(FAnimationTrack))]
	public class FAnimationTrackInspector : FTrackInspector {

		private const string ADVANCE_TRIGGER = "FAdvanceTrigger";

		private FAnimationTrack _animTrack = null;
		
		private SerializedProperty _animatorController = null;
		
		public override void OnEnable()
		{
			base.OnEnable();

			_animTrack = (FAnimationTrack)target;
			_animatorController = serializedObject.FindProperty("_animatorController");
		}
		
		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.Update();

			if( _animatorController.objectReferenceValue == null )
			{
				EditorGUILayout.HelpBox("There's no Animator Controller", MessageType.Warning);
				Rect helpBoxRect = GUILayoutUtility.GetLastRect();

				float yCenter = helpBoxRect.center.y;

				helpBoxRect.xMax -= 3;
				helpBoxRect.xMin = helpBoxRect.xMax - 50;
				helpBoxRect.yMin = yCenter - 12.5f;
				helpBoxRect.height = 25f;

				if( GUI.Button( helpBoxRect, "Create" ) )
				{
					UnityEditor.Animations.AnimatorController newController = CreateAnimatorController( _animTrack );
					if( newController )
						_animatorController.objectReferenceValue = newController;
				}
			}

			UnityEditor.Animations.AnimatorController prevAnimatorController = (UnityEditor.Animations.AnimatorController)_animatorController.objectReferenceValue;

			EditorGUILayout.PropertyField( _animatorController );

			if( _animatorController.objectReferenceValue != prevAnimatorController )
			{
                if( !VerifyOverrideAnimatorController( (UnityEditor.Animations.AnimatorController)_animatorController.objectReferenceValue ) )
                    _animatorController.objectReferenceValue = prevAnimatorController;
			}

			serializedObject.ApplyModifiedProperties();
		}

		public static void RebuildStateMachine( FAnimationTrack track )
		{
            /*
			if( track.GetAnimatorController() == null )
				return;

			UnityEditor.Animations.AnimatorController controller = (UnityEditor.Animations.AnimatorController)track.GetAnimatorController();

			track.UpdateEventIds();

			//if( controller.FindParameter( ADVANCE_TRIGGER ) < 0 )
                controller.AddParameter(ADVANCE_TRIGGER, UnityEngine.AnimatorControllerParameterType.Trigger);

            UnityEditor.Animations.AnimatorControllerLayer baseLayer = controller.layers[0];
			
			UnityEditor.Animations.AnimatorStateMachine baseSM = baseLayer.stateMachine;

			List<FEvent> events = track.GetEvents();

            if (baseSM.states.Length > events.Count)
			{
                for (int i = events.Count; i < baseSM.states.Length; ++i)
					baseSM.RemoveState( baseSM.states[i].state );
			}
            else if (baseSM.states.Length < events.Count)
			{
                for (int i = baseSM.states.Length; i < events.Count; ++i)
					baseSM.AddState( i.ToString() );
			}

			UnityEditor.Animations.AnimatorState lastState = null;
			Vector3 pos = baseSM.anyStatePosition + new Vector3( 300, 0, 0 );
			
			Vector3 statePosDelta = new Vector3(0, 80, 0 );

			for( int i = 0; i != events.Count; ++i )
			{
				FPlayAnimationEvent animEvent = (FPlayAnimationEvent)events[i];

				if( animEvent._animationClip == null ) // dump events without animations
					continue;

                UnityEditor.Animations.ChildAnimatorState child = baseSM.states[i];
                UnityEditor.Animations.ChildAnimatorStateMachine childMachine = baseSM.stateMachines[i];
                UnityEditor.Animations.AnimatorState state = child.state;
				state.name = i.ToString();

                child.position = pos;
				
				pos += statePosDelta;
				
				state.SetAnimationClip( animEvent._animationClip );
				
				animEvent._stateHash = Animator.StringToHash( state.name );
				
				if( lastState )
				{
                    UnityEditor.Animations.AnimatorTransition[] transitionsLastState = baseSM.GetStateMachineTransitions(childMachine.stateMachine);
					
					UnityEditor.Animations.AnimatorTransition transition = null;
					
					if( transitionsLastState.Length == 1 )
					{
						if( transitionsLastState[0].destinationState != state )
						{
							baseSM.RemoveTransition( transitionsLastState[0] );
							transition = baseSM.AddTransition( lastState, state );
							transition.offset = (animEvent._startOffset / animEvent._animationClip.frameRate) / animEvent._animationClip.length;
						}
						else
							transition = transitionsLastState[0];
					}
					else
					{
						if( transitionsLastState.Length > 0 )
						{
							for( int j = 0; j != transitionsLastState.Length; ++j )
								baseSM.RemoveTransition( transitionsLastState[j] );
						}
						transition = baseSM.AddTransition( lastState, state );
						transition.offset = (animEvent._startOffset / animEvent._animationClip.frameRate) / animEvent._animationClip.length;
					}

					if( animEvent.IsBlending() )
					{
						FPlayAnimationEvent prevAnimEvent = (FPlayAnimationEvent)events[i-1];

						animEvent._startOffset = Mathf.Clamp( Mathf.RoundToInt(transition.offset * animEvent._animationClip.length * animEvent._animationClip.frameRate), 0, animEvent._animationClip.isLooping ? animEvent.Length : Mathf.RoundToInt( animEvent._animationClip.length * animEvent._animationClip.frameRate ) - animEvent.Length );
						transition.offset = (animEvent._startOffset / animEvent._animationClip.frameRate) / animEvent._animationClip.length;
						EditorUtility.SetDirty( animEvent );

						transition.duration = (animEvent._blendLength / prevAnimEvent._animationClip.frameRate) / prevAnimEvent._animationClip.length;
						
						AnimatorCondition condition = transition.GetCondition(0);
						
						condition.mode = TransitionConditionMode.ExitTime;
						condition.exitTime = (prevAnimEvent.Length + prevAnimEvent._startOffset) / (prevAnimEvent._animationClip.length*prevAnimEvent._animationClip.frameRate);
						condition.threshold = 0;
					}
					else // animations not blending, needs hack for animation previewing
					{
						animEvent._startOffset = Mathf.RoundToInt(transition.offset * animEvent._animationClip.length * animEvent._animationClip.frameRate);
						EditorUtility.SetDirty( animEvent );

						transition.duration = 0;
						
						AnimatorCondition condition = transition.GetCondition(0);

						condition.parameter = ADVANCE_TRIGGER;
						condition.mode = TransitionConditionMode.If;

						condition.threshold = 0;
					}
				}
				
				lastState = state;
			}
             * */
		}
		
		public static UnityEditor.Animations.AnimatorTransition GetTransitionTo( FPlayAnimationEvent animEvt )
		{
            /*
			FAnimationTrack animTrack = (FAnimationTrack)animEvt.GetTrack();

			if( animTrack.GetAnimatorController() == null )
				return null;

			UnityEditor.Animations.AnimatorController controller = (UnityEditor.Animations.AnimatorController)animTrack.GetAnimatorController();

			UnityEditor.Animations.AnimatorState animState = null;
			
			UnityEditor.Animations.AnimatorStateMachine stateMachine = controller.GetLayer(0).stateMachine;
			
			for( int i = 0; i != stateMachine.stateCount; ++i )
			{
				if( stateMachine.GetState(i).uniqueNameHash == animEvt._stateHash )
				{
					animState = stateMachine.GetState(i);
					break;
				}
			}
			
			if( animState == null )
			{
//				Debug.LogError("Couldn't find state " + animEvt._animationClip );
				return null;
			}
			
			for( int i = 0; i != stateMachine.stateCount; ++i )
			{
				UnityEditor.Animations.AnimatorState state = stateMachine.GetState(i);
				UnityEditor.Animations.AnimatorTransition[] transitions = stateMachine.GetTransitionsFromState( state );
				for( int j = 0; j != transitions.Length; ++j )
				{
					if( transitions[j].dstState == animState )
					{
						return transitions[j];
					}
				}
			}
			*/
			return null;
		}

		public static UnityEditor.Animations.AnimatorController CreateAnimatorController( FAnimationTrack animTrack )
		{
			string defaultFolder = "Assets/";
			string defaultFileName = string.Format( "{0}_{1}.controller", animTrack.GetTimeline().Sequence.name, animTrack.Owner.name );
			string defaultFilePath = AssetDatabase.GenerateUniqueAssetPath( defaultFolder+defaultFileName );
			defaultFileName = System.IO.Path.GetFileNameWithoutExtension( defaultFilePath );
			
			string filePath = EditorUtility.SaveFilePanel( "Create new AnimatorController...", defaultFolder, defaultFileName, "controller" );
			
			if( string.IsNullOrEmpty( filePath ) )
				return null;
			
			// transform the path into a local path
			if( !filePath.StartsWith( Application.dataPath ) )
			{
				Debug.LogError("Cannot save controller outside of the project");
				return null;
			}
			
			string fileLocalPath = "Assets"+filePath.Substring(Application.dataPath.Length, filePath.Length-Application.dataPath.Length);
			
			UnityEditor.Animations.AnimatorController animatorAtPath = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath( fileLocalPath, typeof(UnityEditor.Animations.AnimatorController) );
			if( animatorAtPath != null )
			{
				if( !VerifyOverrideAnimatorController(animatorAtPath) )
					return null;
			}
			
			return UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath( fileLocalPath );
		}
		
		private static bool VerifyOverrideAnimatorController( UnityEditor.Animations.AnimatorController controller )
		{
            /*
            if( controller == null )
                return true;
			for( int i = 0; i != controller.layerCount; ++i )
			{
				UnityEditor.Animations.AnimatorStateMachine layerStateMachine = controller.GetLayer(i).stateMachine;
				if( layerStateMachine.stateCount > 0 || layerStateMachine.stateMachineCount > 0 )
				{
					return EditorUtility.DisplayDialog( "Override Animator Controller's Content?", 
					                                   "This Animator Controller is not empty, overriding will clear all it's content.\n"
					                                   + " Are you sure you want to override it?", "Override", "Cancel" );
				}
			}
			*/
			return true;
		}
	}
}
