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
		public readonly List<AmidaLineSegment> TateLines = new();

		/// <summary> 横糸 </summary>
		private readonly List<AmidaLineSegment> yokoLines = new();

		public AmidaLadder()
		{
			AmidaLineSegment[] initialWarps = {
				new (new (75, 590), new (75, 50)), // 左から1番目の線
				new (new (235, 590), new (235, 50)), // 左から2番目の線
				new (new (395, 590), new (395, 50)), // 左から3番目の線
				new (new (555, 590), new (555, 50)), // 左から4番目の線
			};
			TateLines.AddRange(initialWarps);
		}
		
		/// <summary>
		/// 横糸の追加を試みる
		/// </summary>
		/// <returns>追加できた</returns>
		public bool TryAddWoofLine(AmidaLineSegment line)
		{
			// 他の横糸と交差していたら追加できない
			foreach (var woofLine in yokoLines)
			{
				if (line.IsIntersect(woofLine))
				{
					return false;
				}
			}
			yokoLines.Add(line);
			return true;
		}
		
		/// <summary>
		/// あみだくじの線分上を動かす
		/// </summary>
		/// <param name="playerPoint">あみだくじ上を動かす点</param>
		public void Moved(AmidaPlayerPoint playerPoint)
		{
			const float speed = 1.0f;
			
			if (TateLines.Contains(playerPoint.CurrentLine))
			{
				// 今縦線上にいる
				
				// 横線の中で、始点のX座標が同じで、Y座標が進行方向に最も近いものを探す
				(AmidaLineSegment yoko, float distance) option1 =
					yokoLines
						.Where(yoko =>
							// 始点が今の縦線上にある
							Mathf.Approximately(yoko.Start.x, playerPoint.CurrentLine.Start.x)
						).Where(yoko =>
							// 始点がプレイヤーのY座標より下にある
							yoko.Start.y < playerPoint.Position.y
						)
						.Select(yoko => (yoko, distance: Mathf.Abs(yoko.Start.y - playerPoint.Position.y)))
						.OrderBy(x => x.distance)
						.FirstOrDefault();

				(AmidaLineSegment yoko, float distance) option2 =
					yokoLines
						.Where(yoko =>
							// 終点が今の縦線上にある
							Mathf.Approximately(yoko.End.x, playerPoint.CurrentLine.Start.x)
						).Where(yoko =>
							// 終点がプレイヤーのY座標より下にある
							yoko.End.y < playerPoint.Position.y
						)
						.Select(yoko => (yoko, distance: Mathf.Abs(yoko.End.y - playerPoint.Position.y)))
						.OrderBy(x => x.distance)
						.FirstOrDefault();
				
				if (option1.yoko == null && option2.yoko == null)
				{
					// まがる横線がないのでまっすぐ進むだけ
					playerPoint.Position += playerPoint.Direction * speed;
					return;
				}
				
				if (option1.yoko == null)
				{
					if (option2.distance <= speed)
					{
						playerPoint.Position = option2.yoko.End;
						playerPoint.Direction = option2.yoko.EtoS;
						playerPoint.TargetPoint = option2.yoko.Start;
						playerPoint.CurrentLine = option2.yoko;
						return;
					}
				}
				else if (option2.yoko == null)
				{
					if (option1.distance <= speed)
					{
						playerPoint.Position = option1.yoko.Start;
						playerPoint.Direction = option1.yoko.StoE;
						playerPoint.TargetPoint = option1.yoko.End;
						playerPoint.CurrentLine = option1.yoko;
						return;
					}
				}
				else
				{
					if (option1.distance <= option2.distance && option1.distance <= speed)
					{
						playerPoint.Position = option1.yoko.Start;
						playerPoint.Direction = option1.yoko.StoE;
						playerPoint.TargetPoint = option1.yoko.End;
						playerPoint.CurrentLine = option1.yoko;
						return;
					}
					
					if (option1.distance > option2.distance && option2.distance <= speed)
					{
						playerPoint.Position = option2.yoko.End;
						playerPoint.Direction = option2.yoko.EtoS;
						playerPoint.TargetPoint = option2.yoko.Start;
						playerPoint.CurrentLine = option2.yoko;
						return;
					}
				}

				// ここまできたら次の曲がる先までたどり着いてないので、まっすぐ進む
				playerPoint.Position += playerPoint.Direction * speed;
			}
			else
			{
				// 今横線上にいる
				
				// 次曲がる先は、今プレイヤーが向かう点とX座標が同じ縦線
				var nextLine = TateLines.First(tate =>
					Mathf.Approximately(tate.Start.x, playerPoint.TargetPoint.x)
				);
				
				// TODO: 計算誤差めっちゃ出そう
				// 今のプレイヤー位置から次の移動先までの線分と、縦線が交差しているなら、縦線に曲がる
				var movedLine = new AmidaLineSegment(playerPoint.Position, playerPoint.Position + playerPoint.Direction * speed);
				if (movedLine.IsIntersect(nextLine))
				{
					playerPoint.Position = playerPoint.TargetPoint;
					playerPoint.Direction = nextLine.StoE;
					playerPoint.TargetPoint = nextLine.End;
					playerPoint.CurrentLine = nextLine;
				}
				else
				{
					playerPoint.Position += playerPoint.Direction * speed;
				}
			}
		}
	}
}