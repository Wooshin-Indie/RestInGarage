using Garage.Actions;
using UnityEditor;

namespace Garage.Editors
{
	[CustomEditor(typeof(ActionBase), true)]
    public class ActionBaseEditor : Editor
    {
		private SerializedProperty isAbleToCancelProp;
		private SerializedProperty actionIANameProp;
		private SerializedProperty cancelIANameProp;
		private SerializedProperty endConditionProp;

		private void OnEnable()
		{
			isAbleToCancelProp = serializedObject.FindProperty("isAbleToCancel");
			actionIANameProp = serializedObject.FindProperty("actionIAName");
			cancelIANameProp = serializedObject.FindProperty("cancelIAName");
			endConditionProp = serializedObject.FindProperty("endCondition");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(isAbleToCancelProp);
			EditorGUILayout.PropertyField(actionIANameProp);

			if (isAbleToCancelProp.boolValue)
			{
				EditorGUILayout.PropertyField(cancelIANameProp);
			}

			EditorGUILayout.PropertyField(endConditionProp);

			serializedObject.ApplyModifiedProperties();
		}
	}
}