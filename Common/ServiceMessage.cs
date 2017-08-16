using System;
using System.Runtime.Serialization;


namespace Common
{
    [DataContract]
    public class ServiceMessage
    {
        [DataMember]
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        [DataMember]
        public string CommChannel { get; set; } = string.Empty;
        [DataMember]
        public string MessageJson { get; set; } = string.Empty;
        [DataMember]
        public VisitStamp StampOne { get; set; } = new VisitStamp();
        [DataMember]
        public VisitStamp StampTwo { get; set; } = new VisitStamp();
        [DataMember]
        public VisitStamp StampThree { get; set; } = new VisitStamp();
        [DataMember]
        public VisitStamp StampFour { get; set; } = new VisitStamp();
        [DataMember]
        public VisitStamp StampFive { get; set; } = new VisitStamp();


    }


    public class VisitStamp
    {
        [DataMember]
        public bool Visited { get; set; }
        [DataMember]
        public DateTime TimeNow { get; set; }
    }
}
