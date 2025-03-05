using CSVJoin.ViewModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CSVJoin
{
    public class TableJoiner
    {
        public List<CombinedResult> JoinTables(
            ObservableCollection<TicketViewModel> tickets,
            ObservableCollection<GISInfoViewModel> gis,
            ObservableCollection<ITInfoViewModel> its,
            ObservableCollection<SegmentViewModel> segments,
            ObservableCollection<MagViewModel> mags)
        {
            var ticketDict = new ConcurrentDictionary<string, TicketViewModel>(
                tickets.GroupBy(t => t.ParkID).ToDictionary(g => g.Key, g => g.First()));
            var gisDict = new ConcurrentDictionary<string, GISInfoViewModel>(
                gis.GroupBy(g => g.ParkID).ToDictionary(g => g.Key, g => g.First()));
            var itsDict = new ConcurrentDictionary<string, ITInfoViewModel>(
                its.GroupBy(i => i.ParkID).ToDictionary(g => g.Key, g => g.First()));

            var allParkIDs = new HashSet<string>(ticketDict.Keys
                .Concat(gisDict.Keys)
                .Concat(itsDict.Keys)
                .Concat(mags.Select(m => m.ParkID)));

            var combinedResults = new ConcurrentBag<CombinedResult>();

            Parallel.ForEach(allParkIDs, parkID =>
            {
                var combinedResult = new CombinedResult
                {
                    ParkID = parkID
                };

                if (ticketDict.TryGetValue(parkID, out var ticket))
                {
                    combinedResult.RoadID = ticket.RoadID;
                    combinedResult.Cellno = ticket.CellNo.ToString();
                    combinedResult.Ticketno = ticket.TicketNo.ToString();
                }

                if (gisDict.TryGetValue(parkID, out var gisItem))
                {
                    combinedResult.RoadID = gisItem.RdCode;
                    combinedResult.GisParkID = gisItem.ParkID;
                }

                if (itsDict.TryGetValue(parkID, out var itsItem))
                {
                    combinedResult.RoadID = itsItem.RoadSegID;
                    combinedResult.ItsParkID = itsItem.ParkID;
                }

                var mag = mags.FirstOrDefault(m => m.ParkID == parkID);
                if (mag != null)
                {
                    combinedResult.MagID = mag.ParkID;
                    combinedResult.Status = mag.status.ToString();
                    combinedResult.VendorCode = mag.vendorcode;
                    combinedResult.ReceiveTime = mag.data_dt.ToString();
                }

                combinedResults.Add(combinedResult);
            });

            // Filter CombinedResults by RoadID present in segments
            var filteredResults = (from result in combinedResults.AsParallel()
                                   join segment in segments.AsParallel() on result.RoadID equals segment.segID
                                   orderby segment.segID, result.Cellno
                                   select new CombinedResult
                                   {
                                       ParkID = result.ParkID,
                                       Cellno = result.Cellno,
                                       Ticketno = result.Ticketno,
                                       GisParkID = result.GisParkID,
                                       ItsParkID = result.ItsParkID,
                                       MagID = result.MagID,
                                       Status = result.Status,
                                       VendorCode = result.VendorCode,
                                       ReceiveTime = result.ReceiveTime,
                                       RoadID = segment.segID ?? string.Empty,
                                       RoadName = segment.segName ?? string.Empty,
                                       GisYn = 0,
                                       ItYn = 0,
                                       GisOx = null,
                                       ItOx = null,
                                       ErrType = 0
                                   }
                                   ).ToList(); // Materialize the results into a list

            // Update the filtered results
            foreach (var r in filteredResults)
            {
                if (!string.IsNullOrEmpty(r.Cellno) && !string.IsNullOrEmpty(r.GisParkID) && string.IsNullOrEmpty(r.ItsParkID))
                { r.ItYn = -1; r.GisOx = "Y"; r.ItOx = "N"; r.ErrType = 1; }
                else if (!string.IsNullOrEmpty(r.Cellno) && string.IsNullOrEmpty(r.GisParkID) && !string.IsNullOrEmpty(r.ItsParkID))
                { r.GisYn = -1; r.GisOx = "N"; r.ItOx = "Y"; r.ErrType = 2; }
                else if (string.IsNullOrEmpty(r.Cellno) && !string.IsNullOrEmpty(r.GisParkID) && string.IsNullOrEmpty(r.ItsParkID))
                { r.GisYn = -1; r.GisOx = "Y"; r.ItOx = "N"; r.ErrType = 3; }
                else if (string.IsNullOrEmpty(r.Cellno) && string.IsNullOrEmpty(r.GisParkID) && !string.IsNullOrEmpty(r.ItsParkID))
                { r.ItYn = -1; r.GisOx = "N"; r.ItOx = "Y"; r.ErrType = 4; }
                else if (!string.IsNullOrEmpty(r.Cellno) && string.IsNullOrEmpty(r.GisParkID) && string.IsNullOrEmpty(r.ItsParkID))
                { r.ErrType = 5; r.GisOx = "N"; r.ItOx = "N"; r.ErrType = 5; }
                else if (string.IsNullOrEmpty(r.Cellno) && string.IsNullOrEmpty(r.GisParkID) && string.IsNullOrEmpty(r.ItsParkID))
                { r.ErrType = 6; r.GisOx = "N"; r.ItOx = "N"; r.ErrType = 6; }
            }

            return filteredResults;
        }
    }

}
