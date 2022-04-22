using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AnnoDesigner.Core.Layout.Presets;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner
{
    public interface IPresetLayoutHolder
    {
        List<IPresetLayout> Presets { get; set; }
    }

    public class PresetLayoutViewModel : Notify, IPresetLayoutHolder
    {
        private List<IPresetLayout> presets;
        private PresetLayout selectedPreset;
        private bool loadingLayouts;

        public PresetLayoutLoader PresetLayoutLoader { get; set; }

        public List<IPresetLayout> Presets
        {
            get { return presets; }
            set { UpdateProperty(ref presets, value); }
        }

        public PresetLayout SelectedPreset
        {
            get { return selectedPreset; }
            set { UpdateProperty(ref selectedPreset, value); }
        }

        public bool LoadingLayouts
        {
            get { return loadingLayouts; }
            set { UpdateProperty(ref loadingLayouts, value); }
        }

        public Func<bool> BeforeLayoutOpen { get; set; }

        public IAppSettings AppSettings { get; set; }

        public PresetLayoutViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Presets = new List<IPresetLayout>()
                {
                    new PresetLayoutDirectory()
                    {
                        Name = "Directory",
                        Presets = new List<IPresetLayout>()
                        {
                            new PresetLayout()
                            {
                                Info = new LayoutPresetInfo()
                                {
                                    Name = new MultilangInfo()
                                    {
                                        ["eng"] = "English",
                                        ["ger"] = "German",
                                        ["fra"] = "French",
                                        ["esp"] = "Spanish",
                                        ["pol"] = "Polish",
                                        ["rus"] = "Russian"
                                    },
                                    Description = "description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description description ",
                                    Author = "author",
                                    AuthorContact = "author@contact.com"
                                },
                                Layout = new Core.Layout.Models.LayoutFile()
                                {
                                    LayoutVersion = new Version(2, 1, 3, 12)
                                },
                                Images = new List<ImageSource>()
                            }
                        }
                    }
                };
                SelectedPreset = (Presets[0] as PresetLayoutDirectory).Presets[0] as PresetLayout;
            }
        }

        public async Task LoadLayoutsAsync()
        {
            LoadingLayouts = true;
            Presets = await PresetLayoutLoader.LoadAsync(AppSettings.PresetLayoutLocation);
            LoadingLayouts = false;
        }
    }
}
