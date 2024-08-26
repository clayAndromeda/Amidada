using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// あみだくじを構成する全ての線分を持つ
	/// </summary>
	public class AmidaLadder
	{
		/// <summary> 縦向きの線分 </summary>
		public readonly List<AmidaLineSegment> WarpLines = new();

		/// <summary> 横糸 </summary>
		private readonly List<AmidaLineSegment> woofLines = new();

		private class AmidaRouteData
		{
			public AmidaLineSegment Line;

			/// <summary> Lineと交差する線 </summary>
			public List<AmidaLineSegment> IntersectingLines = new();
		}
		
		private List<AmidaRouteData> routeData = new();

		public AmidaLadder()
		{
			AmidaLineSegment[] initialWarps = {
				new (new (75, 590), new (75, 50)), // 左から1番目の線
				new (new (235, 590), new (235, 50)), // 左から2番目の線
				new (new (395, 590), new (395, 50)), // 左から3番目の線
				new (new (555, 590), new (555, 50)), // 左から4番目の線
			};
			WarpLines.AddRange(initialWarps);
			
			routeData.AddRange(initialWarps.Select(warp => new AmidaRouteData { Line = warp }));
		}
		
		/// <summary>
		/// 横糸の追加を試みる
		/// </summary>
		/// <returns>追加できた</returns>
		public bool TryAddWoofLine(AmidaLineSegment line)
		{
			// 他の横糸と交差していたら追加できない
			foreach (var woofLine in woofLines)
			{
				if (line.IsIntersect(woofLine))
				{
					return false;
				}
			}
			woofLines.Add(line);
			AddRouteData(line);

			return true;
		}

		private void AddRouteData(AmidaLineSegment line)
		{
			// あみだの経路計算用データを更新
			if (Mathf.Approximately(line.Start.x, WarpLines[0].Start.x) ||
			    Mathf.Approximately(line.End.x, WarpLines[0].Start.x))
			{
				// 左から1番目の線と2番目の線を結ぶ横糸
				routeData.First(data => data.Line == WarpLines[0]).IntersectingLines.Add(line);
				routeData.First(data => data.Line == WarpLines[1]).IntersectingLines.Add(line);
			}
			else if (Mathf.Approximately(line.Start.x, WarpLines[1].Start.x) ||
			         Mathf.Approximately(line.End.x, WarpLines[1].Start.x))
			{
				// 2番目と3番目の線を結ぶ横糸
				routeData.First(data => data.Line == WarpLines[1]).IntersectingLines.Add(line);
				routeData.First(data => data.Line == WarpLines[2]).IntersectingLines.Add(line);
			}
			else
			{
				// 3番目と4番目の線を結ぶ横糸
				routeData.First(data => data.Line == WarpLines[2]).IntersectingLines.Add(line);
				routeData.First(data => data.Line == WarpLines[3]).IntersectingLines.Add(line);
			}

			// 横糸の情報もRouteDataに追加する
			routeData.Add(new AmidaRouteData { Line = line });
		}

		/// <summary>
		/// あみだくじの線分上をmeterだけ進む
		/// </summary>
		/// <param name="position">今の位置</param>
		/// <param name="currentLine">今、どの線分上に乗っているか？</param>
		/// <param name="meter">何m進むか？</param>
		/// <returns>(進んだ先の位置, 進んだ先でどの線分上に乗っているか？)</returns>
		public (Vector2, AmidaLineSegment) Step(Vector2 position, AmidaLineSegment currentLine, float meter)
		{
			// routeDataを使って、meterだけ進んだ先の位置と、進んだ先でどの線分上に乗っているかを求める
			// 進んだ先の位置が線分の終点を超えた場合、次の線分に移動する
			
			// この辺り、思ってたより実装大変だー（でもここ乗り越えたら、後は物量だけ）
			
			return default;
		}
	}
}