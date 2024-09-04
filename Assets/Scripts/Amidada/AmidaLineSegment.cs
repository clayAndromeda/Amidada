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
		
		public Vector2 StoE => (End - Start).normalized;
		public Vector2 EtoS => (Start - End).normalized;

		public AmidaLineSegment ExtendStartPosition(float length)
		{
			// 始点方向に指定した長さだけ延長した線分を返す
			Vector2 direction = (Start - End).normalized;
			Vector2 newStart = Start + direction * length;
			return new AmidaLineSegment(newStart, End);
		}

		public AmidaLineSegment ClipVerticalLines(AmidaLineSegment vertLine1, AmidaLineSegment vertLine2)
		{
			// 2本の縦線に挟まれた部分だけを残す

			// 直線の方程式を使って、任意のx座標におけるy座標を求めるデリゲートを作成
			System.Func<float, float> GetY = x =>
			{
				float a = (End.y - Start.y) / (End.x - Start.x);
				float b = Start.y - a * Start.x;
				return a * x + b;
			};

			Vector2 clippedStart = Vector2.zero;
			Vector2 clippedEnd = Vector2.zero;
			if (Start.x < vertLine1.Start.x)
			{
				// 始点が縦線1より左にある場合、始点を縦線1のx座標に移動
				clippedStart.x = vertLine1.Start.x;
				clippedStart.y = GetY(clippedStart.x);
				
				clippedEnd.x = vertLine2.Start.x;
				clippedEnd.y = GetY(clippedEnd.x);
			}
			else
			{
				// 始点が縦線1より右にある場合、始点を縦線2のx座標に移動
				clippedStart.x = vertLine2.Start.x;
				clippedStart.y = GetY(clippedStart.x);
				
				clippedEnd.x = vertLine1.Start.x;
				clippedEnd.y = GetY(clippedEnd.x);
			}
			
			return new AmidaLineSegment(clippedStart, clippedEnd);
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