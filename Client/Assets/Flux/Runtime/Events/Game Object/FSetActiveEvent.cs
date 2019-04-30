using UnityEngine;
using System.Collections;

namespace Flux
{
	[FEvent("Game Object/Set Active")]
	public class FSetActiveEvent : FEvent
	{
		[SerializeField]
		private bool _active = true;

		[SerializeField]
		[Tooltip("Reverse the active flag on the last frame of the event?")]
		private bool _reverseOnFinish = true;

		private bool _wasActive = false;

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			_wasActive = Owner.gameObject.activeSelf;
			Owner.gameObject.SetActive( _active );
		}

		protected override void OnFinish()
		{
			if( _reverseOnFinish )
				Owner.gameObject.SetActive( !_active );
		}

		protected override void OnStop()
		{
			Owner.gameObject.SetActive( _wasActive );
		}
	}
}
