using UnityEditor;
using UnityEngine;

namespace PolishTanks.Editor.Ground
{
    /// <summary>Inspector of <see cref="GroundLayerSources"/> with a button that bakes the variants into the target set's texture array.</summary>
    [CustomEditor(typeof(GroundLayerSources))]
    public class GroundLayerSourcesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Entry i holds the variants of layer i in the target set. Bake after changing them.", MessageType.None);
            if (GUILayout.Button("Bake textures"))
                Debug.Log(GroundLayerSetBaker.Bake((GroundLayerSources)target), target);
        }
    }
}
