using System.Collections.Generic;
using UnityEngine;

namespace Amidada
{
	public class AmidaStage
	{
		private Camera mainCamera;
		private LineRenderer lineTemplate;
		private Canvas canvas;
		
		private AmidaPlayerObject amidaPlayerPrefab;
		private AmidaTarget amidaStarPrefab;
		private AmidaTarget amidaEnemyPrefab;
		
		private Transform tateLineParent;
		private Transform yokoLineParent;

		public AmidaStage(Camera mainCamera, LineRenderer lineTemplate, Canvas canvas, AmidaPlayerObject amidaPlayerPrefab, AmidaTarget amidaStarPrefab, AmidaTarget amidaEnemyPrefab, Transform tateLineParent, Transform yokoLineParent)
		{
			this.mainCamera = mainCamera;
			this.lineTemplate = lineTemplate;
			this.canvas = canvas;
			this.amidaPlayerPrefab = amidaPlayerPrefab;
			this.amidaStarPrefab = amidaStarPrefab;
			this.amidaEnemyPrefab = amidaEnemyPrefab;
			this.tateLineParent = tateLineParent;
			this.yokoLineParent = yokoLineParent;
		}

		public List<AmidaTarget> Stars { get; } = new();
		public List<AmidaTarget> Enemies { get; } = new();

		/// <summary>
		/// 横線を生成する
		/// </summary>
		public void CreateYokoLine(AmidaLineSegment lineSegment)
		{
			CreateLineSegmentObject(lineSegment, yokoLineParent);
		}
		
		/// <summary>
		/// ステージ生成
		/// </summary>
		public void CreateNewStage(AmidaLadder ladder, List<int> starLineIndices, List<int> enemyLineIndices)
		{
			foreach (var line in ladder.TateLines)
			{
				CreateLineSegmentObject(line, tateLineParent);
			}

			foreach (var line in ladder.YokoLines)
			{
				CreateYokoLine(line);
			}
			
			foreach (var starLineIndex in starLineIndices)
			{
				var star = Object.Instantiate(amidaStarPrefab, canvas.transform);
				star.SetPosition(ladder.TateLines[starLineIndex].End, starLineIndex);
				Stars.Add(star);
			}

			foreach (var enemyLineIndex in enemyLineIndices)
			{
				var enemy = Object.Instantiate(amidaEnemyPrefab, canvas.transform);
				enemy.SetPosition(ladder.TateLines[enemyLineIndex].End, enemyLineIndex);
				Enemies.Add(enemy);
			}
		}

		public AmidaPlayerObject[] CreatePlayerObjects(AmidaLadder ladder, List<int> amidaPlayerIndices)
		{
			var playerObjects = new List<AmidaPlayerObject>();
			foreach (var tateLineIndex in amidaPlayerIndices)
			{
				var newPlayerObject = Object.Instantiate(amidaPlayerPrefab, canvas.transform);
				playerObjects.Add(newPlayerObject);
				var startLine = ladder.TateLines[tateLineIndex];
				AmidaPlayerPointData pointDataData = new AmidaPlayerPointData
				{
					Position = startLine.Start,
					Direction = startLine.StoE,
					CurrentLine = startLine,
					TargetPoint = startLine.End,
				};
				newPlayerObject.SetPointData(pointDataData);
			}

			return playerObjects.ToArray();
		}

		/// <summary>
		/// ステージ上に配置したオブジェクトを全て削除する
		/// </summary>
		public void ResetStageObjects()
		{
			// 縦線を全削除
			foreach (Transform child in tateLineParent)
			{
				Object.Destroy(child.gameObject);
			}
			
			// 横線を全削除
			foreach (Transform child in yokoLineParent)
			{
				Object.Destroy(child.gameObject);
			}

			// スターを全削除
			foreach (var star in Stars)
			{
				star.Destroy();
			}
			Stars.Clear();

			// エネミーを全削除
			foreach (var enemy in Enemies)
			{
				enemy.Destroy();
			}
			Enemies.Clear();
		}
		
		/// <summary>
		/// 指定した線分を生成する
		/// </summary>
		private void CreateLineSegmentObject(AmidaLineSegment lineSegment, Transform parent)
		{
			var line = Object.Instantiate(lineTemplate, parent);
			var startPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.Start);
			var endPointScreenSpace = mainCamera.ScreenToWorldPoint(lineSegment.End);

			line.SetPosition(0, new(startPointScreenSpace.x, startPointScreenSpace.y, 0));
			line.SetPosition(1, new(endPointScreenSpace.x, endPointScreenSpace.y, 0));
		}
	}
}