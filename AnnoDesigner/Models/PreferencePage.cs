using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models;

public class PreferencePage : Notify
{
    private string _headerKeyForTranslation;
    private string _name;
    private Notify _viewModel;

    public string HeaderKeyForTranslation
    {
        get => _headerKeyForTranslation;
        set => UpdateProperty(ref _headerKeyForTranslation, value);
    }

    public string Name
    {
        get => _name;
        set => UpdateProperty(ref _name, value);
    }

    public Notify ViewModel
    {
        get => _viewModel;
        set => UpdateProperty(ref _viewModel, value);
    }
}
