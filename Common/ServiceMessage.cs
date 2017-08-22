using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Data;
using Newtonsoft.Json;

namespace Common
{
    [DataContract]
    public class ServiceMessage
    {
        [DataMember]
        public string SessionId { get; set; }

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

    public class JsonNetServiceMessageSerializer : Microsoft.ServiceFabric.Data.IStateSerializer<ServiceMessage>
    {
        ServiceMessage IStateSerializer<ServiceMessage>.Read(BinaryReader binaryReader)
        {
            var message = new ServiceMessage();
            try
            {
                using (StreamReader sr = new StreamReader(binaryReader.BaseStream))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();

                        message = serializer.Deserialize<ServiceMessage>(reader);
                    }
                }
            }
            catch(Exception e)
            {
                string msg = e.Message;
            }

            return message;
        }

        void IStateSerializer<ServiceMessage>.Write(ServiceMessage value, BinaryWriter binaryWriter)
        {
            var msgJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(msgJson);
            binaryWriter.Write(bytes);
        }

        ServiceMessage IStateSerializer<ServiceMessage>.Read(ServiceMessage baseValue, BinaryReader binaryReader)
        {
            return ((Microsoft.ServiceFabric.Data.IStateSerializer<ServiceMessage>)this).Read(binaryReader);
        }

        void IStateSerializer<ServiceMessage>.Write(ServiceMessage baseValue, ServiceMessage targetValue, BinaryWriter binaryWriter)
        {
            ((Microsoft.ServiceFabric.Data.IStateSerializer<ServiceMessage>)this).Write(targetValue, binaryWriter);
        }
    }
}
