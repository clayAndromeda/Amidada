using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// プレイヤーが引いた線を描画するためのコンポーネント
	/// </summary>
	public class AmidaPathPencil : MonoBehaviour
	{
		/*
		 * プレイヤーがカーソルを動かしている間、その軌跡を描き続ける
		 * あみだくじの縦線に触れたら、軌跡を消して始点から終点を線分で結ぶ
		 * 線を描き終わったら、どういう線を書いたか、ゲームシステムに伝える
		 *
		 * 考慮したほうがいいこと
		 * ・先に引いた横線と交差した時 → 始点から終点を結ぶ線分は書けない。失敗した音を鳴らす
		 * ・DSの上画面のように、線を描けない場所があったほうがいい
		 * ・線を引き始める時、そこは線を引ける場所かどうかを判定する
		 */
		[SerializeField] private Camera mainCamera;
		[SerializeField] private LineRenderer linePrefab;

		private LineRenderer currentLine;
		private readonly List<Vector3> pathPoints = new();
		private readonly ReactiveProperty<Vector3?> mousePosition = new(null);

		/// <summary> スクリーン座標系でのマウス位置（nullなら無効） </summary>
		public ReadOnlyReactiveProperty<Vector3?> MousePosition => mousePosition;

		private bool canDraw = true;

		public void StopDraw()
		{
			canDraw = false;

			// マウス位置を無効にする
			mousePosition.Value = null;
			// 今引いている線を消す
			if (currentLine != null)
			{
				Destroy(currentLine.gameObject);
				currentLine = null;
			}
		}

		private void Awake()
		{
			Observable.EveryUpdate()
				.Where(_ => Input.GetMouseButtonDown(0)) // マウスの左クリックを押下した
				.Subscribe(_ =>
				{
					canDraw = true;
					var mousePositionScreenSpace = Input.mousePosition;
					mousePosition.Value = mousePositionScreenSpace;
					CreateLine();
				}).AddTo(this);

			Observable.EveryUpdate()
				.Where(_ => Input.GetMouseButton(0)) // マウスの左クリック押下中
				.Subscribe(_ =>
				{
					if (!canDraw)
					{
						mousePosition.Value = null;
						return;
					}
					
					var mousePositionScreenSpace = Input.mousePosition;
					Vector3 mousePositionWorldSpace = mainCamera.ScreenToWorldPoint(mousePositionScreenSpace);
					mousePositionWorldSpace.z = 0;
					if (!pathPoints.Contains(mousePositionWorldSpace))
					{
						AddPointToPath(mousePositionWorldSpace);
					}
					
					mousePosition.Value = mousePositionScreenSpace;
				}).AddTo(this);

			Observable.EveryUpdate()
				.Where(_ => Input.GetMouseButtonUp(0)) // マウスの左クリックを離した
				.Subscribe(_ =>
				{
					StopDraw();
				}).AddTo(this);
		}

		private void CreateLine()
		{
			currentLine = Instantiate(linePrefab, transform);
			pathPoints.Clear();
			currentLine.positionCount = 0;
		}

		private void AddPointToPath(Vector3 point)
		{
			pathPoints.Add(point);
			currentLine.positionCount = pathPoints.Count;
			currentLine.SetPositions(pathPoints.ToArray());
		}
	}
}