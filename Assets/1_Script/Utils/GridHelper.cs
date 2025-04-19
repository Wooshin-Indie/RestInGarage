using UnityEngine;

namespace Garage.Utils
{
	public static class GridHelper
	{
		public static GridTile[,] ToDictionaryByPosition(this GridTile[] tiles, Vector2Int size, Vector2Int originPos)
		{
			var result = new GridTile[size.x, size.y];

			foreach (var tile in tiles)
			{
				Vector2Int pos = new Vector2Int(
					Mathf.RoundToInt(tile.transform.position.x + .5f),
					Mathf.RoundToInt(tile.transform.position.z + .5f)
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
}