using UnityEngine;
using System.Collections;

namespace Flux
{
	[FEvent("Animation/Play Animation", typeof(FAnimationTrack))]
	public class FPlayAnimationEvent : FEvent
	{
		private const string ADVANCE_TRIGGER = "FAdvanceTrigger";

		[HideInInspector]
		public AnimationClip _animationClip = null;

		[HideInInspector]
		public int _blendLength;

		[HideInInspector]
		public int _startOffset;

		[HideInInspector]
		public int _stateHash;

		private Animator _animator = null;

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			_animator = Owner.GetComponent<Animator>();

			FAnimationTrack animationTrack = (FAnimationTrack)_track;

			if( _animator.runtimeAnimatorController != animationTrack.GetAnimatorController() )
			{
				_animator.runtimeAnimatorController = animationTrack.GetAnimatorController();
			}

			_animator.enabled = _animationClip != null;

			int id = GetId();

			if( _animator.enabled )
			{
				if( id == 0 )
                	_animator.Play( _stateHash, 0, _startOffset * Sequence.InverseFrameRate );
				else if( _track.GetEvents()[id-1].End < Start )
					_animator.SetTrigger( ADVANCE_TRIGGER );
				else
					_animator.ResetTrigger( ADVANCE_TRIGGER );

				if( framesSinceTrigger > 0 )
				{
					// - 0.001f because if you pass the length of the animation
					// it seems that it will go over it and miss the condition
					_animator.Update( timeSinceTrigger-0.001f );
				}
			}
		}

		protected override void OnFinish ()
		{
			if( _animator && (IsLastEvent() || _track.GetEvent( GetId()+1 ).Start != End) )
			{
                _animator.enabled = false;
			}
		}

		protected override void OnStop ()
		{
			int id = GetId();
			if( _animator && (id == 0 || _track.GetEvent( id - 1 ).End != Start ) )
			{
				_animator.enabled = false;
			}
		}

		protected override void OnPause ()
		{
			_animator.enabled = false;
		}

		protected override void OnResume()
		{
			_animator.enabled = true;
		}

		public override int GetMaxLength ()
		{
#if UNITY_EDITOR
			if( IsAnimationEditable() || _animationClip.isLooping )
#else
			if( IsAnimationEditable() )
#endif
				return base.GetMaxLength();

			return Mathf.RoundToInt(_animationClip.length * _animationClip.frameRate - _startOffset);
		}

		public bool IsBlending()
		{
			int id = GetId();
			return id > 0 && _track != null && _track.GetEvents()[id-1].End == Start && ((FAnimationTrack)_track).GetAnimatorController() != null && ((FPlayAnimationEvent)_track.GetEvents()[id-1])._animationClip != null;
		}

		public bool IsAnimationEditable()
		{
			return _animationClip == null || ((_animationClip.hideFlags & HideFlags.NotEditable) == 0) ;
		}

		public int GetMaxStartOffset()
		{
			if( _animationClip == null )
				return 0;
#if UNITY_EDITOR
			return _animationClip.isLooping ? Length : Mathf.RoundToInt(_animationClip.length * _animationClip.frameRate) - Length;
#else
			return Length;
#endif
		}
	}
}
