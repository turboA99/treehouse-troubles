using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace myUI
{
    public class Objectives : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Objectives, UxmlTraits> { }

        private List<Objective> objectives = new List<Objective>();

        private ScrollView objectivesContainer => this.Q<ScrollView>("Objectives");
        private bool isActive = false;
        public Objective AddObjective(string title, bool isCompleted = false)
        {
            Objective objective = new(title, isCompleted);
            objectives.Insert(0, objective);
            Init();
            Debug.Log(objective);
            return objective;
        }
        public Objectives()
        {
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>(
                "UI/Components/Objectives/Objectives"
            );
            asset.CloneTree(this);
            Init();
        }

        private void Init()
        {
            objectivesContainer.Clear();

            foreach (var objective in objectives)
            {
                objective.focusable = isActive;
                objectivesContainer.Add(objective);
                objective.RegisterCallback<FocusEvent>((e) =>
                {
                    objectivesContainer.ScrollTo(objective);
                });
            }


            if (isActive)
            {
                objectives[0].Focus();
            }
        }
        
        public void SetActive(bool state)
        {
            isActive = state;
            Init();
        }
    }
}

