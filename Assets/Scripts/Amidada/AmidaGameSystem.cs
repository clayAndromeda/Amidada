using R3;
using UnityEngine;

namespace Amidada
{
	public class AmidaGameSystem : MonoBehaviour
	{
		[SerializeField] private AmidaPathPencil pathPencil;
		[SerializeField] private LineRenderer lineTemplate;
		[SerializeField] private Camera mainCamera;
		[SerializeField] private float extendedLineLength = 50;
		
		private AmidaLadder ladder;

		private int currentLineStartIndex = -1;
		private Vector2? currentLineStartPosition = null;
		
		private void Awake()
		{
			// あみだくじの道筋をつくる処理を初期化
			InitializeLadder();
			
			// pathPencil.MousePositionがnullから変化したイベントを購読する
			pathPencil.MousePosition
				.Pairwise()
				.Where(pair => pair.Previous == null && pair.Current.HasValue)
				.Subscribe(x =>
				{
					Debug.Log("書き始めた！");
					var mousePosition = x.Current.Value;
					currentLineStartPosition = mousePosition;
				}).AddTo(this);

			pathPencil.MousePosition
				.Subscribe(x =>
				{
					if (!currentLineStartPosition.HasValue || !x.HasValue)
					{
						return;
					}
					
					for (int i = 0; i < ladder.VerticalLines.Count - 1; i++)
					{
						// 始点方向に3px延長した線分が、2本の縦線と交差しているか判定
						var line = new AmidaLineSegment(currentLineStartPosition.Value, x.Value);
						var extendedLine = line.ExtendStartPosition(extendedLineLength);
						if (extendedLine.IsIntersect(ladder.VerticalLines[i]) && extendedLine.IsIntersect(ladder.VerticalLines[i + 1]))
						{
							var clippedLine = extendedLine.ClipX(ladder.VerticalLines[i].Start.x, ladder.VerticalLines[i + 1].Start.x);
							CreateLineSegmentObject(clippedLine);
							pathPencil.StopDraw();
							return;
						}
					}
				}).AddTo(this);
		}

		private void InitializeLadder()
		{
			ladder = new AmidaLadder();
			foreach (var line in ladder.VerticalLines)
			{
				CreateLineSegmentObject(line);
			}
		}

		private void CreateLineSegmentObject(AmidaLineSegment lineSegment)
		{
			var line = Instantiate(lineTemplate, transform);
			var startPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.Start);
			var endPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.End);
			
			line.SetPosition(0, new (startPointScreenSpace.x, startPointScreenSpace.y, 0));
			line.SetPosition(1, new (endPointScreenSpace.x, endPointScreenSpace.y, 0));
		}
	}
}