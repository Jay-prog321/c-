using UnityEngine;
public class GameTile : MonoBehaviour
{
	[SerializeField]
	Transform arrow = default;
	GameTile north, east, south, west, nextOnPath;
	int distance;
	public Vector3 ExitPoint { get; private set; }
	public Direction PathDirection { get; private set; }
	public static void MakeEastWestNeighbors(GameTile east, GameTile west)
	{
		Debug.Assert(west.east == null && east.west == null, "Redefined neighbors!");
		west.east = east;
		east.west = west;
	}
	public static void MakeNorthSouthNeighbors(GameTile north, GameTile south)
	{
		Debug.Assert(
			south.north == null && north.south == null, "Redefined neighbors!"
		);
		south.north = north;
		north.south = south;
	}
	/// <summary>
	/// 初始化：path没被找到前，所有地砖的distance为max，即所有地砖没有path，bool hasPath为false
	/// 当有Destination的时候，path可以被找到
	/// </summary>
	public void ClearPath()
	{
		distance = int.MaxValue;
		nextOnPath = null;
	}
	/// <summary>
	/// 设置此地砖为重点，距离终点距离为0，下一个地砖为空
	/// </summary>
	public void BecomeDestination()
	{
		distance = 0;
		nextOnPath = null;
		ExitPoint = transform.localPosition;
	}
	public bool HasPath => distance != int.MaxValue;
	/// <summary>
	/// 给邻居的地砖传distance的值
	/// </summary>
	/// <param name="neighbor"></param>
	/// <returns></returns>
	GameTile GrowPathTo(GameTile neighbor,Direction direction)
	{
		//Debug.Assert(HasPath, "No path!");
		if (!HasPath || neighbor == null || neighbor.HasPath)
		{
			return null;
		}
		neighbor.distance = distance + 1;
		neighbor.nextOnPath = this;
		neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();//保证enemy在边缘移动
		neighbor.PathDirection = direction;
		return
		neighbor.Content.BlocksPath ? null : neighbor;
	}
	public GameTile GrowPathNorth() => GrowPathTo(north,Direction.South);

	public GameTile GrowPathEast() => GrowPathTo(east,Direction.West);

	public GameTile GrowPathSouth() => GrowPathTo(south,Direction.North);

	public GameTile GrowPathWest() => GrowPathTo(west,Direction.East);
	static Quaternion
	northRotation = Quaternion.Euler(90f, 0f, 0f),
	eastRotation = Quaternion.Euler(90f, 90f, 0f),
	southRotation = Quaternion.Euler(90f, 180f, 0f),
	westRotation = Quaternion.Euler(90f, 270f, 0f);
	/// <summary>
	/// 所有箭头指向destination，destination本身被隐藏
	/// </summary>
	public void ShowPath()
	{
        if (distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
		arrow.localRotation =
			nextOnPath == north ? northRotation :
			nextOnPath == east ? eastRotation :
			nextOnPath == south ? southRotation :
			westRotation;
	}
	/// <summary>
	/// 用于控制地砖按对角线排序
	/// </summary>
	public bool IsAlternative { get; set; }
	GameTileContent content;
	public GameTileContent Content {
		get => content;
		set {
			Debug.Assert(value!=null,"Null assigned to content!");
            if (content != null)
            {
                content.Recycle();
            }
            content = value;
			content.transform.localPosition = transform.position;
		}
	}
	public void HidePath()
	{
		arrow.gameObject.SetActive(false);
	}
	public GameTile NextTileOnPath => nextOnPath;
}
