using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Amidada
{
	public class AmidaGameSystem : MonoBehaviour
	{
		[SerializeField] private AmidaPathPencil pathPencil;
		[SerializeField] private LineRenderer lineTemplate;

		[SerializeField] private RectTransform amidaPlayerPrefab;
		[SerializeField] private Canvas canvas;

		[SerializeField] private Camera mainCamera;
		[SerializeField] private float extendedLineLength = 10;

		[SerializeField, Range(1, 4)] private int gameSpeed = 1;

		private AmidaLadder ladder;

		private void Awake()
		{
			// あみだくじの道筋をつくる処理を初期化
			InitializeLadder();

			// スペースキーを押したら移動開始
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Space))
				.SubscribeAwait(async (_, ct) =>
				{
					// あみだくじを下る点を生成
					var startLine = ladder.TateLines[0];
					var playerTransform = Instantiate(amidaPlayerPrefab, canvas.transform);
					playerTransform.anchoredPosition = startLine.Start;
					AmidaPlayerPoint playerPoint = new AmidaPlayerPoint
					{
						Position = playerTransform.anchoredPosition,
						Direction = startLine.StoE,
						CurrentLine = ladder.TateLines[0],
						TargetPoint = startLine.End,
					};

					bool moving = false;
					while (!moving)
					{
						for (int i = 0; i < gameSpeed; i++)
						{
							ladder.Moved(playerPoint);
							playerTransform.anchoredPosition = playerPoint.Position;

							// 縦線の終点より下に行くまで続ける
							if (playerPoint.Position.y <= ladder.TateLines[0].End.y)
							{
								moving = true;
								break;
							}
						}

						await UniTask.Yield(PlayerLoopTiming.Update, ct);
					}

					Debug.Log("Finished!");
				}).AddTo(this);

			RegisterPathPencilEvents();
		}

		private void InitializeLadder()
		{
			ladder = new AmidaLadder();
			foreach (var line in ladder.TateLines)
			{
				CreateLineSegmentObject(line);
			}
		}

		private void RegisterPathPencilEvents()
		{
			pathPencil.MousePosition
				.Subscribe(x =>
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

							if (!ladder.TryAddWoofLine(clippedLine))
							{
								pathPencil.StopDraw();
								return;
							}

							// 横糸を追加したので、線分を描画して終了
							CreateLineSegmentObject(clippedLine);
							pathPencil.StopDraw();
							return;
						}
					}
				}).AddTo(this);
		}

		private void CreateLineSegmentObject(AmidaLineSegment lineSegment)
		{
			var line = Instantiate(lineTemplate, transform);
			var startPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.Start);
			var endPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.End);

			line.SetPosition(0, new(startPointScreenSpace.x, startPointScreenSpace.y, 0));
			line.SetPosition(1, new(endPointScreenSpace.x, endPointScreenSpace.y, 0));
		}
	}
}