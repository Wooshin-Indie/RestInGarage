using System;
using System.Reflection;
using UnityEditor;

namespace IUtil.Utils
{
#if UNITY_EDITOR
	public static class Extensions
	{
		public static System.Array GetArray(this SerializedProperty property, string fieldName)
		{
			object targetObject = property.serializedObject.targetObject;
			FieldInfo fieldInfo = targetObject.GetType().GetField(
				fieldName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
			);

			if (fieldInfo == null || !fieldInfo.FieldType.IsArray)
			{
				IUtilDebug.NoFieldError("PopupOption", fieldName);
				return null;
			}

			return fieldInfo.GetValue(targetObject) as System.Array;
		}

		public static bool GetBoolean(this SerializedProperty property, string attr, string fieldName)
		{
			object targetObject = property.serializedObject.targetObject;
			FieldInfo fieldInfo = targetObject.GetType().GetField(
				fieldName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
			);

			if (fieldInfo == null)
			{
				IUtilDebug.NoFieldError(attr, fieldName);
				return false;
			}

			if (fieldInfo.FieldType != typeof(bool))
			{
				IUtilDebug.TypeError(attr, fieldInfo.FieldType.ToString());
				return false;
			}

			return Convert.ToBoolean(fieldInfo.GetValue(targetObject));
		}

		public static string GetIconName(this FolderIconType type)
		{
			switch (type)
			{
				case FolderIconType.Script:
					return "cs Script Icon";
				case FolderIconType.Material:
					return "d_Material Icon";
				case FolderIconType.Shader:
					return "d_Shader Icon";
				case FolderIconType.Prefab:
					return "Prefab Icon";
				case FolderIconType.ScriptableObject:
					return "d_ScriptableObject Icon";
				case FolderIconType.Texture:
					return "d_Texture Icon";
				case FolderIconType.Animator:
					return "AnimatorController Icon";
				case FolderIconType.Audio:
					return "AudioClip Icon";
				case FolderIconType.Font:
					return "d_Font Icon";
				case FolderIconType.None:
				default:
					return null;
			}
		}

	}
#endif
}
