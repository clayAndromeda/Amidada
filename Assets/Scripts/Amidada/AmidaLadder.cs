using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// あみだくじを構成する全ての線分を持つ
	/// </summary>
	public class AmidaLadder
	{
		/// <summary> 縦向きの線分（終点があみだくじのゴール） </summary>
		public readonly List<AmidaLineSegment> TateLines = new();

		/// <summary> 横糸 </summary>
		public readonly List<AmidaLineSegment> YokoLines = new();
		
		private readonly Subject<Unit> onTurn = new();

		/// <summary> 点が曲がったか？ </summary>
		public Observable<Unit> OnTurn => onTurn;

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
		public bool TryAddYokoLine(AmidaLineSegment line)
		{
			// 他の横糸と交差していたら追加できない
			foreach (var yokoLine in YokoLines)
			{
				if (line.IsIntersect(yokoLine))
				{
					return false;
				}
			}
			YokoLines.Add(line);
			return true;
		}
		
		/// <summary>
		/// 何番目の縦線か？
		/// </summary>
		public int GetTateLineIndex(AmidaLineSegment line)
		{
			return TateLines.FindIndex(tate => Mathf.Approximately(tate.Start.x, line.Start.x));
		}
		
		/// <summary>
		/// あみだくじの線分上を動かす
		/// </summary>
		/// <param name="playerPointData">あみだくじ上を動かす点</param>
		public void MovePlayerPoint(AmidaPlayerPointData playerPointData)
		{
			const float speed = 2.0f; // FixedUpdate1回あたりの移動量
			
			if (TateLines.Contains(playerPointData.CurrentLine))
			{
				// 今縦線上にいる
				
				// 横線の中で、始点のX座標が同じで、Y座標が進行方向に最も近いものを探す
				(AmidaLineSegment yoko, float distance) option1 =
					YokoLines
						.Where(yoko =>
							// 始点が今の縦線上にある
							Mathf.Approximately(yoko.Start.x, playerPointData.CurrentLine.Start.x)
						).Where(yoko =>
							// 始点がプレイヤーのY座標より下にある
							yoko.Start.y < playerPointData.Position.y
						)
						.Select(yoko => (yoko, distance: Mathf.Abs(yoko.Start.y - playerPointData.Position.y)))
						.OrderBy(x => x.distance)
						.FirstOrDefault();

				(AmidaLineSegment yoko, float distance) option2 =
					YokoLines
						.Where(yoko =>
							// 終点が今の縦線上にある
							Mathf.Approximately(yoko.End.x, playerPointData.CurrentLine.Start.x)
						).Where(yoko =>
							// 終点がプレイヤーのY座標より下にある
							yoko.End.y < playerPointData.Position.y
						)
						.Select(yoko => (yoko, distance: Mathf.Abs(yoko.End.y - playerPointData.Position.y)))
						.OrderBy(x => x.distance)
						.FirstOrDefault();
				
				if (option1.yoko == null && option2.yoko == null)
				{
					// まがる横線がないのでまっすぐ進むだけ
					playerPointData.Position += playerPointData.Direction * speed;
					return;
				}
				
				if (option1.yoko == null)
				{
					if (option2.distance <= speed)
					{
						playerPointData.Position = option2.yoko.End;
						playerPointData.Direction = option2.yoko.EtoS;
						playerPointData.TargetPoint = option2.yoko.Start;
						playerPointData.CurrentLine = option2.yoko;
						onTurn.OnNext(Unit.Default);
						return;
					}
				}
				else if (option2.yoko == null)
				{
					if (option1.distance <= speed)
					{
						playerPointData.Position = option1.yoko.Start;
						playerPointData.Direction = option1.yoko.StoE;
						playerPointData.TargetPoint = option1.yoko.End;
						playerPointData.CurrentLine = option1.yoko;
						onTurn.OnNext(Unit.Default);
						return;
					}
				}
				else
				{
					if (option1.distance <= option2.distance && option1.distance <= speed)
					{
						playerPointData.Position = option1.yoko.Start;
						playerPointData.Direction = option1.yoko.StoE;
						playerPointData.TargetPoint = option1.yoko.End;
						playerPointData.CurrentLine = option1.yoko;
						onTurn.OnNext(Unit.Default);
						return;
					}
					
					if (option1.distance > option2.distance && option2.distance <= speed)
					{
						playerPointData.Position = option2.yoko.End;
						playerPointData.Direction = option2.yoko.EtoS;
						playerPointData.TargetPoint = option2.yoko.Start;
						playerPointData.CurrentLine = option2.yoko;
						onTurn.OnNext(Unit.Default);
						return;
					}
				}

				// ここまできたら次の曲がる先までたどり着いてないので、まっすぐ進む
				playerPointData.Position += playerPointData.Direction * speed;
			}
			else
			{
				// 今横線上にいる
				
				// 次曲がる先は、今プレイヤーが向かう点とX座標が同じ縦線
				var nextLine = TateLines.First(tate =>
					Mathf.Approximately(tate.Start.x, playerPointData.TargetPoint.x)
				);
				
				// TODO: 計算誤差めっちゃ出そう
				// 今のプレイヤー位置から次の移動先までの線分と、縦線が交差しているなら、縦線に曲がる
				var movedLine = new AmidaLineSegment(playerPointData.Position, playerPointData.Position + playerPointData.Direction * speed);
				if (movedLine.IsIntersect(nextLine))
				{
					playerPointData.Position = playerPointData.TargetPoint;
					playerPointData.Direction = nextLine.StoE;
					playerPointData.TargetPoint = nextLine.End;
					playerPointData.CurrentLine = nextLine;
					onTurn.OnNext(Unit.Default);
				}
				else
				{
					playerPointData.Position += playerPointData.Direction * speed;
				}
			}
		}
	}
}