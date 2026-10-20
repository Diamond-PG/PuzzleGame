using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerVisual))]
[CanEditMultipleObjects]
public class PlayerVisualEditor : Editor
{
    // ============================================================
    // COLORS
    // ============================================================

    private static readonly Color HeaderColor =
        new Color(0.04f, 0.55f, 0.90f, 1f);

    private static readonly Color HeaderHoverColor =
        new Color(0.10f, 0.64f, 0.98f, 1f);

    private static readonly Color HeaderTextColor =
        Color.white;

    // ============================================================
    // FOLDOUT STATES
    // ============================================================

    private static bool referencesOpen = false;
    private static bool normalOpen = false;
    private static bool swordOpen = false;
    private static bool clubOpen = false;
    private static bool prisonOpen = false;
    private static bool climbOpen = false;
    private static bool movementOpen = false;
    private static bool blinkOpen = false;

    // ============================================================
    // STYLES
    // ============================================================

    private GUIStyle headerStyle;

    private void CreateStyles()
    {
        if (headerStyle != null)
            return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel);

        headerStyle.normal.textColor = HeaderTextColor;
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.fontSize = 12;
        headerStyle.padding = new RectOffset(12, 6, 0, 0);
    }

    // ============================================================
    // INSPECTOR
    // ============================================================

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CreateStyles();

        DrawSection(
            "01 - REFERENCES",
            ref referencesOpen,
            "spriteRenderer",
            "rb",
            "playerController",
            "playerJump",
            "prisonBreakDoor"
        );

        DrawSection(
            "02 - NORMAL VISUALS",
            ref normalOpen,
            "idleSprite",
            "idleBlinkSprite",
            "walkLeftSprite",
            "walkLeftBlinkSprite",
            "walkRightSprite",
            "walkRightBlinkSprite",
            "jumpSprite",
            "fallSprite",
            "fallVisualDelay",
            "fallVelocityThreshold",
            "hurtSprite",
            "hurtDuration",
            "kickRightSprite",
            "kickLeftSprite"
        );

        DrawSection(
            "03 - SWORD VISUALS",
            ref swordOpen,
            "swordJumpSprite",
            "swordFallSprite",
            "swordHurtLeftSprite",
            "swordHurtRightSprite",
            "swordKickRightSprite",
            "swordKickLeftSprite",
            "swordIdleLeftSprite",
            "swordIdleLeftBlinkSprite",
            "swordIdleRightSprite",
            "swordIdleRightBlinkSprite",
            "swordWalkLeftSprite",
            "swordWalkLeftBlinkSprite",
            "swordWalkRightSprite",
            "swordWalkRightBlinkSprite",
            "swordSwingLeftSprite",
            "swordSwingRightSprite",
            "swordStrikeLeftSprite",
            "swordStrikeRightSprite"
        );

        DrawSection(
            "04 - CLUB VISUALS",
            ref clubOpen,
            "clubJumpSprite",
            "clubFallSprite",
            "clubHurtLeftSprite",
            "clubHurtRightSprite",
            "clubKickRightSprite",
            "clubKickLeftSprite",
            "clubIdleLeftSprite",
            "clubIdleLeftBlinkSprite",
            "clubIdleRightSprite",
            "clubIdleRightBlinkSprite",
            "clubWalkLeftSprite",
            "clubWalkLeftBlinkSprite",
            "clubWalkRightSprite",
            "clubWalkRightBlinkSprite",
            "clubSwingLeftSprite",
            "clubSwingRightSprite",
            "clubStrikeLeftSprite",
            "clubStrikeRightSprite"
        );

        DrawSection(
            "05 - PRISON & DOOR REACTION",
            ref prisonOpen,
            "prisonSadSprite",
            "prisonSadBlinkSprite",
            "doorBreakReactionSprite",
            "doorBreakReactionDuration"
        );

        DrawSection(
            "06 - CLIMB VISUALS",
            ref climbOpen,
            "climbUpLeftSprite",
            "climbUpRightSprite",
            "climbUpLeftBlinkSprite",
            "climbUpRightBlinkSprite",
            "climbDownLeftSprite",
            "climbDownRightSprite",
            "climbDownLeftBlinkSprite",
            "climbDownRightBlinkSprite",
            "climbExitVisualGrace"
        );

        DrawSection(
            "07 - MOVEMENT SETTINGS",
            ref movementOpen,
            "movementThreshold"
        );

        DrawSection(
            "08 - BLINK SETTINGS",
            ref blinkOpen,
            "blinkInterval",
            "blinkDuration",
            "blinkRandomDelay"
        );

        serializedObject.ApplyModifiedProperties();
    }

    // ============================================================
    // DRAW BLUE SECTION
    // ============================================================

    private void DrawSection(
        string title,
        ref bool isOpen,
        params string[] propertyNames
    )
    {
        EditorGUILayout.Space(3f);

        Rect headerRect =
            GUILayoutUtility.GetRect(
                0f,
                34f,
                GUILayout.ExpandWidth(true)
            );

        bool isHovered =
            headerRect.Contains(Event.current.mousePosition);

        EditorGUI.DrawRect(
            headerRect,
            isHovered ? HeaderHoverColor : HeaderColor
        );

        Rect arrowRect = new Rect(
            headerRect.x + 8f,
            headerRect.y + 10f,
            14f,
            14f
        );

        EditorGUI.Foldout(
            arrowRect,
            isOpen,
            GUIContent.none,
            true
        );

        Rect labelRect = new Rect(
            headerRect.x + 26f,
            headerRect.y,
            headerRect.width - 32f,
            headerRect.height
        );

        GUI.Label(
            labelRect,
            title,
            headerStyle
        );

        if (Event.current.type == EventType.MouseDown &&
            headerRect.Contains(Event.current.mousePosition))
        {
            isOpen = !isOpen;
            Event.current.Use();
            Repaint();
        }

        if (!isOpen)
            return;

        EditorGUILayout.Space(6f);

        EditorGUI.indentLevel++;

        foreach (string propertyName in propertyNames)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                EditorGUILayout.PropertyField(
                    property,
                    true
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Property not found: " + propertyName,
                    MessageType.Warning
                );
            }
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.Space(7f);
    }
}