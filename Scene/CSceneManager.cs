using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;

namespace CUtility.Scene
{
	//--------------------------------------------------------------------------------------------------------------------
	//	CSceneManager
	//
	//--------------------------------------------------------------------------------------------------------------------

	public class CSceneManager : CSingleton<CSceneManager>
	{
		//--------------------------------------------------------------------------------------------------------------------
		//	변수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [변수]

		private Camera _clearCamera = null;

		private string _curSceneName = "Root";
		public static string curSceneName { get { return Instance._curSceneName; } }

		private CSceneBase _curScene = null;
		public static CSceneBase curScene
		{
			get
			{
				if (null == Instance._curScene || null == Instance._curScene.gameObject)
					Instance._curScene = null;

				return Instance._curScene;
			}
		}

		private CSceneArgsBase _sceneArgs = null;
		public static CSceneArgsBase Arguments
		{ get { var args = Instance._sceneArgs; Instance._sceneArgs = null; return args; } }

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	property
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [property]

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	기본 함수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [초기화]
		//--------------------------------------------------------------------------------------------------------------------
		protected override void init()
		{
			_clearCamera = gameObject.AddComponent<Camera>();
			_clearCamera.clearFlags = CameraClearFlags.SolidColor;
			_clearCamera.backgroundColor = Color.black;
			_clearCamera.cullingMask = 0;
			_clearCamera.orthographic = true;
			_clearCamera.orthographicSize = 5f;
			_clearCamera.farClipPlane = 5f;
			_clearCamera.nearClipPlane = -5f;
			_clearCamera.depth = -100f;
			_clearCamera.renderingPath = RenderingPath.Forward;
			_clearCamera.useOcclusionCulling = false;
			_clearCamera.allowHDR = false;
			_clearCamera.allowMSAA = false;
#if UNITY_2017_3_OR_NEWER
			_clearCamera.allowDynamicResolution = false;
#endif

			_clearCamera.enabled = false;
		}

		#endregion

		#region [업데이트]
		//--------------------------------------------------------------------------------------------------------------------
		/*
		void Update()
		{
		}
		*/

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	내부 함수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region Scene 불러오기 (코루틴)
		//--------------------------------------------------------------------------------------------------------------------
#if USE_AWAIT
		private async Task loadSceneAsync(string targetSceneName, LoadSceneMode loadMode, Action<bool> callbackOrNull)
#else
		private IEnumerator loadSceneAsync(string targetSceneName, LoadSceneMode loadMode, Action<bool> callbackOrNull)
#endif
		{
			_clearCamera.enabled = true;

			bool res = true;
			AsyncOperation async = null;

			var orgSceneName = string.Format("{0}{1}", CRoot.Instance._sceneHeader, _curSceneName/*.ToLower()*/);
			var targetSceneName_full = string.Format("{0}{1}", CRoot.Instance._sceneHeader, targetSceneName/*.ToLower()*/);

			async = SceneManager.LoadSceneAsync(targetSceneName_full, loadMode);

			//twLoadingScreen.Show(true, async, () => twLoadingScreen.Hide());

			do
			{
#if USE_AWAIT
				await CachedYield.WaitForEndOfFrame;
#else
				yield return CachedYield.WaitForEndOfFrame;
#endif
				Debug.LogFormat("{0} : Load scene ({1} -> {2}) ({3:0.0}%)", GetType().ToString(), orgSceneName, targetSceneName_full, async.progress * 100f);
			}
			while (res && !async.isDone);

			var loadedScene = SceneManager.GetSceneByName(targetSceneName_full);

			CSceneBase scene = null;
			if (!res)
			{
				Debug.LogErrorFormat("{0} : Scene 이동 실패 ({1} -> {2})", GetType().ToString(), orgSceneName, targetSceneName_full);
			}
			else
			{
				var type = Assembly.GetAssembly(GetType()).GetType(targetSceneName_full);

				if (null == type)
				{
					Debug.LogErrorFormat("{0} : 존재하지 않는 타입 ({1})", GetType().ToString(), targetSceneName_full);
#if USE_AWAIT
					return;
#else
					yield break;
#endif
				}

				GameObject root = null;
				var gos = loadedScene.GetRootGameObjects();

				foreach (var go in gos)
				{
					if (go.name.Equals("SceneRoot"))
					{
						root = go;
						break;
					}
				}

				if (null == root)
				{
					root = new GameObject("SceneRoot");
					SceneManager.MoveGameObjectToScene(root, loadedScene);
				}

				if (null == (scene = root.GetComponent(type) as CSceneBase))
					scene = root.AddComponent(type) as CSceneBase;
			}

			while (!scene.isLoaded)
			{
				//	Scene 설정이 완료되지 않았음
#if USE_AWAIT
				await CachedYield.WaitForEndOfFrame;
#else
				yield return CachedYield.WaitForEndOfFrame;
#endif
			}

			_clearCamera.enabled = false;

			_curSceneName = targetSceneName;
			_curScene = scene;

			callbackOrNull?.Invoke(res);

			_sceneArgs = null;

			//CScreenFader.FadeBasic(null, Color.black, new Color(0f, 0f, 0f, 0f), 2f, true, null);

			StartCoroutine(scene.Play());
		}
		#endregion

		#region Scene 제거하기 (코루틴)
		//--------------------------------------------------------------------------------------------------------------------
		private IEnumerator unloadSceneAsync(string targetSceneName, Action<bool> callback)
		{
			var targetSceneName_full = string.Format("{0}{1}", CRoot.Instance._sceneHeader, targetSceneName/*.ToLower()*/);

			Debug.LogFormat("{0} : Unload scene ({1})", GetType().ToString(), targetSceneName_full);

			yield return SceneManager.UnloadSceneAsync(targetSceneName);

			if (null != callback)
				callback(true);

			yield break;
		}
		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	정적 함수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [Scene 컨트롤] 불러오기
		//--------------------------------------------------------------------------------------------------------------------
		public static void LoadSceneAsync(string targetSceneName, LoadSceneMode loadMode, CSceneArgsBase argsOrNull, Action<bool> callbackOrNull)
		{
			if (null == argsOrNull)
				Debug.LogWarningFormat("{0} : args is null", Instance.GetType().ToString());

			Instance._sceneArgs = argsOrNull;
#if USE_AWAIT
			Instance.loadSceneAsync(targetSceneName, loadMode, callbackOrNull);
#else
			Instance.StartCoroutine(Instance.loadSceneAsync(targetSceneName, loadMode, callbackOrNull));
#endif
		}
		#endregion

		#region [Scene 컨트롤] 제거
		//--------------------------------------------------------------------------------------------------------------------
		public static void UnloadScene(string sceneName)
		{
			//SceneManager.UnloadScene(sceneName);
			UnloadSceneAsync(sceneName, null);
		}
		#endregion

		#region [Scene 컨트롤] 제거 (콜백)
		//--------------------------------------------------------------------------------------------------------------------
		public static void UnloadSceneAsync(string sceneName, Action<bool> callback)
		{
			Instance.StartCoroutine(Instance.unloadSceneAsync(sceneName, callback));
		}
		#endregion
	}
}
