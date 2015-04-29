using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace UNWE_Navigator_Services.Models
{
    [DataContract]
    public class Point
    {

        [DataMember(Name="x")]
        public int X { get; set; }

        [DataMember(Name = "y")]
        public int Y { get; set; }
    }
}