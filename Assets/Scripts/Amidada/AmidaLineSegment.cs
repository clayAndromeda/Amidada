using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// あみだくじの線分を表すクラス
	/// </summary>
	public class AmidaLineSegment
	{
		public Vector2 Start { get; }
		public Vector2 End { get; }
		
		public AmidaLineSegment(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}
		
		public AmidaLineSegment(LineRenderer lineRenderer)
		{
			Debug.Assert(lineRenderer.positionCount == 2, "LineRenderer must have 2 positions");
			var start3D = lineRenderer.GetPosition(0);
			var end3D = lineRenderer.GetPosition(1);
			
			Start = new Vector2(start3D.x, start3D.y);
			End = new Vector2(end3D.x, end3D.y);
		}
	}
}