using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace UNWE_Navigator_Services.Models
{
    [DataContract]
    public class RouteModel
    {
        public RouteModel()
        {
            this.PathPoints = new List<Point>();
        }

        //[DataMember(Name="secPic")]
        //public string SecPic { get; set; }

        [DataMember(Name = "rotation")]
        public string Rotation { get; set; }

        [DataMember(Name = "arrCoords")]
        public string ArrCoords { get; set; }

        [DataMember(Name = "floorSectionID")]
        public string FloorSectionID { get; set; }

        [DataMember(Name = "floorSection")]
        public string FloorSection { get; set; }

        [DataMember(Name = "floor")]
        public int Floor { get; set; }

        [DataMember(Name = "roomFromID")]
        public int RoomFromID { get; set; }

        [DataMember(Name = "roomToID")]
        public int RoomToID { get; set; } 

        [DataMember(Name = "pictureFileName")]
        public string PictureFileName { get; set; }

        [DataMember(Name = "pathPoints")]
        public IEnumerable<Point> PathPoints { get; set; }
    }
}