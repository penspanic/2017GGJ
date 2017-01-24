//#define H2_PI_DEBUG

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    public enum h2_PILine
    {
        corner,
        vertical,
        horizontal
    }

    public class h2_ParentIndicator : h2_Icon
    {
        private static float lastDrawY = -1;
        private bool drawEnded;
        private List<Transform> drawList;
        //List<float> drawY;

        private Dictionary<Transform, int> drawMap;
        private int endIndex;
        private Transform lastDraw;

        private bool lastPOpen;

        // ------------------------------ HIERARCHY ICON -----------------------------

        //int pIndex;
        //int dLevel;
        //bool willDraw;
        //Transform lastDraw;


        private float maxY;

        private int pIndex;

        // ------------------------------ REFRESH DRAW PARENTS -----------------------------

        private Transform st;

        public h2_ParentIndicator() : base("h2_parent_indicator", 0)
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.ParentIndicator;
            h2_Selection.Register_OnSelectionChange(RefreshDrawParents);
            RefreshDrawParents(null);
        }

        public void RefreshDrawParents(GameObject[] s)
        {
            drawMap = null;

            if (h2_Selection.gameObject == null)
            {
                drawList = null;
                //drawY = null;
                return;
            }

            maxY = -1;
            st = h2_Selection.gameObject.transform;
            drawList = GetParents(st, true, true);

            //drawY = new List<float>();
            //for (var i = 0;i < drawList.Count; i++)
            //{
            //drawY.Add(-1);
            //}
        }

        private void RefreshDrawMap()
        {
            drawMap = new Dictionary<Transform, int>();
            for (var i = 0; i < drawList.Count; i++)
            {
                drawMap.Add(drawList[i], i);
            }
        }

        public List<Transform> GetParents(Transform t, bool parentFirst = true, bool includeMe = false)
        {
            var result = new List<Transform>();
            var p = includeMe ? t : t.parent;

            while (p != null)
            {
                result.Add(p);
                p = p.parent;
            }

            //reverse so that parent will come before child in the list
            if (parentFirst) result.Reverse();

            var str = "";
            for (var i = 0; i < result.Count; i++)
            {
                str += result[i].name + "\n";
            }

            //Debug.Log(str);

            return result;
        }

        private void DrawLine(GameObject go, h2_PILine lineStyle, int level, Rect r)
        {
            //SceneManager.get_sceneCount() > 1;
            var s = setting as h2_ParentIndicatorSettings;
            var state = s.stateTexture.states[0];
            var c = state.GetColor(h2_Selection.Contains(go), h2_Lazy.isPro, h2_Lazy.isFocus, h2_Lazy.isPlaying);

            var ox = h2_Unity.ParentIndicatorX;
            const int dx = 1;
            const int lineW = 2;
            var w = h2_Unity.parentIndicatorW;

            if (lineStyle == h2_PILine.vertical)
            {
                r.x = ox + level*w + (w - lineW)/2f + dx;
                r.width = lineW;

                h2_GUI.TextureColor(r, EditorGUIUtility.whiteTexture, c);
                return;
            }

            if (lineStyle == h2_PILine.horizontal && go.transform.childCount == 0)
            {
                h2_GUI.TextureColor(new Rect(
                    ox + level*w, r.y + (r.height - lineW)/2f + 1f,
                    w - 4f, lineW
                    ), EditorGUIUtility.whiteTexture, c);
                // no return here !
            }

            if (s.lineOnParent && lineStyle != h2_PILine.vertical && go.transform != st)
            {
                var cc = c;
                cc.a = 0.4f;

                h2_GUI.TextureColor(
                    new Rect(r.x, r.y + r.height - 2f, r.width, 1f),
                    EditorGUIUtility.whiteTexture, cc
                    );
            }

            //draw corner
            r.x = ox + (level - 1)*w + dx;
            r.width = w;
            h2_GUI.TextureColor(r, state.texture, c);
        }

        public void BeforeDraw()
        {
            pIndex = -1;
            //dLevel = -1;

            //drawBegin = false;
            drawEnded = drawList == null;
            lastDraw = null;

            if (drawList == null) return;
#if H2_PI_DEBUG
			Debug.Log("BeforeDraw -------------> " + maxY + " isOpen = " + lastPOpen + "\n"+ Event.current);
#endif
        }

        public override float Draw(Rect r, GameObject go)
        {
            if (!h2_Lazy.isRepaint) return 0f;

            if (r.y < lastDrawY || lastDrawY == -1)
            {
                //Debug.LogWarning("RESET _____________________ " + lastDrawY + " --> " + r.y);
                BeforeDraw();
            }

            lastDrawY = r.y;

#if H2_DEV
            Profiler.BeginSample("h2_ParentIndicator.Draw");
#endif
            if (drawEnded)
            {
#if H2_DEV
                Profiler.EndSample();
#endif
                return MaxWidth;
            }

            var t = go.transform;
            var firstDraw = false;

            if (pIndex == -1)
            {
                firstDraw = true;

                if (t == drawList[0])
                {
                    // new root !
                    pIndex = 0;
                    //drawY[pIndex] = r.y;
                    //dLevel = 0;
                }
                else if (lastDraw == null)
                {
                    var pList = GetParents(t, true, true);

                    lastDraw = t;
                    var dLevel = pList.Count;

                    var max = Mathf.Min(dLevel, drawList.Count);
                    for (var i = 0; i < max; i++)
                    {
                        if (pList[i] != drawList[i]) break;
                        //Debug.Log(drawList[i] + " ---> " + i);
                        pIndex = i;
                    }

                    //Debug.Log(go.name + " ---> " + pIndex);
                }

                if (pIndex != -1)
                {
                    if ((maxY > 0) && (r.y > maxY) && !lastPOpen)
                    {
#if H2_PI_DEBUG
						Debug.Log(" --> Early end Draw " + t.name + " pIndex = " + pIndex + " because of y out of range maxY=" + maxY + "\n" + r);
#endif
                        drawEnded = true;
                    }
#if H2_PI_DEBUG
					else {
						Debug.Log(" --> First Draw " + t.name + ": pIndex = " + pIndex  + " isOpen = " + lastPOpen + " maxY = " + maxY + "\n" + r);
					}
#endif
                }
            }

            if (pIndex != -1)
            {
                var dNext = pIndex < drawList.Count - 1 ? drawList[pIndex + 1] : null;

                if (!firstDraw)
                {
                    if (t == dNext)
                    {
                        pIndex++;
                        lastPOpen = t != st;
                        //drawY[pIndex] = r.y;
                    }
                    else
                    {
                        var p = t.parent;
                        if (p == lastDraw)
                        {
                            if (lastDraw == drawList[pIndex])
                            {
                                lastPOpen = t != st;
                            }

                            //go-to-child
                        }
                        else if (p == lastDraw.parent)
                        {
                            //sibling
                            if (p == drawList[pIndex].parent)
                            {
#if H2_PI_DEBUG
		            			Debug.Log("Cross sibling ! " + pIndex + ":" + p + ":" + drawList[pIndex]);
#endif
                                drawEnded = true;
                                lastPOpen = false; //t == st;
                            }
                        }
                        else //going up or jumps to another root
                        {
                            if (drawMap == null) RefreshDrawMap();

                            if (lastDraw == drawList[pIndex])
                            {
                                lastPOpen = false;
                            }

                            //We do need to check from t (t == dNext won't catch the case when t jumps up)
                            p = t;
                            var found = false;

                            while (p != null)
                            {
                                int v;
                                if (drawMap.TryGetValue(p, out v)) //found in map !
                                {
                                    found = true;
                                    drawEnded = pIndex > v; // draw ends when jumps out (pIndex decrease)
                                    pIndex = v;
                                    break;
                                }

                                p = p.parent;
                            }

                            if (drawEnded || !found)
                            {
                                drawEnded = true;
                                maxY = r.y - 1; // important : -1 pixel so that don't draw this object <t>
#if H2_PI_DEBUG
				            	Debug.LogWarning("jumps out maxY ---> pIndex = " + pIndex + " : maxY = " + maxY + ":" + t.name + "\n" + r);
#endif
                            }
                        }
                    }
                }

                if (!drawEnded)
                {
                    //Debug.Log(" --> Draw " + pIndex + " --> "+ t.name + "\n" + r);

                    if (pIndex != 0 || (go.transform != drawList[0]))
                    {
// Don't draw first parent
                        DrawLine(go,
                            t == st
                                ? h2_PILine.horizontal
                                : t == dNext || t == drawList[pIndex] ? h2_PILine.corner : h2_PILine.vertical,
                            pIndex, r
                            );
                    }

                    lastDraw = t;

                    if (t == st)
                    {
                        drawEnded = true;
                        maxY = r.y - 1; // important : -1 pixel so that don't draw this object <t>
#if H2_PI_DEBUG
			            Debug.LogWarning("special --> maxY ---> " + maxY + ":" + t.name);
#endif
                    }
                }
            }
#if H2_DEV
            Profiler.EndSample();
#endif
            return MaxWidth;
        }
    }

    [Serializable]
    public class h2_ParentIndicatorSettings : h2_IconSetting
    {
        private const string TITLE = "PARENT INDICATOR";
        private static readonly string[] TEXTURES = {"corner_tr"};

        private static readonly Color[][] COLORS =
        {
            new Color[]
            {
                new Color32(0, 171, 171, 255),
                new Color32(225, 225, 225, 255)
            }
        };

        public bool lineOnParent;

        internal override void Reset()
        {
            enableIcon = true;
            enableShortcut = false;
            lineOnParent = true;

            stateTexture = new h2_StateTexture
            {
                states = GetH2Textures(TEXTURES, COLORS)
            };

            shortcuts = null;
            h2_Utils.DelayRepaintHierarchy();
#if H2_DEV
            Debug.Log("RESET ACTIVE");
#endif
        }

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                lineOnParent = GUILayout.Toggle(lineOnParent, "Draw parent lines");
                stateTexture.DrawStates(enableIcon, TEXTURES);
                h2_GUI.DrawLine();
            }
        }
    }
}