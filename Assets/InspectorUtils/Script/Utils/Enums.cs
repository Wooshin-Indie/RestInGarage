
namespace IUtil
{
	[System.Serializable]
	public enum ColorType
	{
		White,
		Red,
		Green,
		Blue,
		Yellow,
		Magenta,
		Cyan,
		Gray,
		Black
	}

	public enum MessageType
	{
		None,
		Info,
		Warning,
		Error
	}

	/// <summary>
	/// Types for Folder's Color (12 Colors)
	/// </summary>
	public enum FolderColorType
	{
		None = -1,
		Red = 0,
		Orange,
		Yellow,
		Chartreuse,
		Green,
		SpringGreen,
		Cyan,
		DodgerBlue,
		Blue,
		Purple,
		Black,
		Custom
	}

	/// <summary>
	/// Types for additional icon (revealed in bottom-left of folder)
	/// </summary>
	public enum FolderIconType
	{
		None = -1,
		Script = 0,
		Material,
		Shader,
		Prefab,
		ScriptableObject,
		Texture,
		Animator,
		Audio,
		Font
	}
}