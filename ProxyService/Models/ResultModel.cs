using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProxyService.Models
{
    [DataContract]
    public class ResultModel : ServiceMessage
    {

        [DataMember]
        [JsonProperty]
        public TimeSpan TotalTravelTime
        {
            get
            {
                return StampFive.TimeNow - StampOne.TimeNow;
            }
        }

        [DataMember]
        [JsonProperty]
        public TimeSpan Leg1
        {
            get
            {
                return StampTwo.TimeNow - StampOne.TimeNow;
            }
        }

        [DataMember]
        [JsonProperty]
        public TimeSpan Leg2
        {
            get
            {
                return StampThree.TimeNow - StampOne.TimeNow;
            }
        }

        [DataMember]
        [JsonProperty]
        public TimeSpan Leg3
        {
            get
            {
                return StampFour.TimeNow - StampOne.TimeNow;
            }
        }

        [DataMember]
        [JsonProperty]
        public TimeSpan Leg4
        {
            get
            {
                return StampFive.TimeNow - StampOne.TimeNow;
            }
        }

        public ResultModel InitFromServiceMessage(ServiceMessage message)
        {
            this.MessageId = message.MessageId;
            this.CommChannel = message.CommChannel;
            this.StampOne = message.StampOne;
            this.StampTwo = message.StampTwo;
            this.StampThree = message.StampThree;
            this.StampFour = message.StampFour;
            this.StampFive = message.StampFive;
            return this;
        }

    }

    public class BoxPlotChartModel
    {
        public string label { get; set; }
        public BoxPlotChartValues values { get; set; }
    }

    public class BoxPlotChartValues
    {
        [JsonProperty(propertyName: "Q1")]
        public decimal Q1 { get; set; }

        [JsonProperty(propertyName: "Q2")]
        public decimal Q2 { get; set; }
        [JsonProperty(propertyName: "Q3")]
        public decimal Q3 { get; set; }
        public decimal whisker_low { get; set; }
        public decimal whisker_high { get; set; }
        public List<decimal> outliers { get; set; } = new List<decimal>();
    }
}
