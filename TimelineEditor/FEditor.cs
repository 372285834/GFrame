using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

using GP;

namespace GPEditor
{
	/**
	 * @brief Base for all the editor classes used in the sequence window.
	 */
	public abstract class FEditor : ScriptableObject, ISelectableElement
	{
		/** @brief Reference to the sequence editor this editor belongs to */
		public abstract GTimelineEditor SequenceEditor{ get; }

		/** @brief Is this element selected? */
		[SerializeField]
		protected bool _isSelected;

		/** @brief What's the rect used to draw this element? */
		protected Rect _rect;

		/** @brief Called on selection. */
		public virtual void OnSelect()
		{
			_isSelected = true;
		}

		/** @brief Called on deselection. */
		public virtual void OnDeselect()
		{
			_isSelected = false;
		}

		/** @brief Is this element selected? */
		public bool IsSelected()
		{
			return _isSelected;
		}

		protected virtual void OnEnable()
		{
			hideFlags = HideFlags.DontSave;
		}

		protected virtual void OnDestroy()
		{

		}

		public abstract void RefreshRuntimeObject();

		/** @brief Inits the editor object.
		 * @param obj CObject the editor manages
		 */
		protected abstract void Init( FObject obj );

		/** @brief What is the real object that is used at runtime? */
		public abstract FObject GetRuntimeObject();

		/** @brief Returns the area where this element is drawn. */
		public UnityEngine.Rect GetRect(){ return _rect; }
	}

	/**
	 * @brief Attribute to specify which editor will handle the representation
	 * of a specific FObject class. It works in the same way as Unity's
	 * CustomEditor, but here it is for specifying how that FObject will be 
	 * represented inside sequence window.
	 */
	public class FEditorAttribute : Attribute
	{
		public Type type;
		
		public FEditorAttribute( Type type )
		{
			this.type = type;
		}
	}
}