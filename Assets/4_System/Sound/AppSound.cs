using UnityEngine;
using UnityEngine.SceneManagement;

public class AppSound : MonoBehaviour
{

    // === 외부 파라미터 ======================================
    public static AppSound instance = null;

    // 배경음
    [System.NonSerialized]
    public audiomanager fm;



    [System.NonSerialized]
    public AudioSource BGM_STAGE;
    [System.NonSerialized]
    public AudioSource BGM_MENU;


    // 효과음
    [System.NonSerialized]
    public AudioSource SE_ITEM_KNIFE;
    [System.NonSerialized]
    public AudioSource SE_ITEM_MONEY;
    [System.NonSerialized]
    public AudioSource SE_MENU_BUTTON;
    [System.NonSerialized]
    public AudioSource SE_MISSION_FAILURE;
    [System.NonSerialized]
    public AudioSource SE_MISSION_SUCCESS;


    // === 내부 파라미터 ======================================
    string sceneName = "non";
    public float SoundBGMVolume = 0.7f;
    public float SoundSEVolume = 1.0f;

    // === 코드 =============================================
    void Start()
    {
        // 사운드
        fm = GameObject.Find("audioManager").GetComponent<audiomanager>();

        // 배경음
        fm.CreateGroup("BGM");
        fm.SoundFolder = "Sounds/";
        BGM_STAGE = fm.LoadResourcesSound("BGM", "Ingame_BGM");
        BGM_MENU = fm.LoadResourcesSound("BGM", "Menu_BGM");

        // 효과음
        fm.CreateGroup("SE");
        fm.SoundFolder = "Sounds/";

        SE_ITEM_KNIFE = fm.LoadResourcesSound("SE", "Item_knife");
        SE_ITEM_MONEY = fm.LoadResourcesSound("SE", "Item_Money");
        SE_MENU_BUTTON = fm.LoadResourcesSound("SE", "Menu+button");
        SE_MISSION_FAILURE = fm.LoadResourcesSound("SE", "Mission_Failure");
        SE_MISSION_SUCCESS = fm.LoadResourcesSound("SE", "Mission_Success");

        instance = this;
    }

    void Update()
    {
        // 씬이 바뀌었는지 검사
		if (sceneName != SceneManager.GetActiveScene().name)
        {
			if(IsSameMenuScene())return;
			if (sceneName == "non" ||sceneName == "Logo" || sceneName =="SelectStage" || sceneName=="Briefing")
				sceneName = "Menu";
			else
            	sceneName = SceneManager.GetActiveScene().name;
            Debug.Log(sceneName);
			Debug.Log(SceneManager.GetActiveScene().name);
            // 볼륨 설정
            fm.SetVolume("BGM", SoundBGMVolume);
            fm.SetVolume("SE", SoundSEVolume);

            // 배경음 재생

            if (sceneName == "Menu")
            {
                //fm.Stop ("BGM");
                fm.FadeOutVolumeGroup("BGM", BGM_MENU, 0.0f, 1.0f, false);
                fm.FadeInVolume(BGM_MENU, SoundBGMVolume, 1.0f, true);
                BGM_MENU.loop = true;
                BGM_MENU.Play();
            }
            else
            if (sceneName == "InGame")
            {
				Debug.Log("인게임 ");
                //fm.Stop("BGM");
                fm.FadeOutVolumeGroup("BGM", BGM_STAGE, 0.0f, 1.0f, false);
                fm.FadeInVolume(BGM_STAGE, SoundBGMVolume, 1.0f, true);
                BGM_STAGE.loop = true;
                BGM_STAGE.Play();
            }
        }
    }

    bool IsSameMenuScene()
    {
		string thisSceneName = SceneManager.GetActiveScene().name;
		if (sceneName == "Menu"&&(thisSceneName == "Logo" || thisSceneName =="SelectStage" || thisSceneName=="Briefing"))
		{
			return true;
        }else
			return false;
    }
}
