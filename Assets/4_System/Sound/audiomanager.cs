﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class audiomanager : MonoBehaviour {


	// === 외부 파라미터(Inspector 표시) =====================
 
	public bool 	DebugLog 				= false;
	public bool 	DontDestroyObjectOnLoad = true;
	public string 	SoundFolder 			= "";

	// === 내부 파라미터 ======================================
	const  string 	soundGroup 		= "soundGroup_";

	// === 코드(Monobehaviour 기본 기능 구현) ================
	void Awake () {
	/*	if (DontDestroyObjectOnLoad) {
			DontDestroyOnLoad (this);
		}*/
	}
	
	// === 코드(리소스 관리 구현) =========================
	public bool CreateGroup(string name) {
		GameObject go = new GameObject ();
		go.name = soundGroup + name;
		go.transform.parent = transform;

		return false;
	}

	public GameObject GetGroup(string name) {
		return GameObject.Find (soundGroup + name);
	}

	public AudioSource LoadResourcesSound(string groupName,string fileName) {

		GameObject 	goSound 	= transform.FindChild (soundGroup + groupName).gameObject;
		AudioSource audioSource = goSound.AddComponent<AudioSource> ();
		audioSource.playOnAwake = false;

		AudioClip 	audioClip 	= Resources.Load (SoundFolder + fileName,typeof(AudioClip)) as AudioClip;
		audioSource.clip = audioClip;

		return audioSource;
	}
    
	public AudioSource FindAudioSource(string groupName,string soundName) {
		GameObject goSound = transform.FindChild (soundGroup + groupName).gameObject;
		AudioSource[] audioSourceList = goSound.GetComponents<AudioSource> ();
		
		foreach (AudioSource audioSource in audioSourceList) {
			if (audioSource.clip.name == soundName) {
				return audioSource;
			}
		}

		return  null;
	}

	public AudioSource[] FindAudioSource(string groupName) {
		GameObject goSound = transform.FindChild (soundGroup + groupName).gameObject;
		return goSound.GetComponents<AudioSource> ();
	}

	public void Play(AudioSource audioSource,bool loop) {
		audioSource.loop = loop;
		audioSource.Play ();
	}

	// === 코드(재생 처리 구현) =============================
	public void Play(string groupName,string soundName,bool loop) {
		AudioSource audioSource = FindAudioSource (groupName, soundName);
		if (audioSource) {
			Play (audioSource,loop);
		}
	}

	public void PlayDontOverride(AudioSource audioSource,bool loop) {
		if (!audioSource.isPlaying) {
			audioSource.loop = loop;
			audioSource.Play ();
		}
	}
	
	public void PlayDontOverride(string groupName,string soundName,bool loop) {
		AudioSource audioSource = FindAudioSource (groupName, soundName);
		if (audioSource) {
			PlayDontOverride (audioSource,loop);
		}
	}
	
	public void PlayOneShot(AudioSource audioSource) {
		audioSource.PlayOneShot (audioSource.clip);
	}

	public void PlayOneShot(string groupName,string soundName) {
		AudioSource audioSource = FindAudioSource(groupName,soundName);
		if (audioSource) {
			PlayOneShot (audioSource);
		}
	}

	public void Stop(AudioSource audioSource) {
		audioSource.Stop();
	}

	public void Stop(string groupName,string soundName) {
		AudioSource audioSource = FindAudioSource(groupName,soundName);
		if (audioSource) {
			Stop (audioSource);
		}
	}

	public void Stop(string groupName) {
		AudioSource[] audioSourceList = FindAudioSource(groupName);
		foreach(AudioSource audioSource in audioSourceList) {
			Stop (audioSource);
		}
	}
	
	public void StopAllSound() {
		AudioSource[] audios = transform.GetComponentsInChildren<AudioSource>();
		foreach(AudioSource audio in audios) {
			audio.Stop();
		}
	}

	// === 코드(볼륨 처리 구현) ========================
	public float GetVolume(AudioSource audioSource) {
		return audioSource.volume;
	}
	
	public float GetVolume(string groupName,string soundName) {
		AudioSource audioSource = FindAudioSource(groupName,soundName);
		if (audioSource) {
			return GetVolume (audioSource);
		}
		return 0.0f;
	}
	
	public void SetVolume(AudioSource audioSource,float vol) {
		audioSource.volume = vol;
	}
	
	public void SetVolume(string groupName,string soundName,float vol) {
		AudioSource audioSource = FindAudioSource(groupName,soundName);
		if (audioSource) {
			SetVolume (audioSource,vol);
		}
	}

	public void SetVolume(string groupName,float vol) {
		GameObject go = GetGroup (groupName);
		AudioSource[] audioSourceList = go.GetComponents<AudioSource> ();

		foreach(AudioSource audioSource in audioSourceList) {
			SetVolume(audioSource,vol);
		}
	}

	// === 코드(페이드 처리 구현) ==========================
	class Fade {
		public AudioSource 	fadeAudio;
		public float 		targetV;
		public float 		dir;
		public float 		time;
		public float 		vmin,vmax;
		public Fade(AudioSource a,float v,float d,float t) {
			fadeAudio 	= a;
			targetV		= v;
			dir 		= d;
			time		= t;
			if (dir < 0.0f) {
				vmin = v;
				vmax = 1.0f;
			} else {
				vmin = 0.0f;
				vmax = v;
			}
		}
	};
	List<Fade> fadeStackList = new List<Fade>();

	public void FadeInVolume(AudioSource audioSource,float v,float t,bool init) {
		if (audioSource.volume < 1.0f && audioSource.isPlaying) {
			if (fadeStackList.Count <= 0) {
				InvokeRepeating ("SoundFade",0.0f,0.02f);
			}
			if (init) {
				audioSource.volume = 0.0f;
			}
			fadeStackList.Add (new Fade (audioSource,v,+1.0f,t));
		}
	}
	public void FadeOutVolume(AudioSource audioSource,float v,float t,bool init) {
		if (audioSource.volume > 0.0f && audioSource.isPlaying) {
			if (fadeStackList.Count <= 0) {
				InvokeRepeating ("SoundFade",0.0f,0.02f);
			}
			if (init) {
				audioSource.volume = 1.0f;
			}
			fadeStackList.Add (new Fade (audioSource,v,-1.0f,t));
		}
	}

	public void FadeOutVolumeGroup(string groupName,AudioSource playAudioSource,float v,float t,bool init) {
		GameObject go = GetGroup (groupName);
		AudioSource[] audioSourceList = go.GetComponents<AudioSource> ();
		
		foreach(AudioSource audioSource in audioSourceList) {
			if (playAudioSource != audioSource) {
				FadeOutVolume(audioSource,v,t,init);
			}
		}
	}

	public void FadeOutVolumeGroup(string groupName,string soundName,float v,float t,bool init) {
		AudioSource audioSource = FindAudioSource(groupName,soundName);
		if (audioSource) {
			FadeOutVolumeGroup(groupName,audioSource,v,t,init);
		}
	}

	public void FadeOutVolumeGroup(string groupName,float v,float t,bool init) {
		FadeOutVolumeGroup(groupName,(AudioSource)null,v,t,init);
	}

	void SoundFade() {
		foreach (Fade fade in fadeStackList) {
			float v = fade.fadeAudio.volume + (1.0f * (0.02f / fade.time)) * fade.dir;
			SetVolume (fade.fadeAudio, v);
		}
		for(int i = 0;i < fadeStackList.Count;i ++) {
			if (fadeStackList[i].fadeAudio.volume <= fadeStackList[i].vmin || 
			    fadeStackList[i].fadeAudio.volume >= fadeStackList[i].vmax) {
				if (fadeStackList[i].fadeAudio.volume <= 0.0f) {
					fadeStackList[i].fadeAudio.Stop();
				}
				fadeStackList.Remove(fadeStackList[i]);
			}
		}
		if (fadeStackList.Count <= 0) {
			CancelInvoke ("SoundFade");
		}
	}

	// === 코드(지원 함수 구현) ========================
	public static audiomanager GetInstance(string gameObjectName = "audiomanager" ) {
		GameObject go = GameObject.Find (gameObjectName);
		if (go) {
			return go.GetComponent<audiomanager> ();
		}
		return null;
	}
}

