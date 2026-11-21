using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Bakvand.UI
{
    [CustomEditor(typeof(CollectionPictureView))]
    internal sealed class CollectionPictureViewEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var view = (CollectionPictureView)target;
            var root = new VisualElement();
            var enabled = new Toggle("Enable Collection Test Mode") { name = "collection-test-mode" };
            var actions = new VisualElement();
            var status = new Label();
            var next = new Button(view.PlayCollectionTestNextPiece) { text = "Play Next Piece", name = "collection-next" };
            var complete = new Button(view.PlayCollectionTestCompletion) { text = "Play Completion Animation", name = "collection-complete" };

            void Refresh()
            {
                if (view == null)
                    return;

                enabled.SetValueWithoutNotify(view.CollectionTestModeEnabled);
                enabled.SetEnabled(EditorApplication.isPlaying);
                actions.SetEnabled(EditorApplication.isPlaying && view.CollectionTestModeEnabled);
                bool ready = view.isActiveAndEnabled && !view.CollectionTestIsAnimating;
                next.SetEnabled(ready && view.CollectionTestPieceCount < 4);
                complete.SetEnabled(ready);
                status.text = view.CollectionTestModeEnabled
                    ? (view.isActiveAndEnabled ? $"Test progress {view.CollectionTestPieceCount}/4" : "Open Gallery → Collection to play animations.")
                    : "Available in Play Mode. Uses temporary progress.";
            }

            enabled.RegisterValueChangedCallback(change =>
            {
                view.SetCollectionTestMode(change.newValue);
                Refresh();
            });
            root.Add(enabled);
            root.Add(status);
            for (int count = 0; count <= 4; count++)
            {
                int pieces = count;
                actions.Add(new Button(() => view.ShowCollectionTestPieces(pieces))
                    { text = $"Show {pieces}/4", name = $"collection-show-{pieces}" });
            }
            actions.Add(next);
            actions.Add(complete);
            actions.Add(new Button(() => view.ShowCollectionTestPieces(0))
                { text = "Reset Test", name = "collection-reset" });
            root.Add(actions);
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            root.schedule.Execute(Refresh).Every(100);
            Refresh();
            return root;
        }
    }
}
