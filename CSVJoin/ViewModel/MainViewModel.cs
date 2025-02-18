using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace CSVJoin.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public static ObservableCollection<TicketViewModel> Tickets { get; set; }
        public static ObservableCollection<GISInfoViewModel> GIS { get; set; }
        public static ObservableCollection<ITInfoViewModel> ITs { get; set; }
        public static ObservableCollection<SegmentViewModel> Segments { get; set; }
        public static ObservableCollection<MagViewModel> Mags { get; set; }
        public static ObservableCollection<CombinedResult> Combined { get; set; }
        public static ObservableCollection<GroupedDataItem> Grouped { get; set; }
        public CollectionViewSource _ticketCollection {  get; set; }
        public CollectionViewSource _gisCollection { get; set; }
        public CollectionViewSource _itsCollection { get; set; }
        public CollectionViewSource _segCollection { get; set; }
        public CollectionViewSource _magCollection { get; set; }
        public CollectionViewSource _combinedCollection { get; set; }
        private ICollectionView _ticketView;
        public ICollectionView ticketView { get; set; }
        public ICollectionView gisView { get; set; }
        public ICollectionView itsView { get; set; }
        public ICollectionView segView { get; set; }
        public ICollectionView magView { get; set; }
        public ICollectionView combinedView { get; set; }

        public MainViewModel()
        {
            Tickets = new ObservableCollection<TicketViewModel>();
            GIS = new ObservableCollection<GISInfoViewModel>();
            ITs = new ObservableCollection<ITInfoViewModel>();
            Segments = new ObservableCollection<SegmentViewModel>();
            Mags = new ObservableCollection<MagViewModel>();
            Combined = new ObservableCollection<CombinedResult>();
            
            _ticketCollection = new CollectionViewSource() { Source = Tickets };
            _gisCollection = new CollectionViewSource() { Source= GIS };
            _itsCollection = new CollectionViewSource() { Source = ITs };
            _segCollection = new CollectionViewSource() { Source = Segments };
            _magCollection = new CollectionViewSource() { Source = Mags };
            _combinedCollection = new CollectionViewSource() { Source = Combined };

            ticketView = _ticketCollection.View;
            gisView = _gisCollection.View;
            itsView = _itsCollection.View;
            segView = _segCollection.View;
            magView = _magCollection.View;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

    }

    public class TicketViewModel : IIndexable
    {

        public string RoadID { get; set; }
        public int CellNo { get; set; }
        public int TicketNo { get; set; }
        public string ParkID { get; set; }
        public string this[int index]
        {
            get
            {
                return index switch
                {
                    0 => RoadID,
                    1 => CellNo.ToString(),
                    2 => TicketNo.ToString(),
                    3 => ParkID,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }

    public class GISInfoViewModel : IIndexable
    {
        public int? Sno { get; set; }
        public string Pkid { get; set; }
        public DateTime? EditTime { get; set; }
        public int? PkType { get; set; }
        public string RdCode { get; set; }
        public string PkRoad { get; set; }
        public string ParkID { get; set; }
        public string this[int index]
        {
            get
            {
                return index switch
                {
                    0 => Sno.ToString(),
                    1 => Pkid.ToString(),
                    2 => EditTime.ToString(),
                    3 => PkType.ToString(),
                    4 => RdCode,
                    5 => PkRoad,
                    6 => ParkID,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }

    public class ITInfoViewModel : IIndexable
    {
        public string PId { get; set; }
        public string RoadSegID { get; set; }
        public string PsId { get; set; }
        public string RoadSegName { get; set; }
        public string ParkID { get; set; }
        public string this[int index]
        {
            get
            {
                return index switch
                {
                    0 => PId,
                    1 => RoadSegID,
                    2 => PsId,
                    3 => RoadSegName,
                    4 => ParkID,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }

    public class SegmentViewModel : IIndexable
    {
        public string segID { get; set; }
        public string segName { get; set; }
        public string this[int index]
        {
            get
            {
                return index switch
                {
                    0 => segID,
                    1 => segName,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }

    public class MagViewModel : IIndexable
    {
        public string section_id { get; set; }
        public string ps_id_str { get;set; }
        public int status { get; set; }
        public string vendorcode { get; set; }
        public DateTime data_dt { get; set; }
        public DateTime receiveTime { get; set; }
        public string ParkID { get; set; }
        public string this[int index]
        {
            get
            {
                return index switch
                {
                    0 => section_id,
                    1 => ps_id_str,
                    2 => status.ToString(),
                    3 => vendorcode,
                    4 => data_dt.ToString(),
                    5 => receiveTime.ToString(),
                    6 => ParkID.ToString(),
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }

    public class CombinedResult
    {
        public string RoadID { get; set; }
        public string RoadName {  get; set; }
        public string ParkID { get; set; }
        public string Cellno { get; set; }
        public string Ticketno { get; set; }
        public string GisParkID { get; set; }
        public string ItsParkID { get; set; }
        public string MagID { get; set; }
        public string Status { get; set; }
        public string VendorCode { get; set; }
        public string ReceiveTime { get; set; }
        public int GisYn { get; set; }
        public int ItYn { get; set; }
        public string GisOx { get; set; }
        public string ItOx { get; set; }
        private int _errType; 
        public int ErrType { 
            get { return _errType; } 
            set { if (_errType != value) { 
                    _errType = value; OnPropertyChanged(nameof(ErrType)); 
                }
            } 
        }
        public event PropertyChangedEventHandler PropertyChanged; 
        protected void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        //public override bool Equals(object obj)
        //{
        //    if (obj is CombinedResult other)
        //    {
        //        return RoadID == other.RoadID &&
        //               ParkID == other.ParkID &&
        //               GisParkID == other.GisParkID &&
        //               ItsParkID == other.ItsParkID &&
        //               Cellno == other.Cellno &&
        //               Ticketno == other.Ticketno &&
        //               MagID == other.MagID &&
        //               VendorCode == other.VendorCode &&
        //               Status == other.Status &&
        //               ReceiveTime == other.ReceiveTime;
        //    }
        //    return false;
        //}

        //public override int GetHashCode()
        //{
        //    return (RoadID, ParkID, Cellno, Ticketno, GisParkID, ItsParkID).GetHashCode();
        //}

        //public string this[int index]
        //{
        //    get
        //    {
        //        return index switch
        //        {
        //            0 => RoadID,
        //            1 => ParkID,
        //            2 => Cellno,
        //            3 => Ticketno,
        //            4 => GisParkID,
        //            5 => ItsParkID,
        //            6 => MagID,
        //            7 => Status,
        //            8 => VendorCode,
        //            9 => ReceiveTime,
        //            10 => GisYn.ToString(),
        //            11 => ItYn.ToString(),
        //            12 => GisOx,
        //            13 => ItOx,
        //            14 => ErrType,
        //            _ => throw new IndexOutOfRangeException()
        //        };
        //    }
        //}
    }

}

public class GroupedDataItem
{
    public string RoadID { get; set; }
    public string RoadName { get; set; }
    public int GisParkIDCount { get; set; }
    public int ItsParkIDCount { get; set; }
    public int TotalGisYn { get; set; }
    public int TotalItYn { get; set; }

}

public interface IIndexable
{
    string this[int index] { get; }
}
