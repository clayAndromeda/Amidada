using ObservableCollections;
using R3;
using UnityEngine;

namespace Amidada
{
	public class AmidaGameSystem : MonoBehaviour
	{
		[SerializeField] private AmidaPathPencil pathPencil;
		[SerializeField] private LineRenderer lineTemplate;
		[SerializeField] private Camera mainCamera;
		
		private AmidaLadder ladder;

		private void Awake()
		{
			// あみだくじの道筋をつくる処理を初期化
			InitializeLadder();
		}

		private void InitializeLadder()
		{
			ladder = new AmidaLadder();
			foreach (var line in ladder.VerticalLines)
			{
				CreateLineSegmentObject(line);
			}

			ladder.VerticalLines.ObserveAdd()
				.Subscribe(x => CreateLineSegmentObject(x.Value))
				.AddTo(this);
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