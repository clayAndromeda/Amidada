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

		public AmidaLineSegment ExtendStartPosition(float length)
		{
			// 始点方向に指定した長さだけ延長した線分を返す
			Vector2 direction = (Start - End).normalized;
			Vector2 newStart = Start + direction * length;
			return new AmidaLineSegment(newStart, End);
		}

		public AmidaLineSegment ClipX(float startX, float endX)
		{
			// 線分を指定した範囲でクリップする
			float x1 = Mathf.Max(Start.x, startX);
			float x2 = Mathf.Min(End.x, endX);
			return new AmidaLineSegment(new Vector2(x1, Start.y), new Vector2(x2, End.y));
		}

		public bool IsIntersect(AmidaLineSegment other)
		{
			// 線分が交差しているか判定する
			// 参考: https://qiita.com/ykob/items/ab7f30c43a0ed52d16f2
			float ax = Start.x;
			float ay = Start.y;
			float bx = End.x;
			float by = End.y;
			float cx = other.Start.x;
			float cy = other.Start.y;
			float dx = other.End.x;
			float dy = other.End.y;
			
			var ta = (cx - dx) * (ay - cy) + (cy - dy) * (cx - ax);
			var tb = (cx - dx) * (by - cy) + (cy - dy) * (cx - bx);
			var tc = (ax - bx) * (cy - ay) + (ay - by) * (ax - cx);
			var td = (ax - bx) * (dy - ay) + (ay - by) * (ax - dx);

			// return tc * td < 0 && ta * tb < 0;
			return tc * td <= 0 && ta * tb <= 0; // 端点を含む場合
		}
	}
}