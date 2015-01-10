
//Code originally taken from FriendsSmash Demo: https://github.com/fbsamples/friendsmash-unity

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using System;

public class FacebookLogin : MonoBehaviour {
	private static List<object> friends = null;
	private static Dictionary<string, string> profile = null;
	private static string userName = null;
	private static Texture userTexture = null;

	public static FacebookLogin instance = null;
	void Awake() {
		if (instance) {
			Destroy (gameObject);
		} else {
			FB.Init(SetInit, OnHideUnity);
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
	}
	
	private void SetInit() {
		if (FB.IsLoggedIn) {
			OnLoggedIn();
		}
	}
	
	private void OnHideUnity(bool isGameShown) {
		if (!isGameShown) {
			Time.timeScale = 0;
		} else {
			Time.timeScale = 1;
		}
	}
	
	void LoginCallback(FBResult result) {
		if (FB.IsLoggedIn) {
			OnLoggedIn();
		}
	}
	string queryProfileString = "/v2.0/me?fields=id,first_name,friends.limit(100).fields(first_name,id,picture.width(128).height(128)),invitable_friends.limit(100).fields(first_name,id,picture.width(128).height(128))";
	void OnLoggedIn() {
		FB.API(queryProfileString, Facebook.HttpMethod.GET, APICallback);
		LoadPictureAPI(FacebookUtils.GetPictureURL("me", 128, 128),MyPictureCallback);
	}
	
	void APICallback(FBResult result) {
		if (result.Error != null) {
			FB.API(queryProfileString, Facebook.HttpMethod.GET, APICallback);
			return;
		}
		
		profile = FacebookUtils.DeserializeJSONProfile(result.Text);
		userName = profile["first_name"];
		friends = FacebookUtils.DeserializeJSONFriends(result.Text);
	}
	
	public void OnLoginSelected() {
		if (!FB.IsLoggedIn && FB.IsInitialized) {
			FB.Login ("public_profile,user_friends,email,publish_actions", LoginCallback);
		}
		
		if (!FB.IsInitialized) {
			Debug.Log("Not Init");
		}
	}
	
	static void CreateDisplayPicture () {
		if (FB.IsLoggedIn) {
			string panelText = "Welcome ";
			panelText += (!string.IsNullOrEmpty (userName)) ? string.Format ("{0}!", userName) : "Smasher!";
			if (userTexture != null)
				GUI.DrawTexture ((new Rect (8, 10, 150, 150)), userTexture);
		}
	}
	
	void OnGUI() {
		CreateDisplayPicture ();
	}
	
	void MyPictureCallback(Texture texture) {
		if (texture ==  null) {
			LoadPictureAPI(FacebookUtils.GetPictureURL("me", 128, 128),MyPictureCallback);
			return;
		}
		
		userTexture = texture;
	}
	
	delegate void LoadPictureCallback (Texture texture);
	IEnumerator LoadPictureEnumerator(string url, LoadPictureCallback callback) {
		WWW www = new WWW(url);
		yield return www;
		callback(www.texture);
	}
	void LoadPictureAPI (string url, LoadPictureCallback callback) {
		FB.API(url,Facebook.HttpMethod.GET,result => {
			if (result.Error != null) {
				return;
			}
			
			var imageUrl = FacebookUtils.DeserializePictureURLString(result.Text);
			StartCoroutine(LoadPictureEnumerator(imageUrl,callback));
		});
	}
}