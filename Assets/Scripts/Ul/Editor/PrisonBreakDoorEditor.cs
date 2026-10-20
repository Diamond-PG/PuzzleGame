using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrisonBreakDoor))]
public class PrisonBreakDoorEditor : Editor
{
    private class Section
    {
        public string title;
        public string[] fields;
        public bool expanded;

        public Section(string title, params string[] fields)
        {
            this.title = title;
            this.fields = fields;
        }
    }

    private readonly Section[] sections =
    {
        new Section(
            "01 - REFERENCES",
            "doorVisual",
            "doorSpriteRenderer",
            "doorCollider",
            "player",
            "playerTag"
        ),

        new Section(
            "02 - BACKGROUND BEHIND DOOR",
            "blackBackground"
        ),

        new Section(
            "03 - DOOR SPRITES",
            "intactSprite",
            "damagedSprite",
            "brokenSprite"
        ),

        new Section(
            "04 - DOOR SETTINGS",
            "hitsToBreak",
            "damagedSpriteHit"
        ),

        new Section(
            "05 - TUTORIAL DOOR LOCK",
            "lockDoorDuringTutorial",
            "tutorialController",
            "unlockDoorAtPage",
            "logBlockedKicks"
        ),

        new Section(
            "06 - CARTOON PUNCH ANIMATION",
            "punchDuration",
            "minPunchScaleX",
            "maxPunchScaleX",
            "minPunchScaleY",
            "maxPunchScaleY",
            "maxPunchMoveX"
        ),

        new Section(
            "07 - FINAL BREAK",
            "finalPunchScaleX",
            "finalPunchScaleY",
            "finalPunchDuration",
            "finalBreakPause"
        ),

        new Section(
            "08 - WOOD CHIPS - DAMAGED HIT",
            "damagedHitChips",
            "damagedChipScale",
            "damagedChipSpreadX",
            "damagedChipLift"
        ),

        new Section(
            "09 - WOOD CHIPS - FINAL BREAK",
            "finalBreakChips",
            "finalChipScaleMin",
            "finalChipScaleMax",
            "finalChipSpreadX",
            "finalChipLift"
        ),

        new Section(
            "10 - WOOD CHIP ANIMATION",
            "chipLaunchDuration",
            "chipFallDuration",
            "chipStayDuration",
            "chipFadeDuration",
            "chipFloorOffsetY",
            "chipGroundLift",
            "chipEndRotationMin",
            "chipEndRotationMax"
        ),

        new Section(
            "11 - HAPTICS",
            "useHaptics",
            "normalHitHapticMs",
            "finalHitHapticMs"
        ),

        new Section(
            "12 - DOOR AUDIO",
            "audioSource",
            "hitSound",
            "breakSound",
            "hitVolume",
            "breakVolume"
        ),

        new Section(
            "13 - PLAYER CELEBRATION AUDIO",
            "usePlayerCelebrationSound",
            "playerCelebrationAudioSource",
            "playerCelebrationSound",
            "playerCelebrationVolume",
            "playerCelebrationDelay"
        ),

        new Section(
            "14 - EDITOR TEST START",
            "skipPrisonIntroForTesting"
        ),

        new Section(
            "15 - DEBUG",
            "debugLogs"
        )
    };

    private GUIStyle foldoutStyle;

    private readonly Color headerColor =
        new Color32(37, 65, 105, 255);

    private readonly Color textColor =
        new Color32(135, 199, 255, 255);

    private void OnEnable()
    {
        foreach (Section section in sections)
        {
            section.expanded = SessionState.GetBool(
                GetSectionKey(section.title),
                false
            );
        }
    }

    private string GetSectionKey(string title)
    {
        return "PrisonBreakDoorEditor_" + title;
    }

    private void CreateStyles()
    {
        if (foldoutStyle != null)
            return;

        foldoutStyle = new GUIStyle(EditorStyles.foldout);

        foldoutStyle.fontStyle = FontStyle.Bold;
        foldoutStyle.fontSize = 12;

        foldoutStyle.normal.textColor = textColor;
        foldoutStyle.onNormal.textColor = textColor;
        foldoutStyle.active.textColor = textColor;
        foldoutStyle.onActive.textColor = textColor;
        foldoutStyle.focused.textColor = textColor;
        foldoutStyle.onFocused.textColor = textColor;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CreateStyles();

        EditorGUILayout.Space(5);

        HashSet<string> displayedFields =
            new HashSet<string>();

        foreach (Section section in sections)
        {
            DrawSection(section, displayedFields);
        }

        DrawAdditionalFields(displayedFields);

        EditorGUILayout.Space(5);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(
        Section section,
        HashSet<string> displayedFields
    )
    {
        EditorGUILayout.Space(3);

        Rect rect = GUILayoutUtility.GetRect(
            0,
            27,
            GUILayout.ExpandWidth(true)
        );

        EditorGUI.DrawRect(rect, headerColor);

        Rect foldoutRect = new Rect(
            rect.x + 8,
            rect.y + 4,
            rect.width - 16,
            rect.height - 8
        );

        EditorGUI.BeginChangeCheck();

        bool expanded = EditorGUI.Foldout(
            foldoutRect,
            section.expanded,
            section.title,
            true,
            foldoutStyle
        );

        if (EditorGUI.EndChangeCheck())
        {
            section.expanded = expanded;

            SessionState.SetBool(
                GetSectionKey(section.title),
                expanded
            );
        }

        foreach (string fieldName in section.fields)
        {
            displayedFields.Add(fieldName);
        }

        if (!section.expanded)
            return;

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        foreach (string fieldName in section.fields)
        {
            SerializedProperty property =
                serializedObject.FindProperty(fieldName);

            if (property == null)
            {
                EditorGUILayout.HelpBox(
                    "Field not found: " + fieldName,
                    MessageType.Warning
                );

                continue;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawAdditionalFields(
        HashSet<string> displayedFields
    )
    {
        List<string> additionalFields =
            new List<string>();

        SerializedProperty iterator =
            serializedObject.GetIterator();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "m_Script")
                continue;

            if (iterator.depth != 0)
                continue;

            if (!displayedFields.Contains(iterator.name))
            {
                additionalFields.Add(iterator.name);
            }
        }

        if (additionalFields.Count == 0)
            return;

        EditorGUILayout.Space(6);

        Rect rect = GUILayoutUtility.GetRect(
            0,
            27,
            GUILayout.ExpandWidth(true)
        );

        EditorGUI.DrawRect(rect, headerColor);

        GUIStyle labelStyle =
            new GUIStyle(EditorStyles.boldLabel);

        labelStyle.normal.textColor = textColor;
        labelStyle.fontSize = 12;

        EditorGUI.LabelField(
            new Rect(
                rect.x + 10,
                rect.y + 5,
                rect.width - 20,
                rect.height - 10
            ),
            "16 - ADDITIONAL SETTINGS",
            labelStyle
        );

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        foreach (string fieldName in additionalFields)
        {
            SerializedProperty property =
                serializedObject.FindProperty(fieldName);

            if (property != null)
            {
                EditorGUILayout.PropertyField(
                    property,
                    true
                );
            }
        }

        EditorGUILayout.EndVertical();
    }
}