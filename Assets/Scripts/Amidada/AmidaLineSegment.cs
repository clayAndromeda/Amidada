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
			// 参考:
			// https://qiita.com/ykob/items/ab7f30c43a0ed52d16f2
			// https://www5d.biglobe.ne.jp/~tomoya03/shtml/algorithm/IntersectionEX.htm
			
			// x座標によるチェック
			if (Start.x >= End.x)
			{
				if ((Start.x < other.Start.x && Start.x < other.End.x) ||
				    (End.x > other.Start.x && End.x > other.End.x))
				{
					return false;
				}
			}
			else
			{
				if ((End.x < other.Start.x && End.x < other.End.x) ||
				    (Start.x > other.Start.x && Start.x > other.End.x))
				{
					return false;
				}
			}

			// y座標によるチェック
			if (Start.y >= End.y)
			{
				if ((Start.y < other.Start.y && Start.y < other.End.y) ||
				    (End.y > other.Start.y && End.y > other.End.y))
				{
					return false;
				}
			}
			else
			{
				if ((End.y < other.Start.y && End.y < other.End.y) ||
				    (Start.y > other.Start.y && Start.y > other.End.y))
				{
					return false;
				}
			}

			if (((Start.x - End.x) * (other.Start.y - Start.y) + (Start.y - End.y) * (Start.x - other.Start.x)) *
			    ((Start.x - End.x) * (other.End.y - Start.y) + (Start.y - End.y) * (Start.x - other.End.x)) > 0)
			{
				return false;
			}

			if (((other.Start.x - other.End.x) * (Start.y - other.Start.y) +
			     (other.Start.y - other.End.y) * (other.Start.x - Start.x)) *
			    ((other.Start.x - other.End.x) * (End.y - other.Start.y) +
			     (other.Start.y - other.End.y) * (other.Start.x - End.x)) > 0)
			{
				return false;
			}

			return true;
		}
	}
}