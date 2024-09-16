using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Amidada
{
	public class GameUIManager : MonoBehaviour
	{
		[SerializeField] private Image speedUpImage;
		[SerializeField] private Color redColor;

		[SerializeField] private Image bigPointImage;
		[SerializeField] private TextMeshProUGUI bigPointText;
		[SerializeField] private RectTransform bigPointImageRect;
		[SerializeField] private Image[] singlePointImages;

		[SerializeField] private TextMeshProUGUI gameInfoText;

		public void RegisterGameEvents(AmidaGameSystem gameSystem)
		{
			ResetUI();
			
			// IsSpeedUpがtrueなら赤色。falseなら白色のまま
			gameSystem.IsSpeedUp
				.Subscribe(isSpeedUp =>
				{
					speedUpImage.color = isSpeedUp ? redColor : Color.white;
				}).AddTo(this);
			
			gameSystem.GamePoint
				.Where(x => x % 5 != 0) // 5で割り切れないときだけ、ここで表示制御
				.Subscribe(x =>
				{
					// 5で割ったあまりの数だけsinglePointImagesを表示する
					for (int i = 0; i < singlePointImages.Length; i++)
					{
						singlePointImages[i].gameObject.SetActive(i < x % 5);
					}
				}).AddTo(this);
			
			gameSystem.State
				.Where(x => x == AmidaGameSystem.GameState.ReadyToPlay)
				.Subscribe(_ =>
				{
					gameInfoText.text = "Press Space to Start";
					ResetUI();
				}).AddTo(this);
			
			gameSystem.State
				.Where(x => x == AmidaGameSystem.GameState.Playing)
				.Subscribe(_ => gameInfoText.text = "").AddTo(this);
			
			gameSystem.State
				.Where(x => x == AmidaGameSystem.GameState.GameOver)
				.Subscribe(_ => gameInfoText.text = "Game Over").AddTo(this);
			
			gameSystem.State
				.Where(x => x == AmidaGameSystem.GameState.StageClear)
				.SubscribeAwait(async (_, ct) =>
				{
					gameInfoText.text = "";
					
					// 小ポイントを全部表示
					foreach (var rect in singlePointImages)
					{
						rect.gameObject.SetActive(true);
					}
					
					// 小ポイントを点滅させる
					List<UniTask> handles = new();
					foreach (var image in singlePointImages)
					{
						var handle = LMotion.Create(0f, 1f, 0.3f)
							.WithDelay(0.5f)
							.WithLoops(10, LoopType.Yoyo)
							.BindToColorA(image);
						handles.Add(handle.ToUniTask(ct));
					}

					await handles; // 移動完了を待つ
					
					// 小ポイントは非表示に
					for (int i = 0; i < singlePointImages.Length; i++)
					{
						singlePointImages[i].gameObject.SetActive(false);
					}
					
					// 5ポイントごとの表示を更新する
					bigPointImage.enabled = true;
					bigPointText.text = (gameSystem.GamePoint.CurrentValue).ToString();
					
					gameInfoText.text = "Press Space to Start";
				}).AddTo(this);
			
		}

		private void ResetUI()
		{
			bigPointImage.enabled = false;
			bigPointText.text = "";

			foreach (var rect in singlePointImages)
			{
				rect.gameObject.SetActive(false);
			}
		}
	}
}