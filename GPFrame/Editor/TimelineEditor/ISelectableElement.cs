namespace GPEditor
{
	public interface ISelectableElement
	{
		void OnSelect();
		void OnDeselect();

		bool IsSelected();

//		UnityEngine.Object GetRuntimeObject();

		UnityEngine.Rect GetRect();
	}
}
