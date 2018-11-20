using UnityEngine;

namespace Flux
{
	[FEvent("Transform/Anchor")]
	public class FAnchorEvent : FEvent
	{
		[SerializeField]
		private Transform _anchor;
		public Transform Anchor { get { return _anchor; } set { _anchor = value; } }

		[SerializeField]
		private bool _parentToAnchor = false;
		private bool ParentToAnchor { get { return _parentToAnchor; } set { _parentToAnchor = value; } }

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			Owner.parent = _anchor;
			Owner.position = _anchor.position;
			Owner.rotation = _anchor.rotation;

			if( _parentToAnchor )
				Owner.parent = _anchor;
		}
	}
}
