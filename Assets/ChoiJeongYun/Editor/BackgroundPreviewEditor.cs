using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BackgroundPreview))]
[CanEditMultipleObjects]
public class BackgroundPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BackgroundPreview preview = (BackgroundPreview)target;

        if (GUILayout.Button("Apply Preview"))
        {
            preview.ApplyPreview();
        }
    }
}
