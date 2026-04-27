using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

#if TMP_VERSION_2_1_0_OR_NEWER
using TMP_UiEditorPanel = TMPro.EditorUtilities.TMP_EditorPanelUI;
#else
using TMP_UiEditorPanel = UnityEditor.Editor;
#endif

namespace RTLTMPro
{
    [CustomEditor(typeof(RTLTextMeshPro)), CanEditMultipleObjects]
    public class RTLTextMeshProEditor : TMP_UiEditorPanel
    {
        private SerializedProperty originalTextProp;
        private SerializedProperty preserveNumbersProp;
        private SerializedProperty farsiProp;
        private SerializedProperty fixTagsProp;
        private SerializedProperty forceFixProp;

        private bool foldout;
        private RTLTextMeshPro tmpro;

        protected virtual void OnEnable()
        {
#if TMP_VERSION_2_1_0_OR_NEWER
            base.OnEnable();
#endif
            foldout = true;
            preserveNumbersProp = serializedObject.FindProperty("preserveNumbers");
            farsiProp = serializedObject.FindProperty("farsi");
            fixTagsProp = serializedObject.FindProperty("fixTags");
            forceFixProp = serializedObject.FindProperty("forceFix");
            originalTextProp = serializedObject.FindProperty("originalText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            tmpro = (RTLTextMeshPro)target;

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(originalTextProp, new GUIContent("RTL Text Input Box"));

            ListenForZeroWidthNoJoiner();

            if (EditorGUI.EndChangeCheck())
                OnChanged();

            serializedObject.ApplyModifiedProperties();

#if TMP_VERSION_2_1_0_OR_NEWER
            base.OnInspectorGUI();
#endif

            foldout = EditorGUILayout.Foldout(foldout, "RTL Settings", true);
            if (foldout)
            {
                DrawOptions();

                if (GUILayout.Button("Re-Fix"))
                    MarkPropertiesChanged();

                if (EditorGUI.EndChangeCheck())
                    MarkPropertiesChanged();
            }

            if (HavePropertiesChanged())
                OnChanged();

            serializedObject.ApplyModifiedProperties();
        }

        protected void OnChanged()
        {
            tmpro.UpdateText();
            ClearPropertiesChangedFlag();
            EditorUtility.SetDirty(target);

#if TMP_VERSION_2_1_0_OR_NEWER
            m_TextComponent.havePropertiesChanged = true;
            m_TextComponent.ComputeMarginSize();
#endif
        }

        protected virtual void DrawOptions()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            farsiProp.boolValue = GUILayout.Toggle(farsiProp.boolValue, new GUIContent("Farsi"));
            forceFixProp.boolValue = GUILayout.Toggle(forceFixProp.boolValue, new GUIContent("Force Fix"));
            preserveNumbersProp.boolValue = GUILayout.Toggle(preserveNumbersProp.boolValue, new GUIContent("Preserve Numbers"));

            if (tmpro.richText)
                fixTagsProp.boolValue = GUILayout.Toggle(fixTagsProp.boolValue, new GUIContent("FixTags"));

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void ListenForZeroWidthNoJoiner()
        {
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            bool shortcutPressed =
                (Event.current.modifiers & EventModifiers.Control) != 0 &&
                (Event.current.modifiers & EventModifiers.Shift) != 0 &&
                Event.current.type == EventType.KeyUp &&
                Event.current.keyCode == KeyCode.Alpha2;

            if (!shortcutPressed) return;

            originalTextProp.stringValue = originalTextProp.stringValue.Insert(
                editor.cursorIndex,
                ((char)SpecialCharacters.ZeroWidthNoJoiner).ToString()
            );

            editor.selectIndex++;
            editor.cursorIndex++;
            Event.current.Use();
            Repaint();
        }

        private void MarkPropertiesChanged()
        {
#if TMP_VERSION_2_1_0_OR_NEWER
            m_HavePropertiesChanged = true;
#endif
        }

        private bool HavePropertiesChanged()
        {
#if TMP_VERSION_2_1_0_OR_NEWER
            return m_HavePropertiesChanged;
#else
            return false;
#endif
        }

        private void ClearPropertiesChangedFlag()
        {
#if TMP_VERSION_2_1_0_OR_NEWER
            m_HavePropertiesChanged = false;
#endif
        }
    }
}