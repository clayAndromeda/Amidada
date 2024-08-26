using System.Collections.Generic;

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

		public AmidaLadder()
		{
			AmidaLineSegment[] initialWarps = {
				new (new (75, 590), new (75, 50)), // 左から1番目の線
				new (new (235, 590), new (235, 50)), // 左から2番目の線
				new (new (395, 590), new (395, 50)), // 左から3番目の線
				new (new (555, 590), new (555, 50)), // 左から4番目の線
			};
			WarpLines.AddRange(initialWarps);
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
			return true;
		}
	}
}