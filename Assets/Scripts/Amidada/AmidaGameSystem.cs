using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Amidada
{
	public class AmidaGameSystem : MonoBehaviour
	{
		[SerializeField] private AmidaPathPencil pathPencil;
		[SerializeField] private LineRenderer lineTemplate;
		[SerializeField] private float extendedLineLength = 10;

		[Header("Prefab参照")] [SerializeField] private AmidaPlayerObject amidaPlayerPrefab;
		[SerializeField] private AmidaTarget amidaStarPrefab;
		[SerializeField] private AmidaTarget amidaEnemyPrefab;

		[SerializeField] private Camera mainCamera;

		[SerializeField, Range(1, 4)] private int gameSpeed = 1;

		[Header("オブジェクトのルート"), SerializeField]
		private Canvas canvas;

		[SerializeField] private Transform tateLineParent;
		[SerializeField] private Transform yokoLineParent;

		private AmidaLadder ladder;
		private readonly List<AmidaPlayerObject> playerObjects = new();
		private readonly List<AmidaTarget> stars = new();
		private readonly List<AmidaTarget> enemies = new();

		private int gamePoint;
		private bool isAlive = true;

		private void Awake()
		{
			// あみだくじの縦線を生成する
			InitializeTateLines();

			// MEMO: SubscribeAwaitのctは、AddTo or RegisterToで登録してDisposableなオブジェクトの寿命が尽きたタイミングでCancelAndDisposeされる？

			isAlive = false;
			
			// Enterキーを押したら次ステージ開始
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Return))
				.Where(_ => !isAlive)
				.SubscribeAwait(async (_, ct) =>
				{
					isAlive = true;
					
					// 今あるステージは削除
					ClearCurrentGameData();

					// 5点取る度1ステージ進む
					int stageNumber = gamePoint / 5;
					
					// 今のステージ番号に対応するゲーム開始設定を取得する
					var launchSettings = GameLaunchSettings.GetSettingsByStageNumber(stageNumber);
					
					gameSpeed = launchSettings.GameSpeed;
					
					// 線と目標物を生成する
					CreateNewStage(launchSettings);
					
					// ゲーム開始設定に合わせて、最初のプレイヤーを生成する
					var playerLineIndices = ChooseRandomTateLineIndices(launchSettings.PlayerCount);
					foreach (var tateLineIndex in playerLineIndices)
					{
						var newPlayerObject = Instantiate(amidaPlayerPrefab, canvas.transform);
						playerObjects.Add(newPlayerObject);
						SetInitialPosition(newPlayerObject, tateLineIndex);
					}

					var tasks = new List<UniTask>();
					for (int i = 0; i < playerObjects.Count; ++i)
					{
						// 少しずつ遅らせてプレイヤーを動かし始める
						tasks.Add(LaunchPlayerObjectAsync(playerObjects[i], (i + 1) * launchSettings.DelayedSecond, ct));
					}

					await UniTask.WhenAll(tasks);

				}, AwaitOperation.Drop).AddTo(this);

			// 横線を引く処理を登録する
			RegisterPathPencilEvents();
		}

		/// <summary>
		/// 縦線Indexから。プレイヤーを初期位置にセットする
		/// </summary>
		private void SetInitialPosition(AmidaPlayerObject playerObject, int tateLineIndex)
		{
			var startLine = ladder.TateLines[tateLineIndex];
			AmidaPlayerPointData pointDataData = new AmidaPlayerPointData
			{
				Position = startLine.Start,
				Direction = startLine.StoE,
				CurrentLine = ladder.TateLines[tateLineIndex],
				TargetPoint = startLine.End,
			};
			playerObject.SetPointData(pointDataData);
		}

		private async UniTask LaunchPlayerObjectAsync(AmidaPlayerObject playerObject, float initialDelayTime, CancellationToken cancellationToken)
		{
			// initialDelayTime秒待ってから移動開始
			await UniTask.Delay(TimeSpan.FromSeconds(initialDelayTime), cancellationToken: cancellationToken);

			bool moving = true;
			while (moving && playerObject != null)
			{
				if (!isAlive)
				{
					break;
				}
				for (int i = 0; i < gameSpeed; i++)
				{
					ladder.MovePlayerPoint(playerObject.PointData);
					playerObject.UpdatePointData();

					// 縦線の終点より下に行くまで続ける
					if (playerObject.PointData.Position.y <= ladder.TateLines[0].End.y)
					{
						moving = false;
						break;
					}
				}

				await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
			}

			if (!isAlive)
			{
				return; // ゲーム終了済
			}

			if (playerObject != null)
			{
				var playerTateLineIndex = ladder.GetTateLineIndex(playerObject.PointData.CurrentLine);
				var enemy = enemies.FirstOrDefault(x => x.TateLineIndex == playerTateLineIndex);
				if (enemy != null)
				{
					isAlive = false;
					return;
				}
				
				// スターと同じ縦線にいれば、ポイントゲット。必要ならスタート位置に戻る
				var star = stars.FirstOrDefault(x => x.TateLineIndex == playerTateLineIndex);
				if (star != null)
				{
					gamePoint++;
					if (gamePoint % 5 == 0)
					{
						// 5で割り切れるならゲームクリア。
						isAlive = false;
						return;
					}
					
					// まだゲームが続くなら、スタート位置にすぐ戻して、再スタート
					var tateLineIndexList = new List<int> { 0, 1, 2, 3 };
					var index = Random.Range(0, tateLineIndexList.Count);
					SetInitialPosition(playerObject, tateLineIndexList[index]);
					await LaunchPlayerObjectAsync(playerObject, 0, cancellationToken);
				}
			}
		}

		private void CreateNewStage(GameLaunchSettings launchSettings)
		{
			// 縦線の終点に、ランダムで1～3個のスターを配置
			// それ以外の場所にはエネミーを配置する
			var starLineIndices = ChooseRandomTateLineIndices(launchSettings.StarCount);
			var enemyLineIndices = new List<int> { 0, 1, 2, 3 }.Except(starLineIndices).ToList();

			foreach (var starLineIndex in starLineIndices)
			{
				var star = Instantiate(amidaStarPrefab, canvas.transform);
				star.SetPosition(ladder.TateLines[starLineIndex].End, starLineIndex);
				stars.Add(star);
			}

			foreach (var enemyLineIndex in enemyLineIndices)
			{
				var enemy = Instantiate(amidaEnemyPrefab, canvas.transform);
				enemy.SetPosition(ladder.TateLines[enemyLineIndex].End, enemyLineIndex);
				enemies.Add(enemy);
			}
		}

		private static List<int> ChooseRandomTateLineIndices(int count)
		{
			var tateLineIndexList = new List<int> { 0, 1, 2, 3 };
			var chosenIndices = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				var index = Random.Range(0, tateLineIndexList.Count);
				chosenIndices.Add(tateLineIndexList[index]);
				tateLineIndexList.RemoveAt(index);
			}

			return chosenIndices;
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

			// プレイヤーオブジェクトを全削除
			foreach (var playerObject in playerObjects)
			{
				playerObject.Destroy();
			}
			playerObjects.Clear();

			// スターを全削除
			foreach (var star in stars)
			{
				star.Destroy();
			}
			stars.Clear();

			// エネミーを全削除
			foreach (var enemy in enemies)
			{
				enemy.Destroy();
			}
			enemies.Clear();
		}

		private void RegisterPathPencilEvents()
		{
			pathPencil.MousePosition
				.Subscribe(_ =>
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

		private void OnGUI()
		{
			// 画面右上にポイント表示
			GUI.Label(new Rect(Screen.width - 100, 0, 100, 50), $"Point: {gamePoint}");
			// その下に、
		}
	}
}