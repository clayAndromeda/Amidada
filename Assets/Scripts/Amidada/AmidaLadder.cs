using System.Collections.Generic;

namespace Amidada
{
	/// <summary>
	/// あみだくじを構成する全ての線分を持つ
	/// </summary>
	public class AmidaLadder
	{
		/// <summary> 縦向きの線分 </summary>
		public readonly List<AmidaLineSegment> VerticalLines = new();

		public AmidaLadder()
		{
			AmidaLineSegment[] initialLadders = {
				new (new (75, 590), new (75, 50)), // 左から1番目の線
				new (new (235, 590), new (235, 50)), // 左から2番目の線
				new (new (395, 590), new (395, 50)), // 左から3番目の線
				new (new (555, 590), new (555, 50)), // 左から4番目の線
			};
			VerticalLines.AddRange(initialLadders);
		}
	}
}