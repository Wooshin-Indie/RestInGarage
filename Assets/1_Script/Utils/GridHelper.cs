using Garage;
using Garage.Manager;
using UnityEngine;

public static class GridHelper
{
	public static GridTile[,] ToDictionaryByPosition(this GridTile[] tiles, Vector2Int size, Vector2Int originPos)
	{
		var result = new GridTile[size.x, size.y];

		foreach (var tile in tiles)
		{
			Debug.Log(tile.gameObject.name);
			Vector2Int pos = new Vector2Int(
				Mathf.RoundToInt(tile.transform.position.x),
				Mathf.RoundToInt(tile.transform.position.z)
			);

			Vector2Int index = pos - originPos;
			if (index.x >= 0 && index.x < size.x && index.y >= 0 && index.y < size.y)
			{
				result[index.x, index.y] = tile;
			}
		}

		return result;
	}
}