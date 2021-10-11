using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.CustomEventArgs;
using AnnoDesigner.Undo;

namespace AnnoDesigner.Models
{
    public interface IAnnoCanvas : IHotkeySource
    {
        event EventHandler<EventArgs> ColorsInLayoutUpdated;
        event Action<List<LayoutObject>> OnClipboardChanged;
        event EventHandler<UpdateStatisticsEventArgs> StatisticsUpdated;
        event EventHandler<FileLoadedEventArgs> OnLoadedFileChanged;
        event Action<string> OnStatusMessageChanged;
        event Action<LayoutObject> OnCurrentObjectChanged;
        event EventHandler<OpenFileEventArgs> OpenFileRequested;
        event EventHandler<SaveFileEventArgs> SaveFileRequested;

        QuadTree<LayoutObject> PlacedObjects { get; set; }
        List<LayoutObject> SelectedObjects { get; set; }
        List<LayoutObject> ClipboardObjects { get; }
        BuildingPresets BuildingPresets { get; }
        Dictionary<string, IconImage> Icons { get; }
        IUndoManager UndoManager { get; }
        bool RenderGrid { get; set; }
        bool RenderInfluences { get; set; }
        bool RenderTrueInfluenceRange { get; set; }
        bool RenderHarborBlockedArea { get; set; }

        bool RenderLabel { get; set; }
        bool RenderIcon { get; set; }
        string LoadedFile { get; set; }
        int GridSize { get; set; }

        void ForceRendering();
        void SetCurrentObject(LayoutObject obj);
        void ResetZoom();
        void Normalize();
        void Normalize(int border);
        void RaiseStatisticsUpdated(UpdateStatisticsEventArgs args);
        void RaiseColorsInLayoutUpdated();
        Rect ComputeBoundingRect(IEnumerable<LayoutObject> objects);
        bool CheckUnsavedChanges();
        void CheckUnsavedChangesBeforeCrash();
        void UpdatePlacedObjectCount();
    }
}
