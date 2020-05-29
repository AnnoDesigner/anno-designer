using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace AnnoDesigner.Models
{
    public interface INavigatedTo
    {
        void NavigatedTo(object extraData);
    }
}
