using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class LicensesViewModel : Notify
    {
        private ObservableCollection<LicenseInfo> _Licenses;
        public ObservableCollection<LicenseInfo> Licenses
        {
            get => _Licenses;
            set => UpdateProperty(ref _Licenses, value);
        }

        private const string APACHE_2 = "Apache-2.0 License";
        private const string MS_PL = "Microsoft Public License (Ms-PL)";
        //private const string MIT = "MIT License";
        //etc

        public LicensesViewModel()
        {
            Licenses = new ObservableCollection<LicenseInfo>
            {
                new LicenseInfo
                {
                    License = APACHE_2,
                    LicenseURL = "https://github.com/google/material-design-icons/blob/master/LICENSE",
                    ProjectName = "Material design icons",
                    ProjectWebsite = "https://github.com/google/material-design-icons",
                    Assets = new List<string>()
                    {
                        "left-click.png",
                        "middle-click.png",
                        "right-click.png",
                        "chevron-up.png"
                    }
                },
                new LicenseInfo
                {
                    License=MS_PL,
                    LicenseURL="https://licenses.nuget.org/MS-PL",
                    ProjectName="Extended WPF Toolkit™ (3.8.2)",
                    ProjectWebsite="https://github.com/xceedsoftware/wpftoolkit"
                }
            };



            // Junk licenses for testing only //


            //Licenses.Add(new LicenseInfo()
            //{
            //    License = MIT,
            //    LicenseURL = "https://github.com/google/material-design-icons/blob/master/LICENSE",
            //    ProjectName = "Material design icons Some extra test long text",
            //    ProjectWebsite = "https://github.com/google/material-design-iconsAnextralongUrlAnextralongUrlAnextralongUrl",
            //    Assets = new List<string>()
            //    {
            //        "left-click.png",
            //        "middle-click.png",
            //        "right-click.png",
            //        "left-click.png",
            //        "middle-click.png",
            //        "right-click.png",
            //        "left-click.png",
            //        "middle-click.png",
            //        "right-click.png"
            //    }
            //});
        }

    }
}
