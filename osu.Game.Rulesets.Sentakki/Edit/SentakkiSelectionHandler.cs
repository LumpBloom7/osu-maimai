using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : EditorSelectionHandler
    {
        private readonly Bindable<TernaryState> selectionBreakState = new Bindable<TernaryState>();
        private readonly Bindable<TernaryState> selectionSlideMirroredState = new Bindable<TernaryState>();

        public SentakkiSelectionHandler()
        {
            selectionBreakState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setBreakState(s.NewValue == TernaryState.True);
                        break;
                }
            };

            selectionSlideMirroredState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setMirroredState(s.NewValue == TernaryState.True);
                        break;
                }
            };
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            // The lanes are arranged in a circular fashion
            // Blindly deriving new angle by adding the vector to each selection point will yield unintuitive results, such as all notes moving towards the drag direction
            //
            // Instead, we should derive an angle delta by comparing the current mouse position and the drag origin
            // The drag origin is the blueprint most closest to the mouse during the beginning of the drag, rather than the earliest note (see "SentakkiBlueprintContainer.SortForMovement")
            // This allows a more intuitive "steering wheel" like lane adjustments
            if (SelectedBlueprints.All(bp => bp.Item is SentakkiLanedHitObject))
            {
                var oldPosition = moveEvent.Blueprint.ScreenSpaceSelectionPoint;
                var newPosition = moveEvent.Blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta;
                var playfieldCentre = ToScreenSpace(new Vector2(300));
                var angleDelta = playfieldCentre.GetDegreesFromPosition(newPosition) - playfieldCentre.GetDegreesFromPosition(oldPosition);

                foreach (var bp in SelectedBlueprints.ToList())
                {
                    var laned = bp.Item as SentakkiLanedHitObject;
                    var currentAngle = laned.Lane.GetRotationForLane() + angleDelta;
                    laned.Lane = currentAngle.GetNoteLaneFromDegrees();
                }
                return true;
            }
            else if (SelectedBlueprints.All(bp => bp.Item is Touch))
            {
                // Special movement handling to ensure that all touch notes are within 250 units from the playfield centre
                moveTouchNotes(this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta));
                return true;
            }
            return false;
        }

        private void setBreakState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>();

            EditorBeatmap.BeginChange();

            foreach (var lho in lhos)
            {
                if (lho.Break == state)
                    continue;

                lho.Break = state;
                EditorBeatmap.Update(lho);
            }

            EditorBeatmap.EndChange();
        }

        private void setMirroredState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<Slide>();

            EditorBeatmap.BeginChange();

            foreach (var lho in lhos)
            {
                if (lho.SlideInfoList.First().Mirrored == state)
                    continue;

                lho.SlideInfoList.First().Mirrored = state;
                EditorBeatmap.Update(lho);
            }

            EditorBeatmap.EndChange();
        }

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
            selectionSlideMirroredState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Slide>(), h => h.SlideInfoList.First().Mirrored);
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.Any(s => s.Item is SentakkiLanedHitObject))
                yield return new TernaryStateToggleMenuItem("Break") { State = { BindTarget = selectionBreakState } };

            if (selection.All(s => s.Item is Slide))
            {
                yield return new TernaryStateToggleMenuItem("Mirrored") { State = { BindTarget = selectionSlideMirroredState } };
                yield return new OsuMenuItem("Patterns") { Items = getContextMenuItemsForSlide() };
            }

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }

        private List<MenuItem> getContextMenuItemsForSlide()
        {
            var patterns = new List<MenuItem>();

            var SectionItems = new List<OsuMenuItem>();
            void createPatternGroup(string patternName)
                => patterns.Add(new OsuMenuItem(patternName) { Items = SectionItems = new List<OsuMenuItem>() });

            for (int i = 0; i < SlidePaths.VALIDPATHS.Count; ++i)
            {
                if (i == 0)
                    createPatternGroup("Circular");
                else if (i == 7)
                    createPatternGroup("L shape");
                else if (i == 11)
                    createPatternGroup("Straight");
                else if (i == 14)
                    createPatternGroup("Thunder");
                else if (i == 15)
                    createPatternGroup("U shape");
                else if (i == 23)
                    createPatternGroup("V shape");
                else if (i == 26)
                    createPatternGroup("Cup shape");

                int j = i;
                SectionItems.Add(createMenuEntryForPattern(j));
            }
            return patterns;
        }

        private OsuMenuItem createMenuEntryForPattern(int ID)
        {
            void commit()
            {
                EditorBeatmap.BeginChange();
                foreach (var bp in SelectedBlueprints)
                {
                    (bp.Item as Slide).SlideInfoList.First().ID = ID;
                    EditorBeatmap.Update(bp.Item);
                }
                EditorBeatmap.EndChange();
            }

            return new OsuMenuItem(SlidePaths.VALIDPATHS[ID].Item1.EndLane.ToString(), MenuItemType.Standard, commit);
        }

        private void moveTouchNotes(Vector2 dragDelta)
        {
            const float boundary_radius = 250;

            float dragDistance(Vector2 origin, Vector2 destination)
                => MathF.Min((destination - origin).Length, circleIntersectionDistance(origin, destination - origin));

            float circleIntersectionDistance(Vector2 centre, Vector2 direction)
            {
                direction.Normalize();
                var b = (direction.X * centre.X) + (direction.Y * centre.Y);
                var c = centre.LengthSquared - (boundary_radius * boundary_radius);
                return MathF.Sqrt((b * b) - c) - b;
            }

            var touches = SelectedBlueprints.Select(bp => bp.Item as Touch).ToList();
            var centre = touches.Aggregate(Vector2.Zero, (a, b) => a + b.Position) / touches.Count;
            var cappedDragDelta = touches.Min(t => dragDistance(t.Position - centre, t.Position + dragDelta));

            if (!(cappedDragDelta >= 0)) return; // No movement or invalid movement occurred

            foreach (var touch in touches)
                touch.Position = touch.Position - centre + (cappedDragDelta * (dragDelta + centre).Normalized());
        }
    }
}
