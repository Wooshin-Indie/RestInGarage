using UnityEditor;
using UnityEngine;

public class URPtoToonShader : EditorWindow
{
    private Shader newShader; // 변경할 새 셰이더를 담을 변수

    [MenuItem("Tools/Change Selected Materials Shader")] // 에디터 메뉴에 항목 추가
    public static void ShowWindow()
    {
        GetWindow<URPtoToonShader>("Change Shader");
    }

    void OnGUI()
    {
        GUILayout.Label("Select New Shader", EditorStyles.boldLabel);

        // Shader 필드를 만들어 Inspector처럼 셰이더를 선택할 수 있게 함
        newShader = (Shader)EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false);

        GUILayout.Space(20);

        if (GUILayout.Button("Change Shader for Selected Materials"))
        {
            ChangeSelectedMaterialsShader();
        }
    }

    void ChangeSelectedMaterialsShader()
    {
        if (newShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a new shader first.", "OK");
            return;
        }

        // 현재 선택된 모든 오브젝트 가져오기
        Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Information", "No materials selected in the Project window.", "OK");
            return;
        }

        int changedCount = 0;
        foreach (Object obj in selectedObjects)
        {
            Material mat = obj as Material;
            if (mat != null)
            {
                Undo.RecordObject(mat, "Change Material Shader"); // 변경 사항을 Undo 가능하게 기록
                mat.shader = newShader;
                EditorUtility.SetDirty(mat); // 변경 사항을 저장하도록 표시
                changedCount++;
            }
        }

        AssetDatabase.SaveAssets(); // 변경된 에셋 저장 (선택 사항, 없어도 보통 저장됨)
        AssetDatabase.Refresh();    // 프로젝트 창 새로고침

        EditorUtility.DisplayDialog("Success", $"{changedCount} materials' shader changed to '{newShader.name}'.", "OK");
    }
}
