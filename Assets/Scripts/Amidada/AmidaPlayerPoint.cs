using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// あみだくじ上を動く点
	/// </summary>
	public class AmidaPlayerPoint
	{
		/// <summary> 今どの位置にいる？ </summary>
		public Vector2 Position;
		
		/// <summary> 動く方向 </summary>
		public Vector2 Direction;
		
		/// <summary> 今線上のどの点の方向に向かっているか？ </summary>
		public Vector2 TargetPoint;

		/// <summary> 今どの線上を動いているか？ </summary>
		public AmidaLineSegment CurrentLine;
		
	}
}