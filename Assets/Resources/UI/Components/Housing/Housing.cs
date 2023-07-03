using UnityEngine.UIElements;
using UnityEngine;

namespace myUI
{
    public class Housing : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Housing, UxmlTraits> { }

        private HousingObj housingObj;

        private Label price => this.Q<Label>("Price");
        private Label title => this.Q<Label>("Title");
        private Label description => this.Q<Label>("Description");
        private VisualElement image => this.Q<VisualElement>("Image");

        public Housing() {
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>(
                "UI/Components/Housing/Housing"
            );
            asset.CloneTree(this);
            housingObj = HousingObj.CreateInstance<HousingObj>();
            housingObj.description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";
            Init();
        }
        public Housing(HousingObj _housingObj)

        {
            Init();
            housingObj = _housingObj;
           
        }
        private void Init()
        {
            style.alignItems = Align.Center;
            price.text = housingObj.price + "€";
            description.text = housingObj.description;
            title.text = housingObj.title;
            image.style.backgroundImage = new StyleBackground(housingObj.picture);
            image.style.height = image.style.width;
        }

        public void SetHousing(HousingObj housingObj)
        {
            this.housingObj = housingObj;
            Init();
        }
    }
}

