using UnityEditor;
using UnityEngine;
using TMPro;

public class FontReplacer : EditorWindow
{
    private TMP_FontAsset oldFont;
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Replace TMP Font")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacer>("Replace TMP Font");
    }

    void OnGUI()
    {
        oldFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Old Font", oldFont, typeof(TMP_FontAsset), false);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace Fonts"))
        {
            ReplaceFonts();
        }
    }

    void ReplaceFonts()
    {
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        int count = 0;

        foreach (TMP_Text tmp in allTexts)
        {
            if (tmp.font == oldFont)
            {
                Undo.RecordObject(tmp, "Change TMP Font");
                tmp.font = newFont;
                count++;
            }
        }

        Debug.Log($"Replaced font on {count} TextMeshPro components.");
    }
}
