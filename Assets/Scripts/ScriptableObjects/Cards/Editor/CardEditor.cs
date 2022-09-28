using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableCard))]
public partial class CardEditor : Editor {
    const float _maxWidth = 400f;
    const float _margins = 0.2f;
    const float _imageAspect = 1.5f;

    static readonly Color[] _themesColorList = new Color[] {
            new Color(.937f, .278f, .435f),
            new Color(1f, .82f, .4f),
            new Color(.0667f, .541f, .698f),
        };

    static List<Texture2D> _bgCache;
    public static void RefreshBGCache() {
        _bgCache = new List<Texture2D>();
        var refBG = Resources.Load<Texture2D>("Sprites/CardSprites/CardRef");
        _bgCache.Add(refBG);
    }

    public static Texture2D GetBG(EChipType theme) {
        return GetBG((int)theme);
    }

    public static Texture2D GetBG(int theme) {
        if (_bgCache == null)
            RefreshBGCache();
        if (theme >= _bgCache.Count)
            theme = _bgCache.Count - 1;
        return _bgCache[theme];
    }

    public ScriptableCard Target => target as ScriptableCard;

    /// <summary>
    /// Remove required in another editor (external usage)
    /// </summary>
    public bool RemoveRequired { get; protected set; } = false;

    public override void OnInspectorGUI() {
        // try {
        var Cost = serializedObject.FindProperty(nameof(Target.Cost));
        if (GUILayout.Button("RefreshBGCache"))
            RefreshBGCache();

        // start
        EditorGUI.BeginChangeCheck();
        serializedObject.UpdateIfRequiredOrScript();

        // var propTheme = serializedObject.FindProperty("Data").FindPropertyRelative("Theme");
        EditorGUILayout.PropertyField(Cost);

        // Background
        var rect = GUILayoutUtility.GetRect(256,6);
        if(_bgCache == null) RefreshBGCache();
        var bg = _bgCache[0];

        var bgWidth = Mathf.Min(Screen.width,_maxWidth);
        GUI.DrawTexture(
            new Rect(
                Screen.width > bgWidth ?
                (Screen.width - bgWidth) / 2f 
                : 0,rect.y,bgWidth,bgWidth 
                * bg.height 
                / bg.width),
            bg,
            ScaleMode.ScaleToFit,true);

        // Content 
        var contentWidth = bgWidth - bgWidth * (_margins * 2f);
        contentWidth = Mathf.Min(contentWidth,_maxWidth);
        var inMargin = (bgWidth * _margins) / 2f;
        float posY = 0f;

        // Container
        using (new EditorGUILayout.HorizontalScope()) {
            GUILayout.FlexibleSpace();
            // Content
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(contentWidth))) {
                GUILayout.Space(bgWidth * _margins);

                posY += (bgWidth * _margins);

                GUILayout.Space(inMargin);
                posY += (inMargin);

                // Image

                var bgimage = AssetPreview.GetAssetPreview(Target.IllustrationImage);

                var flexibleBorderMargin = (Screen.width - contentWidth) / 2f;
                if (bgimage != null)
                    GUI.DrawTexture(new Rect(flexibleBorderMargin,posY + 75,contentWidth,contentWidth / _imageAspect),bgimage,ScaleMode.ScaleToFit,true);
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(contentWidth),GUILayout.Height(contentWidth / _imageAspect))) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Target.IllustrationImage)),new GUIContent("Illustration","Illustration"));
                }

                posY += (contentWidth / _imageAspect);
                posY += (inMargin);

                // Title

                var propTitle = serializedObject.FindProperty(nameof(Target.Title));
                var styleTitle = new GUIStyle("DefaultCenteredLargeText");
                styleTitle.normal.textColor = Color.white;
                propTitle.stringValue = EditorGUILayout.TextField(propTitle.stringValue,styleTitle);
                posY += styleTitle.lineHeight + styleTitle.padding.top + styleTitle.padding.bottom;

                GUILayout.Space(inMargin);
                posY += (inMargin);

                // Description

                var propDescription = serializedObject.FindProperty(nameof(Target.Description));
                var descHeight = (bg.height * (bgWidth / bg.width)) - posY - (bgWidth * _margins);
                var styleDesc = new GUIStyle("DefaultCenteredText");
                styleDesc.alignment = TextAnchor.UpperCenter;
                styleDesc.normal.textColor = Color.white;
                styleDesc.wordWrap = true;
                propDescription.stringValue = EditorGUILayout.TextArea(propDescription.stringValue,styleDesc,GUILayout.Height(descHeight));
                posY += styleDesc.CalcHeight(new GUIContent(propDescription.stringValue),contentWidth) + styleDesc.padding.top + styleDesc.padding.bottom;

                GUILayout.Space(inMargin);
                posY += (bgWidth * _margins);
            }

            GUILayout.FlexibleSpace();

        }

        // Save
        if (EditorGUI.EndChangeCheck()) {

            Save();
        }
        //} catch {            base.OnInspectorGUI();        }
    }

    void Save() {
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }
    public override Texture2D RenderStaticPreview(string assetPath,UnityEngine.Object[] subAssets,int width,int height) {
        if (Target == null || Target.IllustrationImage == null)
            return null;

        Texture2D cache = new Texture2D(width,height);
        var mimgw = width - Mathf.RoundToInt(width * (_margins * 2f));
        var mimgh = Mathf.RoundToInt(Target.IllustrationImage.rect.height * (mimgw / Target.IllustrationImage.rect.width));

        Texture2D bgImg = new Texture2D(mimgw,mimgh);
        var prev = AssetPreview.GetAssetPreview(Target.IllustrationImage);
        if (prev == null)
            return null;
        EditorUtility.CopySerialized(prev,bgImg);
        bgImg = ResizeTexture(bgImg,mimgw,mimgh);

        Texture2D mainImg = new Texture2D(mimgw,mimgh);
        prev = AssetPreview.GetAssetPreview(Target.IllustrationImage);
        if (prev == null)
            return null;
        EditorUtility.CopySerialized(prev,mainImg);
        mainImg = ResizeTexture(mainImg,mimgw,mimgh);

        var tex = _bgCache[0];
        tex = ResizeTexture(tex,width,height);

        tex.SetPixels((width - mimgw) / 2,height - Mathf.RoundToInt(height * _margins) - mimgh,mimgw,mimgh,bgImg.GetPixels());

        int bx = (width - mimgw) / 2;
        int by = height - Mathf.RoundToInt(height * _margins) - mimgh;

        int x1 = 0;
        int y1 = 0;
        for (int i = bx;i < bx + mimgw;++i) {
            y1 = 0;
            for (int y = by;y < by + mimgh;++y) {
                var px = mainImg.GetPixel(x1,y1);
                var px2 = tex.GetPixel(i,y);
                px2.a = 1f - px.a;

                if (px2.a != 1f) {
                    tex.SetPixel(i,y,new Color(px.r * px.a + px2.r * px2.a,px.g * px.a + px2.g * px2.a,px.b * px.a + px2.b * px2.a,1f));
                }

                ++y1;
            }
            ++x1;
        }

        tex.Apply();

        EditorUtility.CopySerialized(tex,cache);
        return cache;
    }

    public static Texture2D ResizeTexture(Texture2D img,int width,int height) {
        if (img != null) {
            var finalIcon = new Texture2D(width,height);

            for (int x = 0;x < width;++x) {
                for (int y = 0;y < height;++y) {
                    finalIcon.SetPixel(x,y,img.GetPixel((x * img.width) / width,(y * img.height) / height));
                }
            }
            finalIcon.Apply();

            img = finalIcon;
        } else {
            img = new Texture2D(width,height);
        }

        return img;
    }
}

