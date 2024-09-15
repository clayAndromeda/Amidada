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
	public class AmidaGameSystem : IDisposable
	{
		private AmidaPathPencil pathPencil;
		private AmidaStage stage;
		
		
		public int GamePoint { get; private set; }
		
		private AmidaLadder ladder;
		private int gameSpeed;
		private bool isAlive;
		private bool isSpeedUp;
		
		private DisposableBag gameDisposableBag = new();

		private const float ExtendedLineLength = 10;

		public AmidaGameSystem(AmidaStage stageArg, AmidaPathPencil pathPencilArg)
		{
			stage = stageArg;
			pathPencil = pathPencilArg;
		}

		public async UniTask StartGameAsync()
		{
			DisposableBag disposableBag = new();
			
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Space))
				.Subscribe(_ => isSpeedUp = true).AddTo(ref disposableBag);
			
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyUp(KeyCode.Space))
				.Subscribe(_ => isSpeedUp = false).AddTo(ref disposableBag);
			
			// ペンで横線を描けるようにイベント登録
			RegisterPathPencilEvents(disposableBag);

			// 今のステージをリセットする
			stage.ResetStageObjects();
			
			// 5点取る度1ステージ進む
			int stageNumber = GamePoint / 5;
			
			// 今のステージ番号に対応するゲーム開始設定を取得する
			var (launchSettings, initialYokoLines) = GameLaunchSettings.GetSettingsByStageNumber(stageNumber);
			gameSpeed = launchSettings.GameSpeed;
			
			// Ladderを初期化
			ladder = new AmidaLadder();
			foreach (var yoko in initialYokoLines)
			{
				ladder.TryAddYokoLine(yoko);
			}

			// 縦線の終点に、ランダムで1～3個のスターを配置
			var starLineIndices = ChooseRandomTateLineIndices(launchSettings.StarCount);
			// スターが配置されなかった場所にはエネミーを配置する
			var enemyLineIndices = new List<int> { 0, 1, 2, 3 }.Except(starLineIndices).ToList();
			
			// ステージ生成
			stage.CreateNewStage(ladder, starLineIndices, enemyLineIndices);
			
			// プレイヤーオブジェクトを生成する
			var playerLineIndices = ChooseRandomTateLineIndices(launchSettings.PlayerCount);
			var playerObjects = stage.CreatePlayerObjects(ladder, playerLineIndices);

			isAlive = true;
			CancellationTokenSource cts = new();
			cts.AddTo(ref gameDisposableBag);
			
			var tasks = new List<UniTask>();
			for (int i = 0; i < playerObjects.Length; i++)
			{
				// 少しずつ遅らせてプレイヤーを動かし始める
				tasks.Add(LaunchPlayerObjectAsync(playerObjects[i], (i + 1) * launchSettings.DelayedSecond, cts.Token));
			}
			await UniTask.WhenAll(tasks);
		}

		private async UniTask LaunchPlayerObjectAsync(AmidaPlayerObject playerObject, float initialDelayTime, CancellationToken cancellationToken)
		{
			// initialDelayTime秒待ってから移動開始
			// TODO: スタート時間のDelayも加速したい
			await UniTask.Delay(TimeSpan.FromSeconds(initialDelayTime), cancellationToken: cancellationToken);

			bool moving = true;
			while (moving && playerObject != null)
			{
				if (!isAlive)
				{
					break;
				}

				int sampleRate = isSpeedUp ? 3 : gameSpeed;
				for (int i = 0; i < sampleRate; i++)
				{
					ladder.MovePlayerPoint(playerObject.PointData);
					playerObject.UpdateAnchoredPosition();

					// 縦線の終点より下に行くまで続ける
					if (playerObject.PointData.Position.y <= ladder.TateLines[0].End.y)
					{
						moving = false;
						break;
					}
				}

				await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
			}

			if (!isAlive)
			{
				return; // ゲーム終了済
			}

			if (playerObject != null)
			{
				var playerTateLineIndex = ladder.GetTateLineIndex(playerObject.PointData.CurrentLine);
				var enemy = stage.Enemies.FirstOrDefault(x => x.TateLineIndex == playerTateLineIndex);
				if (enemy != null)
				{
					isAlive = false;
					return;
				}
				
				// スターと同じ縦線にいれば、ポイントゲット。必要ならスタート位置に戻る
				var star = stage.Stars.FirstOrDefault(x => x.TateLineIndex == playerTateLineIndex);
				if (star != null)
				{
					GamePoint++;
					if (GamePoint % 5 == 0)
					{
						// 5で割り切れるならゲームクリア。
						isAlive = false;
						return;
					}
					
					// まだゲームが続くなら、スタート位置にすぐ戻して、再スタート
					var tateLineIndexList = new List<int> { 0, 1, 2, 3 };
					var index = Random.Range(0, tateLineIndexList.Count);
					var restartedLine = ladder.TateLines[tateLineIndexList[index]];
					AmidaPlayerPointData restartedPointData = new AmidaPlayerPointData
					{
						Position = restartedLine.Start,
						Direction = restartedLine.StoE,
						CurrentLine = restartedLine,
						TargetPoint = restartedLine.End,
					};
					playerObject.SetPointData(restartedPointData);
					await LaunchPlayerObjectAsync(playerObject, 0, cancellationToken);
				}
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
		
		private void RegisterPathPencilEvents(DisposableBag disposableBag)
		{
			pathPencil.MousePosition
				.Subscribe(_ =>
				{
					var line = pathPencil.StartToEndLine;
					if (line == null) return;

					var extendedLine = line.ExtendStartPosition(ExtendedLineLength);

					for (int i = 0; i < ladder.TateLines.Count - 1; i++)
					{
						// 始点方向に3px延長した線分が、2本の縦線と交差しているか判定
						if (extendedLine.IsIntersect(ladder.TateLines[i]) &&
						    extendedLine.IsIntersect(ladder.TateLines[i + 1]))
						{
							// 始点と終点を2本の縦線で切り取る
							var clippedLine = extendedLine.ClipVerticalLines(ladder.TateLines[i], ladder.TateLines[i + 1]);
							
							// TODO: これいらなそう
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
							stage.CreateYokoLine(clippedLine);
							pathPencil.StopDraw();
						}
					}
				}).AddTo(ref disposableBag);
		}

		public void Dispose()
		{
			gameDisposableBag.Dispose();
		}
	}
}