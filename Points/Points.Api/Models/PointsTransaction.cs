using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Points.Models
{
    [DataContract]
    public class PointsTransaction
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string UserId { get; set; }
        [DataMember]
        public string PayerName { get; set; }
        [DataMember]
        public int Points { get; set; }
        [DataMember]
        public DateTime TransactionDate { get; set; }
    }
}
