using System.Linq;
using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// ゲーム起動時の設定
	/// </summary>
	public class GameLaunchSettings
	{
		/// <summary>
		/// ステージ上を動くプレイヤーは、最初何個出現するか？
		/// </summary>
		public int PlayerCount { get; private set; }
		
		/// <summary>
		/// 目標となるスターは最大何個か？
		/// </summary>
		public int StarCount { get; private set; }
		
		/// <summary>
		/// プレイヤーは何秒間隔あけて動きだすか？
		/// </summary>
		public float DelayedSecond { get; private set; }
		
		/// <summary>
		/// ゲームスピード（大きいほど早い）
		/// </summary>
		public int GameSpeed { get; private set; }


		private GameLaunchSettings(int playerCount, int starCount, float delayedSecond, int gameSpeed)
		{
			PlayerCount = playerCount;
			StarCount = starCount;
			DelayedSecond = delayedSecond;
			GameSpeed = gameSpeed;
		}

		private static readonly GameLaunchSettings[] LowLevelSettings =
		{
			new(1, 1, 2, 1),
			new(2, 2, 2, 1),
			new(1, 1, 1.5f, 2),
			new(2, 2, 1, 2),
			new(3, 2, 1f, 1),
			new(3, 2, 1f, 1),
			new(3, 2, 1f, 2),
		};

		private static readonly AmidaLineSegment[][] InitialYokoLines =
		{
			new [] { CreateClipped01(500, 500), CreateClipped12(400, 400), CreateClipped23(500, 500) },
			new [] { CreateClipped01(400, 400), CreateClipped12(500, 500), CreateClipped23(400, 400) },
			new [] { CreateClipped01(500, 400), CreateClipped12(450, 450), CreateClipped12(350, 350), CreateClipped23(400, 500) },
			new [] { CreateClipped01(500, 500), CreateClipped01(400, 400), CreateClipped12(450, 450), CreateClipped12(350, 350), CreateClipped23(500, 500), CreateClipped23(400, 400) },
		};

		private static AmidaLineSegment CreateClipped01(float leftY, float rightY) => CreateClippedLine(leftY, rightY, 0, 1);
		private static AmidaLineSegment CreateClipped12(float leftY, float rightY) => CreateClippedLine(leftY, rightY, 1, 2);
		private static AmidaLineSegment CreateClipped23(float leftY, float rightY) => CreateClippedLine(leftY, rightY, 2, 3);
			
		private static AmidaLineSegment CreateClippedLine(float leftY, float rightY, int leftIndex, int rightIndex)
		{
			float[] tateXs = {75, 235, 395, 555};
			return new AmidaLineSegment(new Vector2(tateXs[leftIndex], leftY), new Vector2(tateXs[rightIndex], rightY));
		}
		
		/// <summary>
		/// ステージ番号から、ゲーム設定を返す
		/// </summary>
		/// <param name="stageNumber">0始まり</param>
		public static (GameLaunchSettings, AmidaLineSegment[]) GetSettingsByStageNumber(int stageNumber)
		{
			// InitialYokoLinesからランダムに1つ選ぶ
			int yokoLinesIndex = Random.Range(0, InitialYokoLines.Length);
			var yokoLines = InitialYokoLines[yokoLinesIndex];
			Debug.Log($"{yokoLinesIndex}の横線を選択");
			
			// 範囲外なら末尾を返す
			if (stageNumber >= LowLevelSettings.Length)
			{
				return (LowLevelSettings.Last(), yokoLines);
			}
			
			return (LowLevelSettings[stageNumber], yokoLines);
		}
	}
}