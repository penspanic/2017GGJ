using UnityEngine;
using UnityEngine.SceneManagement;

public class AppSound : MonoBehaviour {

	// === 외부 파라미터 ======================================
	public static AppSound instance = null;

	// 배경음
	[System.NonSerialized] public audiomanager fm;
	
	
	
	[System.NonSerialized] public AudioSource BGM_STAGE;
	[System.NonSerialized] public AudioSource BGM_SELECT;


	// 효과음
	[System.NonSerialized] public AudioSource SE_TOUCH_1;
	[System.NonSerialized] public AudioSource SE_COMPLETE;
	

	// === 내부 파라미터 ======================================
	string sceneName = "non";
    public float SoundBGMVolume = 0.7f;
    public float SoundSEVolume = 1.0f;

	// === 코드 =============================================
	void Start () {
		// 사운드
		fm = GameObject.Find("audioManager").GetComponent<audiomanager>();

		// 배경음
		fm.CreateGroup("BGM");
		fm.SoundFolder = "Sounds/";
		BGM_STAGE 				= fm.LoadResourcesSound("BGM","bg1");
	/*	BGM_STAGE	 	= fm.LoadResourcesSound("BGM","battle_1");
		BGM_GAMEOVER 				= fm.LoadResourcesSound("BGM","gameover_1");
	*/	

		// 효과음
		fm.CreateGroup("SE");
		fm.SoundFolder = "Sounds/";

		SE_TOUCH_1 				= fm.LoadResourcesSound("SE","UItouch");
		SE_COMPLETE				= fm.LoadResourcesSound("SE","chartolocation");
		

		instance = this;
	}

	void Update() {
		// 씬이 바뀌었는지 검사
		if (sceneName != SceneManager.GetActiveScene().name) {
			sceneName = SceneManager.GetActiveScene().name;

			// 볼륨 설정
			fm.SetVolume("BGM",SoundBGMVolume);
			fm.SetVolume("SE" ,SoundSEVolume);

			// 배경음 재생
			
			if (sceneName == "Ingame") {
				//fm.Stop ("BGM");
				fm.FadeOutVolumeGroup("BGM",BGM_STAGE,0.0f,1.0f,false);
				fm.FadeInVolume(BGM_STAGE,SoundBGMVolume,1.0f,true);
				BGM_STAGE.loop = true;
				BGM_STAGE.Play();
			}
		}
	}
}
