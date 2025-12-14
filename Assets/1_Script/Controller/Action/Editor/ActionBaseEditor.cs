using Garage.Actions;
using UnityEditor;

namespace Garage.Editors
{
	[CustomEditor(typeof(ActionBase), true)]
    public class ActionBaseEditor : Editor
    {
		private SerializedProperty isAbleToCancelProp;
		private SerializedProperty actionIARefProp;
		private SerializedProperty cancelIARefProp;
		private SerializedProperty endConditionProp;

		private void OnEnable()
		{
			isAbleToCancelProp = serializedObject.FindProperty("isAbleToCancel");
			actionIARefProp = serializedObject.FindProperty("actionIARef");
			cancelIARefProp = serializedObject.FindProperty("cancelIARef");
			endConditionProp = serializedObject.FindProperty("endCondition");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(isAbleToCancelProp);
			EditorGUILayout.PropertyField(actionIARefProp);

			if (isAbleToCancelProp.boolValue)
			{
				EditorGUILayout.PropertyField(cancelIARefProp);
			}

			EditorGUILayout.PropertyField(endConditionProp);

			serializedObject.ApplyModifiedProperties();
		}
	}
}