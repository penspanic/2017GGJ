#define DISABLED


////////////////////////////////////////////////////////////////////////////////
//  
// @module IOS Deploy
// @author Stanislav Osipov (Stan's Assets) 
// @support support@stansassets.com
//
////////////////////////////////////////////////////////////////////////////////


#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;


namespace SA.IOSDeploy {

	[CustomEditor(typeof(ISD_Settings))]
	public class SettingsEditor : UnityEditor.Editor {
		
		private static string newFramework 			= string.Empty;
		private static string NewLibrary 			= string.Empty;
		private static string NewLinkerFlag 		= string.Empty;
		private static string NewCompilerFlag 		= string.Empty;
		private static string NewPlistValueName 	= string.Empty;
		private static string NewLanguage 			= string.Empty;
		private static string NewValueName 			= string.Empty;


		private static GUIContent SdkVersion   = new GUIContent("Plugin Version [?]", "This is Plugin version.  If you have problems or compliments please include this so we know exactly what version to look out for.");

		public override void OnInspectorGUI () {
			GUI.changed = false;
			EditorGUILayout.LabelField("IOS Deploy Settings", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			#if DISABLED
			GUI.enabled = false;
			#endif

			BuildSettings ();
			EditorGUILayout.Space ();
			Frameworks();
			EditorGUILayout.Space();
			Library ();
			EditorGUILayout.Space();
			LinkerFlags();
			EditorGUILayout.Space();
			CompilerFlags();
			EditorGUILayout.Space();
			PlistValues ();
			EditorGUILayout.Space();
			LanguageValues();
			EditorGUILayout.Space();
			AboutGUI();

			if(GUI.changed) {
				DirtyEditor();
			}
		}

		public static void BuildSettings(){
			ISD_Settings.Instance.IsBuildSettingsOpen = EditorGUILayout.Foldout(ISD_Settings.Instance.IsBuildSettingsOpen, "Build Settins");

			if (ISD_Settings.Instance.IsBuildSettingsOpen) {
				
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginVertical (GUI.skin.box);
				EditorGUILayout.BeginHorizontal();
				//EditorGUILayout.LabelField("Bitcode");


				//bool t = ISD_Settings.Instance.enableBitCode;
				ISD_Settings.Instance.enableBitCode = EditorGUILayout.Toggle ("ENABLE_BITCODE" ,ISD_Settings.Instance.enableBitCode);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				ISD_Settings.Instance.enableTestability = EditorGUILayout.Toggle ("ENABLE_TESTABILITY" ,ISD_Settings.Instance.enableTestability);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				ISD_Settings.Instance.generateProfilingCode = EditorGUILayout.Toggle ("GENERATE_PROFILING_CODE" ,ISD_Settings.Instance.generateProfilingCode);	

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical ();
									

//				Dictionary<string, object> dict = ISD_Settings.Instance.buildSettings;
//				Debug.Log ("cound, " + dict.Count);
//				for (int i = 0; i < dict.Count; i++) {
//					Debug.Log ("Pair");
//					EditorGUILayout.BeginVertical (GUI.skin.box);
//					EditorGUILayout.BeginHorizontal();
//					EditorGUILayout.LabelField("Bitcode");
//					if (dict.ContainsKey("ENABLE_BITCODE")) {
//						//EditorGUILayout.Toggle window = (EditorGUILayout.Toggle)EditorWindow.GetWindow (typeof(EditorGUILayoutToggle), true, "My Empty Window");
//
//						
//
//					}
//
//					EditorGUILayout.EndHorizontal();
//					EditorGUILayout.EndVertical ();
//				}

				EditorGUI.indentLevel--;
			}

		}

		public static void Frameworks() {
			ISD_Settings.Instance.IsfwSettingOpen = EditorGUILayout.Foldout(ISD_Settings.Instance.IsfwSettingOpen, "Frameworks");

			if(ISD_Settings.Instance.IsfwSettingOpen) {
				if (ISD_Settings.Instance.Frameworks.Count == 0) {

					EditorGUILayout.HelpBox("No Frameworks added", MessageType.None);
				}

				EditorGUI.indentLevel++; {	
					foreach(Framework framework in ISD_Settings.Instance.Frameworks) {
						EditorGUILayout.BeginVertical (GUI.skin.box);

						EditorGUILayout.BeginHorizontal();
						framework.IsOpen = EditorGUILayout.Foldout(framework.IsOpen, framework.Name);
						
						if(framework.IsOptional) {
							EditorGUILayout.LabelField("(Optional)");
						}
						bool ItemWasRemoved = SA.Common.Editor.Tools.SrotingButtons((object) framework, ISD_Settings.Instance.Frameworks);
						if(ItemWasRemoved) {
							return;
						}

						EditorGUILayout.EndHorizontal();
						if(framework.IsOpen) {
							EditorGUI.indentLevel++; {
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField("Optional");
								framework.IsOptional = EditorGUILayout.Toggle (framework.IsOptional);
								EditorGUILayout.EndHorizontal();
							}EditorGUI.indentLevel--;
						}
						EditorGUILayout.EndVertical ();
					}
				} EditorGUI.indentLevel--;
				EditorGUILayout.Space();

				EditorGUILayout.BeginVertical (GUI.skin.box);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Add New Framework", GUILayout.Width(120));
				newFramework = EditorGUILayout.TextField(newFramework);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical ();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();

				if(GUILayout.Button("Add",  GUILayout.Width(100))) {
					if(!ISD_Settings.Instance.ContainsFreamworkWithName(newFramework) && newFramework.Length > 0) {
						Framework f =  new Framework();
						f.Name = newFramework;
						ISD_Settings.Instance.Frameworks.Add(f);
						newFramework = string.Empty;
					}				
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		public static void Library () {
			ISD_Settings.Instance.IsLibSettingOpen = EditorGUILayout.Foldout(ISD_Settings.Instance.IsLibSettingOpen, "Libraries");

			if(ISD_Settings.Instance.IsLibSettingOpen){
				if (ISD_Settings.Instance.Libraries.Count == 0) {
					EditorGUILayout.HelpBox("No Libraries added", MessageType.None);
				}

				EditorGUI.indentLevel++; {
					foreach(Lib lib in ISD_Settings.Instance.Libraries) {	
						EditorGUILayout.BeginVertical (GUI.skin.box);
						
						EditorGUILayout.BeginHorizontal();
						lib.IsOpen = EditorGUILayout.Foldout(lib.IsOpen, lib.Name);
						if(lib.IsOptional) {
							EditorGUILayout.LabelField("(Optional)");
						}
			
						bool ItemWasRemoved = SA.Common.Editor.Tools.SrotingButtons((object) lib, ISD_Settings.Instance.Libraries);
						if(ItemWasRemoved) {
							return;
						}					
						EditorGUILayout.EndHorizontal();
						if(lib.IsOpen) {						
							EditorGUI.indentLevel++; {							
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField("Optional");
								lib.IsOptional = EditorGUILayout.Toggle (lib.IsOptional);
								EditorGUILayout.EndHorizontal();						
							}EditorGUI.indentLevel--;
						}
						EditorGUILayout.EndVertical ();					
					}
				}EditorGUI.indentLevel--;
				
				EditorGUILayout.Space();

				EditorGUILayout.BeginVertical (GUI.skin.box);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Add New Library", GUILayout.Width(120));
				NewLibrary = EditorGUILayout.TextField(NewLibrary);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical ();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("Add",  GUILayout.Width(100))) {
					if(!ISD_Settings.Instance.ContainsLibWithName(NewLibrary) && NewLibrary.Length > 0 ) {
						Lib lib = new Lib();
						lib.Name = NewLibrary;
						ISD_Settings.Instance.Libraries.Add(lib);
						NewLibrary = string.Empty;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}


		public static void LinkerFlags() {
			ISD_Settings.Instance.IslinkerSettingOpne = EditorGUILayout.Foldout(ISD_Settings.Instance.IslinkerSettingOpne, "Linker Flags");
			
			if(ISD_Settings.Instance.IslinkerSettingOpne) {
				if (ISD_Settings.Instance.linkFlags.Count == 0) {				
					EditorGUILayout.HelpBox("No Linker Flags added", MessageType.None);
				}

				foreach(string flasg in ISD_Settings.Instance.linkFlags) {			
					EditorGUILayout.BeginVertical (GUI.skin.box);				
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.SelectableLabel(flasg, GUILayout.Height(18));
					EditorGUILayout.Space();
					
					bool pressed  = GUILayout.Button("x",  EditorStyles.miniButton, GUILayout.Width(20));
					if(pressed) {
						ISD_Settings.Instance.linkFlags.Remove(flasg);
						return;
					}
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.EndVertical ();				
				}

				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField("Add New Flag");
				NewLinkerFlag = EditorGUILayout.TextField(NewLinkerFlag, GUILayout.Width(200));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("Add",  GUILayout.Width(100))) {
					if(!ISD_Settings.Instance.linkFlags.Contains(NewLinkerFlag) && NewLinkerFlag.Length > 0) {
						ISD_Settings.Instance.linkFlags.Add(NewLinkerFlag);
						NewLinkerFlag = string.Empty;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		public static void CompilerFlags() {
			ISD_Settings.Instance.IscompilerSettingsOpen = EditorGUILayout.Foldout(ISD_Settings.Instance.IscompilerSettingsOpen, "Compiler Flags");
			
			if(ISD_Settings.Instance.IscompilerSettingsOpen) {
				if (ISD_Settings.Instance.compileFlags.Count == 0) {
					EditorGUILayout.HelpBox("No Linker Flags added", MessageType.None);
				}

				foreach(string flasg in ISD_Settings.Instance.compileFlags) {
					EditorGUILayout.BeginVertical (GUI.skin.box);
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.SelectableLabel(flasg, GUILayout.Height(18));
					
					EditorGUILayout.Space();
					
					bool pressed  = GUILayout.Button("x",  EditorStyles.miniButton, GUILayout.Width(20));
					if(pressed) {
						ISD_Settings.Instance.compileFlags.Remove(flasg);
						return;
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical ();
				}

				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Add New Flag");
				NewCompilerFlag = EditorGUILayout.TextField(NewCompilerFlag, GUILayout.Width(200));
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.Space();
				
				if(GUILayout.Button("Add",  GUILayout.Width(100))) {
					if(!ISD_Settings.Instance.compileFlags.Contains(NewCompilerFlag) && NewCompilerFlag.Length > 0) {
						ISD_Settings.Instance.compileFlags.Add(NewCompilerFlag);
						NewCompilerFlag = string.Empty;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		public static void PlistValues ()	{
			ISD_Settings.Instance.IsPlistSettingsOpen = EditorGUILayout.Foldout(ISD_Settings.Instance.IsPlistSettingsOpen, "Plist values");
			
			if(ISD_Settings.Instance.IsPlistSettingsOpen) {
				
				if (ISD_Settings.Instance.PlistVariables.Count == 0) {
					EditorGUILayout.HelpBox("No Plist values added", MessageType.None);
				}

				EditorGUI.indentLevel++; {	
					foreach(Variable var in ISD_Settings.Instance.PlistVariables) {
						EditorGUILayout.BeginVertical (GUI.skin.box);
						DrawPlistVariable (var, (object) var, ISD_Settings.Instance.PlistVariables);
						EditorGUILayout.EndVertical ();

						if(!ISD_Settings.Instance.PlistVariables.Contains(var)) {
							return;
						}

					}
					EditorGUILayout.Space();
				} EditorGUI.indentLevel--;



				SA.Common.Editor.Tools.BlockHeader ("Add New Variable");

				EditorGUILayout.BeginVertical (GUI.skin.box);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(" New Variable Name");
				NewPlistValueName = EditorGUILayout.TextField(NewPlistValueName);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("Add",  GUILayout.Width(100))) {
					if (NewPlistValueName.Length > 0) {
						Variable var = new Variable ();
						var.Name = NewPlistValueName;
						ISD_Settings.Instance.AddNewVariable(var);					
					}
					NewPlistValueName = string.Empty;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();
			}
		}


		public static void DrawPlistVariable(Variable var, object valuePointer, IList valueOrigin) {
			EditorGUILayout.BeginHorizontal();

			if(var.Name.Length > 0) {
				var.IsOpen = EditorGUILayout.Foldout(var.IsOpen, var.Name);
				EditorGUILayout.LabelField("(" + var.Type.ToString() +  ")");
			} else {
				var.IsOpen = EditorGUILayout.Foldout(var.IsOpen, var.Type.ToString());
			}



			bool ItemWasRemoved = SA.Common.Editor.Tools.SrotingButtons (valuePointer, valueOrigin);
			if(ItemWasRemoved) {
				ISD_Settings.Instance.RemoveVariable (var, valueOrigin);
				return;
			}
			EditorGUILayout.EndHorizontal();

			if(var.IsOpen) {						
				EditorGUI.indentLevel++; {

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Type");
					if (var.ChildrensIds.Count > 0) {
						GUI.enabled = false;
						var.Type = (PlistValueTypes)EditorGUILayout.EnumPopup (var.Type);
						GUI.enabled = true;
					} else {
						var.Type = (PlistValueTypes)EditorGUILayout.EnumPopup (var.Type);
					}
					EditorGUILayout.EndHorizontal();


					if (var.Type == PlistValueTypes.Array) {
						DrawArrayValues (var);
					} else if (var.Type == PlistValueTypes.Dictionary) {
						DrawDictionaryValues (var);
					} else if (var.Type == PlistValueTypes.Boolean) {
						var.BooleanValue = SA.Common.Editor.Tools.YesNoFiled ("Value", var.BooleanValue);

					} else {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Value");								
						switch(var.Type) {
						case PlistValueTypes.Float:
							var.FloatValue = EditorGUILayout.FloatField(var.FloatValue);
							break;									
						case PlistValueTypes.Integer:
							var.IntegerValue = EditorGUILayout.IntField (var.IntegerValue);
							break;									
						case PlistValueTypes.String:
							var.StringValue = EditorGUILayout.TextField (var.StringValue);
							break;
						}
						EditorGUILayout.EndHorizontal();
					}

				} EditorGUI.indentLevel--;
			}

		}


		public static void DrawArrayValues (Variable var) {


			var.IsListOpen = EditorGUILayout.Foldout (var.IsListOpen, "Array Values (" + var.ChildrensIds.Count + ")");

			if (var.IsListOpen) {		

				EditorGUI.indentLevel++; {
					
					foreach	(string uniqueKey in var.ChildrensIds) {
						Variable v = ISD_Settings.Instance.getVariableByKey(uniqueKey);
						DrawPlistVariable (v, uniqueKey, var.ChildrensIds);

						if(!var.ChildrensIds.Contains(uniqueKey)) {
							return;
						}
					}
					 

					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.Space ();
					if (GUILayout.Button ("Add Value", GUILayout.Width (100))) {
						Variable newVar = new Variable();

						var.AddChild (newVar);
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.Space ();

				} EditorGUI.indentLevel--;
			} 
		}
			
		public static void DrawDictionaryValues (Variable var) {
			var.IsListOpen = EditorGUILayout.Foldout (var.IsListOpen, "Dictionary Values");

			if (var.IsListOpen) {

				EditorGUI.indentLevel++; {
					
					foreach	(string uniqueKey in var.ChildrensIds) {
						Variable v = ISD_Settings.Instance.getVariableByKey(uniqueKey);
						DrawPlistVariable (v, uniqueKey, var.ChildrensIds);

						if(!var.ChildrensIds.Contains(uniqueKey)) {
							return;
						}
					}


					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.PrefixLabel ("New Key");
					NewValueName = EditorGUILayout.TextField (NewValueName);

					if (GUILayout.Button ("Add", GUILayout.Width (50))) {
						if (NewValueName.Length > 0) {
							Variable v = new Variable ();
							v.Name = NewValueName;
							var.AddChild (v);									
						}
					}

					EditorGUILayout.EndHorizontal ();
				} EditorGUI.indentLevel--;
			} 
				
		}




		public static void LanguageValues ()	{
			ISD_Settings.Instance.IsLanguageSettingOpen = EditorGUILayout.Foldout(ISD_Settings.Instance.IsLanguageSettingOpen, "Languages");

			if(ISD_Settings.Instance.IsLanguageSettingOpen)	 {
				if (ISD_Settings.Instance.langFolders.Count == 0)	{
					EditorGUILayout.HelpBox("No Languages added", MessageType.None);
				}

				foreach(string lang in ISD_Settings.Instance.langFolders) 	{
					EditorGUILayout.BeginVertical (GUI.skin.box);
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.SelectableLabel(lang, GUILayout.Height(18));
					EditorGUILayout.Space();

					bool pressed  = GUILayout.Button("x",  EditorStyles.miniButton, GUILayout.Width(20));
					if(pressed) 	{
						ISD_Settings.Instance.langFolders.Remove(lang);
						return;
					}

					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical ();
				}
				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Add New Language");
				NewLanguage = EditorGUILayout.TextField(NewLanguage, GUILayout.Width(200));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();			
				EditorGUILayout.Space();

				if(GUILayout.Button("Add",  GUILayout.Width(100)))	{
					if(!ISD_Settings.Instance.langFolders.Contains(NewLanguage) && NewLanguage.Length > 0)	{
						ISD_Settings.Instance.langFolders.Add(NewLanguage);
						NewLanguage = string.Empty;
					}				
				}
				EditorGUILayout.EndHorizontal();
			}
		}




	
		public static void AboutGUI() {
			GUI.enabled = true;
			EditorGUILayout.HelpBox("About the Plugin", MessageType.None);
			EditorGUILayout.Space();
		

			SA.Common.Editor.Tools.SelectableLabelField(SdkVersion,   ISD_Settings.VERSION_NUMBER);
			SA.Common.Editor.Tools.SupportMail();

			SA.Common.Editor.Tools.DrawSALogo();
			#if DISABLED
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Note: This version of IOS Deploy designed for Stan's Assets");
			EditorGUILayout.LabelField("plugins internal use only. If you want to use IOS Deploy  ");
			EditorGUILayout.LabelField("for your project needs, please, ");
			EditorGUILayout.LabelField("purchase a copy of IOS Deploy plugin.");
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Documentation",  GUILayout.Width(150))) {
				Application.OpenURL("https://goo.gl/sOJFXJ");
			}
			
			if(GUILayout.Button("Purchase",  GUILayout.Width(150))) {
				Application.OpenURL("https://goo.gl/Nqbuuv");
			}		
			EditorGUILayout.EndHorizontal();
			#endif
		}
		


		private static void DirtyEditor() {
			#if UNITY_EDITOR
			EditorUtility.SetDirty(ISD_Settings.Instance);
			#endif
		}
	}

}
#endif