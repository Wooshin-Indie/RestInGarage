using System.Collections.Generic;
using UnityEngine;

namespace IUtil.SO
{
	[System.Serializable]
	public class FolderConfigElement
	{
		public string Path = string.Empty;
		public FolderColorType ColorType = FolderColorType.None;
		public FolderIconType IconType = FolderIconType.None;
		public Color CustomFolderColor = Color.white;

		public FolderConfigElement(string path)
		{
			Path = path;
		}
	}

	[CreateAssetMenu(fileName = "FolderConfig", menuName = "IUtil/FolderConfig")]
	public class FolderConfig : ScriptableObject
	{ 
		public List<FolderConfigElement> Elements = new();
	}
}