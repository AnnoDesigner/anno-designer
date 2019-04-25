using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.model;

namespace AnnoDesigner.viewmodel
{
    public class StatisticsViewModel : Notify
    {
        private string _usedTiles;
        private ObservableCollection<StatisticsBuilding> _buildings;

        public StatisticsViewModel()
        {
            UsedTiles = "308 Tiles";
        }

        public string UsedTiles
        {
            get { return _usedTiles; }
            set { UpdateProperty(ref _usedTiles, value); }
        }

        public ObservableCollection<StatisticsBuilding> Buildings
        {
            get { return _buildings; }
            set { UpdateProperty(ref _buildings, value); }
        }

        public void UpdateStatistics(List<AnnoObject> placedObjects)
        {
            // calculate bouding box
            var boxX = placedObjects.Max(_ => _.Position.X + _.Size.Width) - placedObjects.Min(_ => _.Position.X);
            var boxY = placedObjects.Max(_ => _.Position.Y + _.Size.Height) - placedObjects.Min(_ => _.Position.Y);
            // calculate area of all buildings
            var minTiles = placedObjects.Where(_ => !_.Road).Sum(_ => _.Size.Width * _.Size.Height);

            // format lines            
            //informationLines.AppendFormat(" {0}x{1}", boxX, boxY).AppendLine();
            //informationLines.AppendFormat(" {0} Tiles", boxX * boxY).AppendLine();

            UsedTiles = string.Format("{0} Tiles", boxX * boxY);
        }
    }
}
