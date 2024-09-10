using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Amidada
{
	public class AmidaGameSystem : MonoBehaviour
	{
		[SerializeField] private AmidaPathPencil pathPencil;
		[SerializeField] private LineRenderer lineTemplate;
		[SerializeField] private float extendedLineLength = 10;

		[Header("Prefab参照")]
		[SerializeField] private AmidaPlayerObject amidaPlayerPrefab;
		[SerializeField] private AmidaTarget amidaStarPrefab;
		[SerializeField] private AmidaTarget amidaEnemyPrefab;

		[SerializeField] private Camera mainCamera;

		[SerializeField, Range(1, 4)] private int gameSpeed = 1;
		
		[Header("オブジェクトのルート")]
		[SerializeField] private Canvas canvas;
		[SerializeField] private Transform tateLineParent;
		[SerializeField] private Transform yokoLineParent;

		private AmidaLadder ladder;
		private AmidaPlayerObject playerObject;
		private readonly List<AmidaTarget> stars = new();
		private readonly List<AmidaTarget> enemies = new();

		private int point;

		private void Awake()
		{
			// あみだくじの縦線を生成する
			InitializeTateLines();
			
			// スペースキーを押したら、ゲーム状況をリセットする
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Space))
				.Subscribe(_ =>
				{
					// 現在の横糸・プレイヤー・ターゲットを削除
					ClearCurrentGameData();

					// 縦線の終点に、ランダムで1～3個のスターを配置（それ以外の場所にはエネミーを配置する）
					var tateLineIndexList = new List<int>{ 0, 1, 2, 3 };
					var startCount = Random.Range(1, 4);
					var starLineIndexList = new List<int>(startCount);
					for (int i = 0; i < startCount; i++)
					{
						var index = Random.Range(0, tateLineIndexList.Count);
						starLineIndexList.Add(tateLineIndexList[index]);
						tateLineIndexList.RemoveAt(index);
					}

					var enemyLineIndexList = new List<int> { 0, 1, 2, 3 }.Except(starLineIndexList).ToList();
					foreach (var starLineIndex in starLineIndexList)
					{
						var star = Instantiate(amidaStarPrefab, canvas.transform);
						star.SetPosition(ladder.TateLines[starLineIndex].End, starLineIndex);
						stars.Add(star);
					}
					foreach (var enemyLineIndex in enemyLineIndexList)
					{
						var enemy = Instantiate(amidaEnemyPrefab, canvas.transform);
						enemy.SetPosition(ladder.TateLines[enemyLineIndex].End, enemyLineIndex);
						enemies.Add(enemy);
					}

					// スターを所定個集めたらゴール。次のステージへ。
				}).AddTo(this);

			// Enterキーを押したら移動開始
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Return))
				.SubscribeAwait(async (_, ct) =>
				{
					// あみだくじを下る点を生成
					var startLine = ladder.TateLines[0];
					playerObject = Instantiate(amidaPlayerPrefab, canvas.transform);
					AmidaPlayerPoint pointData = new AmidaPlayerPoint
					{
						Position = startLine.Start,
						Direction = startLine.StoE,
						CurrentLine = ladder.TateLines[0],
						TargetPoint = startLine.End,
					};
					playerObject.SetPointData(pointData);

					// TODO: 複数のプレイヤーを段階的に動かすにはどうする？
					bool moving = true;
					while (moving && playerObject != null)
					{
						for (int i = 0; i < gameSpeed; i++)
						{
							ladder.MovePlayerPoint(pointData);
							playerObject.SetPointData(pointData);

							// 縦線の終点より下に行くまで続ける
							if (pointData.Position.y <= ladder.TateLines[0].End.y)
							{
								moving = false;
								break;
							}
						}

						await UniTask.Yield(PlayerLoopTiming.Update, ct);
					}
					
					if (playerObject != null)
					{
						var playerTateLineIndex = ladder.GetTateLineIndex(pointData.CurrentLine);
						// スターと同じ縦線にいる？
						var star = stars.FirstOrDefault(x => x.TateLineIndex == playerTateLineIndex);
						var enemy = enemies.FirstOrDefault(x => x.TateLineIndex == playerTateLineIndex);
						if (star != null)  { point++; }
						else if (enemy != null) { point--; }
					}

					Debug.Log("Finished! Point: " + point);
				}, AwaitOperation.Drop).AddTo(this);

			// 横線を引く処理を登録する
			RegisterPathPencilEvents();
		}

		private void InitializeTateLines()
		{
			ladder = new AmidaLadder();
			foreach (var line in ladder.TateLines)
			{
				CreateLineSegmentObject(line, tateLineParent);
			}
		}
		
		private void ClearCurrentGameData()
		{
			// 横線を全削除
			ladder.ClearYokoLines();
			foreach (Transform child in yokoLineParent)
			{
				Destroy(child.gameObject);
			}

			if (playerObject != null)
			{
				playerObject?.Destroy();
				playerObject = null;
			}
			
			foreach (var star in stars)
			{
				star.Destroy();
			}
			stars.Clear();
			
			foreach (var enemy in enemies)
			{
				enemy.Destroy();
			}
			enemies.Clear();
		}


		private void RegisterPathPencilEvents()
		{
			pathPencil.MousePosition
				.Subscribe(_  =>
				{
					var line = pathPencil.StartToEndLine;
					if (line == null) return;
					
					var extendedLine = line.ExtendStartPosition(extendedLineLength);

					for (int i = 0; i < ladder.TateLines.Count - 1; i++)
					{
						// 始点方向に3px延長した線分が、2本の縦線と交差しているか判定
						if (extendedLine.IsIntersect(ladder.TateLines[i]) &&
						    extendedLine.IsIntersect(ladder.TateLines[i + 1]))
						{
							// 始点と終点を2本の縦線で切り取る
							var clippedLine =
								extendedLine.ClipVerticalLines(ladder.TateLines[i], ladder.TateLines[i + 1]);
							// 横糸は、X座標が小さい方から大きい方へ向かうようにする
							if (clippedLine.Start.x > clippedLine.End.x)
							{
								clippedLine = new AmidaLineSegment(clippedLine.End, clippedLine.Start);
							}

							if (!ladder.TryAddYokoLine(clippedLine))
							{
								pathPencil.StopDraw();
								return;
							}

							// 横糸を追加したので、線分を描画して終了
							CreateLineSegmentObject(clippedLine, yokoLineParent);
							pathPencil.StopDraw();
							return;
						}
					}
				}).AddTo(this);
		}

		private void CreateLineSegmentObject(AmidaLineSegment lineSegment, Transform parent)
		{
			var line = Instantiate(lineTemplate, parent);
			var startPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.Start);
			var endPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.End);

			line.SetPosition(0, new(startPointScreenSpace.x, startPointScreenSpace.y, 0));
			line.SetPosition(1, new(endPointScreenSpace.x, endPointScreenSpace.y, 0));
		}
	}
}