using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using MiniJSON;
using UnityEngine.Networking;
using UniRx.Async;

public class Main : MonoBehaviour
{
	// Start is called before the first frame update
	async void Start()
	{
		Scenario s;
		StartCoroutine(Run((c) => s = c));

		//var scenario = await AsyncRun();
	}

	private async UniTask<Scenario> AsyncRun()
	{
		var request = UnityWebRequest.Get($"{URL}?sheetName={_sheetName}");

		await request.SendWebRequest();

		var text = request.downloadHandler.text;
		Debug.Log(text);

		var json = (List<object>)Json.Deserialize(text);

		Debug.Log(json);

		string path = "Scenarios/" + _sheetName;
		string fullPath = "Assets/Resources/" + path + ".asset";

		if (json == null)
		{
			Debug.LogError(text);
		}
		else
		{
			// ファイルを取得
			var load = Resources.LoadAsync<Scenario>(path);

			await load;

			Scenario scenario = load.asset as Scenario;

			if (!scenario)
			{
				// 存在しない場合作成
				scenario = CreateScriptableObject<Scenario>(fullPath);
			}
			else
			{
				scenario.ReSet();
			}

			foreach (var data in json)
			{
				var dic = data as Dictionary<string, object>;
				scenario.charas.Add(dic["character"] as string);
				scenario.texts.Add(dic["contents"] as string);
			}

			// ScriptableObjectのEditor編集を無効
			//scenario.hideFlags = HideFlags.NotEditable;
			// テキスト設定
			Debug.Log("complete.");

			return scenario;
		}

		return null;
	}


	const string URL = "https://script.google.com/macros/s/AKfycbyt_Er9hHJgVcqV8-kfFDb2l-33pB847JMpqrHA2PlRpmlJh80/exec";
	string _sheetName = "Sheet1";

	IEnumerator Run(System.Action<Scenario> callback = null)
	{
		var request = UnityWebRequest.Get($"{URL}?sheetName={_sheetName}");

		yield return request.SendWebRequest();

		Debug.Log(request.downloadHandler.text);

		string path = "Scenarios/" + _sheetName;
		string fullPath = "Assets/Resources/" + path + ".asset";
		var json = (List<object>)Json.Deserialize(request.downloadHandler.text);

		Debug.Log(json);

		if (json == null)
		{
			Debug.LogError(request.downloadHandler.text);
		}
		else
		{
			// ファイルを取得
			Scenario scenario = null;
			scenario = Resources.Load<Scenario>(path);

			if (!scenario)
			{
				// 存在しない場合作成
				scenario = CreateScriptableObject<Scenario>(fullPath);
			}
			else
			{
				scenario.ReSet();
			}

			foreach (var data in json)
			{
				var dic = data as Dictionary<string, object>;
				scenario.charas.Add(dic["character"] as string);
				scenario.texts.Add(dic["contents"] as string);
			}

			// ScriptableObjectのEditor編集を無効
			//scenario.hideFlags = HideFlags.NotEditable;
			// テキスト設定
			Debug.Log("complete.");

			if (callback != null) callback.Invoke(scenario);
		}
	}

	T CreateScriptableObject<T>(string output) where T : ScriptableObject
	{
		var res = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset((ScriptableObject)res, output);
		return res;
	}
}