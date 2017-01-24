using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    internal class h2_Shortcut
    {
        public static Action<string> OnTrigger;

        private static h2_Shortcut _api;

        //----------------------------------- GROUP MAP --------------------------------

        Dictionary<string, h2_AHandler> hndMap;

        //----------------------------------- CHECK --------------------------------

        internal h2_MatchData matchData;

        public static h2_Shortcut Api
        {
            get { return _api ?? (_api = new h2_Shortcut()); }
        }

        internal static h2_AShortcut[] FromStrings(string[] shortcuts)
        {
            /*
			FORMAT : DisplayName, Id, Shortcut
		*/
            var list = new List<h2_AShortcut>();

            for (var i = 0; i < shortcuts.Length; i += 3)
            {
                list.Add(new h2_AShortcut(
                    shortcuts[i],
                    shortcuts[i + 1],
                    shortcuts[i + 2],
                    !string.IsNullOrEmpty(shortcuts[i + 2])
                    ));
            }

            return list.ToArray();
        }

        public void RemoveHandler(string id)
        {
            if (hndMap == null || !hndMap.ContainsKey(id)) return;
            hndMap.Remove(id);
        }

        public void AddHandler(string id, Action<string> callback, Func<h2_AShortcut[]> getShortcuts)
        {
            if (hndMap == null) hndMap = new Dictionary<string, h2_AHandler>();

            h2_AHandler result;
            if (hndMap.TryGetValue(id, out result))
            {
                Debug.LogWarning("Handler id <" + id + "> already existed - overwritting ... ");
                hndMap.Remove(id);
            }

            result = new h2_AHandler
            {
                id = id,
                callback = callback,
                getShortcuts = getShortcuts
            };
            hndMap.Add(id, result);
        }

        public void Check()
        {
            if (hndMap == null) return;

            var e = Event.current;
            if (e.type != EventType.keyUp || e.keyCode == KeyCode.None)
            {
                return;
            }

            if (matchData == null) matchData = new h2_MatchData();
            matchData.Check(e, hndMap);
        }
    }

    internal class h2_MatchData
    {
        const float CHAIN_DURATION = 0.3f;
        public float chainTime;
        public List<h2_AShortcut> comboMatch;
        //public h2_AShortcutMatch match;
        public List<h2_AShortcut> exactMatch;

        public bool hasChain;


        public h2_MatchData()
        {
            exactMatch = new List<h2_AShortcut>();
            comboMatch = new List<h2_AShortcut>();
        }

        void Filter(Dictionary<string, h2_AHandler> hndMap)
        {
            exactMatch.Clear();
            comboMatch.Clear();

            foreach (var item in hndMap)
            {
                item.Value.Append(this);
            }

            // Debug.Log("Filter : " + exactMatch.Count + ":" + comboMatch.Count);
        }

        internal void Check(Event e, Dictionary<string, h2_AHandler> hndMap)
        {
            //var eventStr = CurrentEventSimplify;
            //if (string.IsNullOrEmpty(eventStr)) return;
	        if (e.keyCode == KeyCode.None) return;

            if (hasChain)
            {
                var firstChain = exactMatch.Count > 0 ? exactMatch[0] : comboMatch[0];


                if ((e.control != firstChain.ctrl) || (e.alt != firstChain.alt) || (e.shift != firstChain.shift))
                {
                    //Debug.Log("Not matched : " + e.control + ":" + Match.Ctrl + ":" + e.alt + ":" + Match.Alt + ":" + e.shift + ":" + Match.Shift);
	                CancelMatch();
                }
                else
                {
                    var listCombo = new List<h2_AShortcut>();
                    for (var i = 0; i < comboMatch.Count; i++)
                    {
                        if (comboMatch[i].chain == e.keyCode)
                        {
                            listCombo.Add(comboMatch[i]);
                        }
                    }

                    if (listCombo.Count > 0)
                    {
                        Trigger(listCombo);
                    }
                    else
                    {
                        CancelMatch();
                    }
                }
            }


            Filter(hndMap);
            hasChain = comboMatch.Count > 0;

            if (hasChain)
            {
                chainTime = Time.realtimeSinceStartup;
                Event.current.Use();
                EditorApplication.update -= ChainCheck;
                EditorApplication.update += ChainCheck;
                return;
            }

            Trigger(exactMatch);
            Event.current.Use();
        }

        void CancelMatch()
        {
            EditorApplication.update -= ChainCheck;
            hasChain = false;
        }

        void ChainCheck()
        {
            if (Time.realtimeSinceStartup - chainTime < CHAIN_DURATION) return;
            if (exactMatch.Count > 0)
            {
                Trigger(exactMatch);
            }
            else
            {
                CancelMatch();
            }
        }

        void Trigger(List<h2_AShortcut> matchList)
        {
            //Debug.Log("Trigger :: " + matchList.Count);
            EditorApplication.update -= ChainCheck;
            hasChain = false;

            for (var i = 0; i < matchList.Count; i++)
            {
                var m = matchList[i];
                m.handler.callback(m.id);
            }

            if (h2_Shortcut.OnTrigger != null && matchList.Count > 0)
            {
                h2_Shortcut.OnTrigger(matchList[0].GetHelp());
            }

            if (matchList.Count > 1)
            {
                var idList = new List<string>();
                for (var j = 0; j < matchList.Count; j++)
                {
                    var m = matchList[j];
                    idList.Add(m.handler.id + "->" + m.id);
                }
                Debug.LogWarning(matchList.Count + " handlers being found for the same shortcut [ " +
                                 matchList[0].GetHelp() + "]\n" +
                                 string.Join("\n", idList.ToArray())
                    );
            }
        }


        //----------------------------------- UTILS --------------------------------

        //public static string CurrentEvent
        //{
        //    get
        //    {
        //        var e = Event.current;
        //        return GetShortcutValue(e.control, e.alt, e.shift, e.keyCode, KeyCode.None);
        //    }
        //}

        //public static string CurrentEventSimplify

        //{
        //    get
        //    {
        //        var e = Event.current;
        //        var result = GetShortcutValue(e.control, e.alt, e.shift, e.keyCode, KeyCode.None);
        //        if (result == "&" || result == "%" || result == "#" || result == "_") return string.Empty;
        //        return result;
        //    }
        //}

        //private static readonly KeyCode[] _noneList =
        //{
        //    KeyCode.LeftControl,
        //    KeyCode.LeftAlt,
        //    KeyCode.LeftShift,
        //    KeyCode.LeftApple,
        //    KeyCode.RightControl,
        //    KeyCode.RightAlt,
        //    KeyCode.RightShift,
        //    KeyCode.RightApple,
        //};


        //internal static string GetShortcutValue(bool ctrl, bool alt, bool shift, KeyCode code, KeyCode chain)
        //{
        //    if (_noneList.Contains(code)) code = KeyCode.None;

        //    var modifier = (ctrl ? "%" : string.Empty) +
        //                    (alt ? "&" : string.Empty) +
        //                    (shift ? "#" : "");

        //    if (string.IsNullOrEmpty(modifier))
        //    {
        //        //if (code == KeyCode.None) return string.Empty;
        //        modifier = "_";
        //    }

        //    var chainStr = (chain != KeyCode.None)
        //        ? ("+" + GetKeyCodeString(chain))
        //        : string.Empty;

        //    return modifier + GetKeyCodeString(code) + chainStr;
        //}
    }

    internal class h2_AHandler
    {
        internal Action<string> callback;
        internal Func<h2_AShortcut[]> getShortcuts;
        internal string id;

        internal void Append(h2_MatchData data)
        {
            var shortcuts = getShortcuts();
            if (shortcuts == null || shortcuts.Length == 0) return;

            for (var i = 0; i < shortcuts.Length; i++)
            {
                var sc = shortcuts[i];
                if (!sc.enable) continue;

                var m = sc.GetMatch(Event.current);
                if (m == 0) continue; // not matched

                sc.handler = this;

                if (m == 1) // perfect match
                {
                    data.exactMatch.Add(sc);
                }
                else
                {
                    data.comboMatch.Add(sc);
                }
            }
        }
    }

    [Serializable]
    internal class h2_AShortcut
    {
        const string LABEL_INVALID_SHORTCUT = "Invalid Shortcut!";
        static readonly GUIContent LABEL_CTRL = new GUIContent("Ctrl");
        static readonly GUIContent LABEL_ALT = new GUIContent("Alt");
        static readonly GUIContent LABEL_SHIFT = new GUIContent("Shift");
        static readonly Color COLOR_INVALID = new Color32(255, 128, 128, 255);

        static Dictionary<int, string> helpMap;
        public bool alt;
        public KeyCode chain;

        public bool ctrl;
        //internal string uniqueId;

        public bool enable;


        internal h2_AHandler handler;

        public string id;
        public KeyCode key;
        public string label;
        public bool shift;

        public h2_AShortcut(string label, string id, string shortcut, bool enable = true)
        {
            //Debug.Log("h2_AShortcut : " + label + ":" + id + ":" + shortcut);

            this.id = id;
            this.label = label;
            this.enable = enable;
            //this.shortcut	= shortcut;
            SetShortcutString(shortcut);
        }

        public bool isValid
        {
            get { return key != KeyCode.None; }
        }

        void Clear()
        {
            ctrl = false;
            alt = false;
            shift = false;
            key = KeyCode.None;
            chain = KeyCode.None;
        }

        public void SetShortcutString(string shortcut)
        {
            Clear();

            for (var i = 0; i < shortcut.Length; i++)
            {
                var c = shortcut[i];
                if (c == '_') continue;

                switch (c)
                {
                    case '%':
                        ctrl = true;
                        break;
                    case '&':
                        alt = true;
                        break;
                    case '#':
                        shift = true;
                        break;
                    default:
                    {
                        var str = shortcut.Substring(i, shortcut.Length - i);
                        var arr = str.Split('+');

                        key = String2KeyCode(arr[0]);
                        if (arr.Length > 1) chain = String2KeyCode(arr[1]);
                    }

                        return;
                }
            }
        }

        public int GetMatch(Event e)
        {
            if ((ctrl != e.control) || (alt != e.alt) || (shift != e.shift) || (key != e.keyCode)) return 0;
            return chain == KeyCode.None ? 1 : 2;
        }

        public void Draw()
        {
            //var uniqueId = GetUniqueId(ctrl, alt, shift, key, chain);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    enable = GUILayout.Toggle(enable, label, GUILayout.Width(Screen.width/2f));
                    if (enable)
                    {
                        if (isValid)
                        {
                            EditorGUILayout.HelpBox(GetHelp(), MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(LABEL_INVALID_SHORTCUT, MessageType.Warning);
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Space(8);
                    if (enable)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            ctrl = GUILayout.Toggle(ctrl, LABEL_CTRL);
                            alt = GUILayout.Toggle(alt, LABEL_ALT);
                            shift = GUILayout.Toggle(shift, LABEL_SHIFT);
                        }
                        EditorGUILayout.EndHorizontal();

                        if (isValid)
                        {
                            h2_KeyCodePopup.Popup(ref key);
                        }
                        else
                        {
                            var c = GUI.color;
                            GUI.color = COLOR_INVALID;
                            h2_KeyCodePopup.Popup(ref key);
                            GUI.color = c;
                        }
                        h2_KeyCodePopup.Popup(ref chain);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawKey(ref KeyCode key)
        {
            //var idx = Array.IndexOf(PopupKeys, KeyCode2String(key));
            //var nIdx = EditorGUILayout.Popup(idx, PopupKeys);

            //if (nIdx != idx)
            //{
            //	//validate !
            //	var newKey = String2KeyCode(PopupKeys[nIdx]);
            //	if (newKey != key){
            //		//actually changed !
            //		key = newKey;
            //		uniqueId = null;
            //	}
            //}
        }

        static int GetUniqueId(bool ctrl, bool alt, bool shift, KeyCode key, KeyCode chain)
        {
            if (key == KeyCode.None) return 0;
            return ((int) key << 12) | ((int) chain << 3) | ((ctrl ? 1 : 0) << 2) | ((alt ? 1 : 0) << 1) |
                   (shift ? 1 : 0);
        }

        public string GetHelp()
        {
            return GetHelp(ctrl, alt, shift, key, chain);
        }

        public static string GetHelp(bool ctrl, bool alt, bool shift, KeyCode key, KeyCode chain)
        {
            if (key == KeyCode.None) return string.Empty;
            if (helpMap == null) helpMap = new Dictionary<int, string>();

            var uid = GetUniqueId(ctrl, alt, shift, key, chain);

            string result;
            if (helpMap.TryGetValue(uid, out result)) return result;

            var arr = new List<string>();
            if (ctrl) arr.Add("Ctrl");
            if (alt) arr.Add("Alt");
            if (shift) arr.Add("Shift");

            var modifiers = arr.Count == 0 ? string.Empty : string.Join("+", arr.ToArray()) + "+";

            if (chain == KeyCode.None)
            {
                result = string.Format("{0}{1}", modifiers, KeyCode2String(key));
            }
            else
            {
                result = string.Format("{0}{1}\n{0}{2}", modifiers, KeyCode2String(key), KeyCode2String(chain));
            }

            helpMap.Add(uid, result);
            return result;
        }

        // -------------------------------------------------- STATIC ---------------------------------------

        //private static readonly KeyCode[] _noneList =
        //{
        //	KeyCode.LeftControl,
        //	KeyCode.LeftAlt,
        //	KeyCode.LeftShift,
        //	KeyCode.LeftApple,
        //	KeyCode.RightControl,
        //	KeyCode.RightAlt,
        //	KeyCode.RightShift,
        //	KeyCode.RightApple,
        //};

        //internal static string[] PopupKeys = {
        //	"NONE","",
        //	"DELETE","BACKSPACE","ESC",
        //	"",
        //	"NUMBER/1", "NUMBER/2", "NUMBER/3", "NUMBER/4", "NUMBER/5", "NUMBER/6", "NUMBER/7", "NUMBER/8",
        //	"Alphabet/A", "Alphabet/B", "Alphabet/C", "Alphabet/D", "Alphabet/E", "Alphabet/F", "Alphabet/G", "Alphabet/H", "Alphabet/I","Alphabet/J","Alphabet/K","Alphabet/L","Alphabet/M","Alphabet/N","Alphabet/O","Alphabet/P","Alphabet/Q","Alphabet/R","Alphabet/S","Alphabet/T","Alphabet/U","Alphabet/V","Alphabet/W","Alphabet/X","Alphabet/Y","Alphabet/Z",
        //	"",
        //	"SIGN/>", "SIGN/<", "SIGN/-","SIGN/+", "SIGN/[","SIGN/]","SIGN/(","SIGN/)",
        //	"OTHER/SLASH", "OTHER/BACK SLASH",
        //	"OTHER/", "OTHER/TAB","OTHER/SPACE","OTHER/ENTER",
        //	"",
        //	"NAVIGATE/HOME","NAVIGATE/END","NAVIGATE/PAGE UP","NAVIGATE/PAGE DOWN",
        //	"ARROW/LEFT","ARROW/RIGHT","ARROW/UP","ARROW/DOWN"
        //};

        internal static KeyCode String2KeyCode(string code)
        {
            code = code.ToUpper().Trim();
            if (string.IsNullOrEmpty(code)) return KeyCode.None;

            if (code.Length == 1)
            {
                var c = code[0];

                if (c >= '0' && c <= '9')
                {
                    return (KeyCode) ((int) KeyCode.Alpha0 + (c - '0'));
                }

                if (c >= 'A' && c <= 'Z')
                {
                    return (KeyCode) ((int) KeyCode.A + (c - 'A'));
                }

                switch (c)
                {
                    case '`':
                    case '~':
                        return KeyCode.BackQuote;
                    case ',':
                    case '<':
                        return KeyCode.Comma;
                    case '.':
                    case '>':
                        return KeyCode.Period;
                    case '?':
                    case '/':
                        return KeyCode.Backslash;
                    case ':':
                    case ';':
                        return KeyCode.Semicolon;
                    case '"':
                    case '\'':
                        return KeyCode.Quote;
                    case '{':
                    case '[':
                        return KeyCode.LeftBracket;
                    case '}':
                    case ']':
                        return KeyCode.RightBracket;
                    case '\\':
                    case '|':
                        return KeyCode.Backslash;
                    case '-':
                    case '_':
                        return KeyCode.Minus;
                    case '=':
                    case '+':
                        return KeyCode.Plus;

                    case '(':
                        return KeyCode.Alpha9;
                    case ')':
                        return KeyCode.Alpha0;
                }

                Debug.LogWarning("Unsupoprted key <" + c + ">");
                return KeyCode.None;
            }

            switch (code)
            {
                case "ESC":
                    return KeyCode.Escape;
                case "TAB":
                    return KeyCode.Tab;
                case "BACKSPACE":
                    return KeyCode.Backspace;
                case "SPACE":
                    return KeyCode.Space;
                case "DELETE":
                    return KeyCode.Delete;
                case "RETURN":
                    return KeyCode.Return;
                case "ENTER":
                    return KeyCode.Return;

                case "LEFT":
                    return KeyCode.LeftArrow;
                case "RIGHT":
                    return KeyCode.RightArrow;
                case "UP":
                    return KeyCode.UpArrow;
                case "DOWN":
                    return KeyCode.DownArrow;

                case "PAGEUP":
                    return KeyCode.PageUp;
                case "PAGEDOWN":
                    return KeyCode.PageDown;
                case "HOME":
                    return KeyCode.Home;
                case "END":
                    return KeyCode.End;
            }

            Debug.LogWarning("Unsupoprted key <" + code + ">");
            return KeyCode.None;
        }

        internal static string KeyCode2String(KeyCode key)
        {
            if (key == KeyCode.None) return string.Empty;

            if (key >= KeyCode.Alpha1 && key <= KeyCode.Alpha8)
            {
                return ((char) ('1' + (key - KeyCode.Alpha1))).ToString();
            }

            if (key >= KeyCode.A && key <= KeyCode.Z)
            {
                return ((char) ('A' + (key - KeyCode.A))).ToString();
            }

            switch (key)
            {
                case KeyCode.BackQuote:
                    return "~";
                case KeyCode.LeftBracket:
                    return "[";
                case KeyCode.RightBracket:
                    return "]";
                case KeyCode.LeftParen:
                    return "(";
                case KeyCode.RightParen:
                    return ")";


                case KeyCode.Backslash:
                    return "\\";

                case KeyCode.Comma:
                case KeyCode.Less:
                    return "<";
                case KeyCode.Period:
                case KeyCode.Greater:
                    return ">";
                case KeyCode.Question:
                case KeyCode.Slash:
                    return "/";
                case KeyCode.Semicolon:
                case KeyCode.Colon:
                    return ";";
                case KeyCode.Quote:
                case KeyCode.DoubleQuote:
                    return "\"";

                case KeyCode.Minus:
                case KeyCode.Underscore:
                    return "-";
                case KeyCode.Plus:
                case KeyCode.Equals:
                    return "=";

                case KeyCode.Exclaim:
                    return "1";
                case KeyCode.At:
                    return "2";
                case KeyCode.Hash:
                    return "3";
                case KeyCode.Dollar:
                    return "4";
                case KeyCode.Caret:
                    return "6";
                case KeyCode.Ampersand:
                    return "7";

                case KeyCode.Escape:
                    return "ESC";
                case KeyCode.Tab:
                    return "TAB";
                case KeyCode.Backspace:
                    return "BACKSPACE";
                case KeyCode.Space:
                    return "SPACE";
                case KeyCode.Delete:
                    return "DELETE";
                case KeyCode.Return:
                    return "ENTER";

                case KeyCode.LeftArrow:
                    return "LEFT";
                case KeyCode.RightArrow:
                    return "RIGHT";
                case KeyCode.UpArrow:
                    return "UP";
                case KeyCode.DownArrow:
                    return "DOWN";

                case KeyCode.PageUp:
                    return "PAGEUP";
                case KeyCode.PageDown:
                    return "PAGEDOWN";
                case KeyCode.Home:
                    return "HOME";
                case KeyCode.End:
                    return "END";
            }

            Debug.Log("Unsupported key <" + key + ">");

            return string.Empty;
        }
    }

    internal class h2_KeyCodePopup
    {
        internal static GUIContent[] KEY_NAMES =
        {
            new GUIContent(" - NONE -"),
            GUIContent.none,
            new GUIContent("DELETE"), new GUIContent("BACKSPACE"), new GUIContent("ESC"),
            GUIContent.none,
            new GUIContent("Number/1"), new GUIContent("Number/2"), new GUIContent("Number/3"),
            new GUIContent("Number/4"), new GUIContent("Number/5"), new GUIContent("Number/6"),
            new GUIContent("Number/7"), new GUIContent("Number/8"),
            new GUIContent("Alphabet/A"), new GUIContent("Alphabet/B"), new GUIContent("Alphabet/C"),
            new GUIContent("Alphabet/D"), new GUIContent("Alphabet/E"), new GUIContent("Alphabet/F"),
            new GUIContent("Alphabet/G"), new GUIContent("Alphabet/H"), new GUIContent("Alphabet/I"),
            new GUIContent("Alphabet/J"), new GUIContent("Alphabet/K"), new GUIContent("Alphabet/L"),
            new GUIContent("Alphabet/M"), new GUIContent("Alphabet/N"), new GUIContent("Alphabet/O"),
            new GUIContent("Alphabet/P"), new GUIContent("Alphabet/Q"), new GUIContent("Alphabet/R"),
            new GUIContent("Alphabet/S"), new GUIContent("Alphabet/T"), new GUIContent("Alphabet/U"),
            new GUIContent("Alphabet/V"), new GUIContent("Alphabet/W"), new GUIContent("Alphabet/X"),
            new GUIContent("Alphabet/Y"), new GUIContent("Alphabet/Z"),
            GUIContent.none,
            new GUIContent("Sign/>"), new GUIContent("Sign/<"), new GUIContent("Sign/-"), new GUIContent("Sign/+"),
	        new GUIContent("Sign/["), new GUIContent("Sign/]"), new GUIContent("Sign/("), new GUIContent("Sign/)"), new GUIContent("Sign/~"), 
            new GUIContent("Others/SLASH"), new GUIContent("Others/BACK SLASH"),
            new GUIContent("Others/"), new GUIContent("Others/TAB"), new GUIContent("Others/SPACE"),
            new GUIContent("Others/ENTER"),
            GUIContent.none,
            new GUIContent("Navigate/HOME"), new GUIContent("Navigate/END"), new GUIContent("Navigate/PAGE UP"),
            new GUIContent("Navigate/PAGE DOWN"),
            new GUIContent("Arrow/LEFT"), new GUIContent("Arrow/RIGHT"), new GUIContent("Arrow/UP"),
            new GUIContent("Arrow/DOWN")
        };

        internal static KeyCode[] KEY_VALUES =
        {
            KeyCode.None,
            KeyCode.None,
            KeyCode.Delete, KeyCode.Backspace, KeyCode.Escape,
            KeyCode.None,
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
            KeyCode.Alpha7, KeyCode.Alpha8,
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
            KeyCode.None,
	        KeyCode.Period, KeyCode.Comma, KeyCode.Minus, KeyCode.Plus, 
	        KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.LeftParen, KeyCode.RightParen, KeyCode.BackQuote,
            KeyCode.Slash, KeyCode.Backslash,
            KeyCode.None, KeyCode.Tab, KeyCode.Space, KeyCode.Return,
            KeyCode.None,
            KeyCode.Home, KeyCode.End, KeyCode.PageUp, KeyCode.PageDown,
            KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
        };

        internal static Dictionary<KeyCode, int> indexMap;

        public static bool Popup(ref KeyCode key)
        {
            //Profiler.BeginSample("h2_Shortcut.Popup");
            if (indexMap == null)
            {
                indexMap = new Dictionary<KeyCode, int>();
                for (var i = 0; i < KEY_VALUES.Length; i++)
                {
                    var k = KEY_VALUES[i];
                    if (k == KeyCode.None) continue;
                    indexMap.Add(k, i);
                }
                indexMap.Add(KeyCode.None, 0);
            }

            var idx = 0;
            if (!indexMap.TryGetValue(key, out idx))
            {
                idx = 0;
            }
            ;

            // THIS HACK MEANS TO OPTIMIZE PERFORMANCE FOR POPUP GUI 
            // (which is very slow and allocates a lot of garbage)
            // 
            var e = Event.current;
            var r = GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);

            if (e.type == EventType.Layout)
            {
                //Profiler.EndSample();
                return false;
            }

            if (e.type == EventType.Repaint)
            {
                GUI.Button(r, KEY_NAMES[idx], EditorStyles.popup);
            }
            else
            {
                var newIdx = EditorGUI.Popup(r, idx, KEY_NAMES);
                if (newIdx != idx)
                {
                    key = KEY_VALUES[newIdx];
                    //Profiler.EndSample();
                    return true;
                }
            }
            //Profiler.EndSample();

            return false;
        }
    }

    public class h2_ShortcutWindow : EditorWindow
    {
        static h2_ShortcutWindow window;
        static GUIStyle style;
        internal static string lastShortcut;
        internal static float lastTime;


        [MenuItem("Window/ShortcutWindow")]
        public static void Init()
        {
            if (window == null)
            {
                window = GetWindow<h2_ShortcutWindow>();
            }

            h2_Unity.SetWindowTitle(window, "Shortcut");
            window.ShowPopup();

            h2_Shortcut.OnTrigger -= UpdateShortcut;
            h2_Shortcut.OnTrigger += UpdateShortcut;

            EditorApplication.update -= DoRepaint;
            EditorApplication.update += DoRepaint;
        }

        static void DoRepaint()
        {
            window.Repaint();
        }

        static void UpdateShortcut(string shortcut)
        {
            lastShortcut = shortcut.Replace("+", " + ");
            lastTime = Time.realtimeSinceStartup;
            window.Repaint();
        }

        void OnGUI()
        {
            if (Time.realtimeSinceStartup - lastTime > 2f)
            {
                lastShortcut = string.Empty;
            }

            if (style == null)
            {
                style = new GUIStyle(EditorStyles.label);
                style.fontSize = 50;
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.yellow;
            }

            if (!string.IsNullOrEmpty(lastShortcut))
            {
                var r = GUILayoutUtility.GetRect(0, 0, Screen.width, Screen.height);
                GUI.Label(r, lastShortcut, style);
            }
        }
    }
}