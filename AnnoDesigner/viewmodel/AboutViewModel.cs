using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.viewmodel
{
    public class AboutViewModel : Notify
    {
        private readonly ICommons _commons;

        public AboutViewModel(ICommons commonsToUse)
        {
            _commons = commonsToUse;

            OpenOriginalHomepageCommand = new RelayCommand(OpenOriginalHomepage);
            OpenProjectHomepageCommand = new RelayCommand(OpenProjectHomepage);
            OpenWikiHomepageCommand = new RelayCommand(OpenWikiHomepage);
            CloseWindowCommand = new RelayCommand<ICloseable>(CloseWindow);

            UpdateLanguage();
        }

        #region localization

        public void UpdateLanguage()
        {
            string language = Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage);

            TitleAbout = Localization.Localization.Translations[language]["TitleAbout"];
            BuildingLayoutDesigner = Localization.Localization.Translations[language]["BuildingLayoutDesigner"];

            //Credits
            Credits = Localization.Localization.Translations[language]["Credits"];
            OriginalApplicationBy = Localization.Localization.Translations[language]["OriginalApplicationBy"];
            BuildingPresets = Localization.Localization.Translations[language]["BuildingPresets"];
            CombinedForAnnoVersions = Localization.Localization.Translations[language]["CombinedForAnnoVersions"];
            AdditionalChanges = Localization.Localization.Translations[language]["AdditionalChanges"];
            ManyThanks = Localization.Localization.Translations[language]["ManyThanks"];
            VisitTheFandom = Localization.Localization.Translations[language]["VisitTheFandom"];
            OriginalHomepage = Localization.Localization.Translations[language]["OriginalHomepage"];
            ProjectHomepage = Localization.Localization.Translations[language]["ProjectHomepage"];
            GoToFandom = Localization.Localization.Translations[language]["GoToFandom"];
            Close = Localization.Localization.Translations[language]["Close"];
        }

        private string _titleAbout;
        public string TitleAbout
        {
            get { return _titleAbout; }
            set { UpdateProperty(ref _titleAbout, value); }
        }

        //This string is the same in every language, so does not need to be localized.
        public string AnnoDesigner
        {
            get { return "Anno Designer"; }
        }

        private string _buildingLayoutDesigner;
        public string BuildingLayoutDesigner
        {
            get { return _buildingLayoutDesigner; }
            set { UpdateProperty(ref _buildingLayoutDesigner, value); }
        }

        //Credits
        private string _credits;
        public string Credits
        {
            get { return _credits; }
            set { UpdateProperty(ref _credits, value); }
        }
        private string _originalApplicationBy;
        public string OriginalApplicationBy
        {
            get { return _originalApplicationBy; }
            set { UpdateProperty(ref _originalApplicationBy, value); }
        }
        private string _buildingPresets;
        public string BuildingPresets
        {
            get { return _buildingPresets; }
            set { UpdateProperty(ref _buildingPresets, value); }
        }
        private string _combinedForAnnoVersions;
        public string CombinedForAnnoVersions
        {
            get { return _combinedForAnnoVersions; }
            set { UpdateProperty(ref _combinedForAnnoVersions, value); }
        }
        private string _additionalChanges;
        public string AdditionalChanges
        {
            get { return _additionalChanges; }
            set { UpdateProperty(ref _additionalChanges, value); }
        }
        private string _manyThanks;
        public string ManyThanks
        {
            get { return _manyThanks; }
            set { UpdateProperty(ref _manyThanks, value); }
        }
        private string _visitTheFandom;
        public string VisitTheFandom
        {
            get { return _visitTheFandom; }
            set { UpdateProperty(ref _visitTheFandom, value); }
        }
        private string _originalHomepage;
        public string OriginalHomepage
        {
            get { return _originalHomepage; }
            set { UpdateProperty(ref _originalHomepage, value); }
        }
        private string _projectHomepage;
        public string ProjectHomepage
        {
            get { return _projectHomepage; }
            set { UpdateProperty(ref _projectHomepage, value); }
        }
        private string _goToFandom;
        public string GoToFandom
        {
            get { return _goToFandom; }
            set { UpdateProperty(ref _goToFandom, value); }
        }
        private string _close;
        public string Close
        {
            get { return _close; }
            set { UpdateProperty(ref _close, value); }
        }

        #endregion

        #region commands

        public ICommand OpenOriginalHomepageCommand { get; private set; }

        private void OpenOriginalHomepage(object param)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/anno-designer/");
        }

        public ICommand OpenProjectHomepageCommand { get; private set; }

        private void OpenProjectHomepage(object param)
        {
            System.Diagnostics.Process.Start("https://github.com/AnnoDesigner/anno-designer/");
        }

        public ICommand OpenWikiHomepageCommand { get; private set; }

        private void OpenWikiHomepage(object param)
        {
            System.Diagnostics.Process.Start("https://anno1800.fandom.com/wiki/Anno_Designer");
        }

        public ICommand CloseWindowCommand { get; private set; }

        private void CloseWindow(ICloseable window)
        {
            window?.Close();
        }

        #endregion
    }
}


