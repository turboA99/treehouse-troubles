using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace myUI
{
    public class Objective : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Objective, UxmlTraits> { }

        // Add the two custom UXML attributes.
        public new class UxmlTraits : VisualElement.UxmlTraits {
            UxmlStringAttributeDescription m_title = new UxmlStringAttributeDescription{name = "objective", defaultValue = "Objective"};
            UxmlBoolAttributeDescription m_isCompleted = new UxmlBoolAttributeDescription { name = "is-completed", defaultValue = false};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var objective = ve as Objective;

                objective.title = m_title.GetValueFromBag(bag, cc);
                objective.isCompleted = m_isCompleted.GetValueFromBag(bag, cc);
                objective.Init();
            }
        }

        public Objective() : this("Objective", false) { }

        private string title { get; set; }
        private bool isCompleted { get; set; }

        private Label text => this.Q<Label>("Title");
        private Label dots => this.Q<Label>("Dots");
        private VisualElement tick => this.Q<VisualElement>("Tick");

        public Objective(string title, bool isCompleted)
        {
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>(
                "UI/Components/Objective/Objective"
            );
            this.title = title;
            this.isCompleted = isCompleted;
            asset.CloneTree(this);
            AddToClassList("objective-root");
            Init();
        }
        private void Init()
        {
            text.text = title;
            switch (isCompleted)
            {
                case true:
                    if (!dots.ClassListContains("hidden")) dots.AddToClassList("hidden");
                    if (tick.ClassListContains("hidden")) tick.RemoveFromClassList("hidden");
                    break;
                case false:
                    if (!tick.ClassListContains("hidden")) tick.AddToClassList("hidden");
                    if (dots.ClassListContains("hidden")) dots.RemoveFromClassList("hidden");
                    break;
            }
        }
        public void SetCompleted(bool value)
        {
            isCompleted = value;
            Init();
        }
    }
}
