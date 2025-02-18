using CSVJoin.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CSVJoin
{
    public class GroupBy
    {
        public ObservableCollection<GroupedDataItem> TableGroup(ObservableCollection<CombinedResult> CombinedView)
        {
            // Group the data, sum GisYn and ItYn, count items, and count non-null GisParkID and ItsParkID
            var GroupedView = new ObservableCollection<GroupedDataItem>(
                CombinedView.GroupBy(item => item.RoadID)
                            .Select(group => new GroupedDataItem
                            {
                                RoadID = group.Key,
                                RoadName = group.First().RoadName,
                                GisParkIDCount = group.Count(item => !string.IsNullOrEmpty(item.GisParkID)),
                                ItsParkIDCount = group.Count(item => !string.IsNullOrEmpty(item.ItsParkID)),
                                TotalGisYn = group.Where(x=>(x.ErrType != 5 && x.ErrType!=0)).Sum(item => item.GisYn==-1 ? 1 : 0),
                                TotalItYn = group.Where(x => (x.ErrType != 5 && x.ErrType != 0)).Sum(item => item.ItYn==-1 ? 1 : 0)
                            })
            );
            return GroupedView;
        }
    }
}
