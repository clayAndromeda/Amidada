using System.Linq;

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

		public GameLaunchSettings(int playerCount, int starCount, float delayedSecond, int gameSpeed)
		{
			PlayerCount = playerCount;
			StarCount = starCount;
			DelayedSecond = delayedSecond;
			GameSpeed = gameSpeed;
		}

		private static readonly GameLaunchSettings[] LowLevelSettings =
		{
			new(1, 2, 2, 1),
			new(2, 3, 2, 1),
			new(2, 2, 2, 1),
			new(3, 3, 1.5f, 1),
			new(3, 2, 1.5f, 1),
			new(3, 2, 1.5f, 2),
		};
		
		/// <summary>
		/// ステージ番号から、ゲーム設定を返す
		/// </summary>
		/// <param name="stageNumber">0始まり</param>
		/// <returns></returns>
		public static GameLaunchSettings GetSettingsByStageNumber(int stageNumber)
		{
			// 範囲外なら末尾を返す
			if (stageNumber >= LowLevelSettings.Length)
			{
				return LowLevelSettings.Last();
			}
			
			return LowLevelSettings[stageNumber];
		}
	}
}