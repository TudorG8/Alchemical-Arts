using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlchemicalArts.Shared.Scopes
{
	public class SceneLoadingScope : IAsyncDisposable
	{
		private readonly Scene scene;


		private SceneLoadingScope(Scene scene)
		{
			this.scene = scene;
		}


		public static async Awaitable<SceneLoadingScope> Create(string scenePath)
		{
			await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

			var scene = SceneManager.GetSceneByName(scenePath);
			SceneManager.SetActiveScene(scene);
			
			return new SceneLoadingScope(scene);
		}

		public async ValueTask DisposeAsync()
		{
			await SceneManager.UnloadSceneAsync(scene);
		}
	}
}