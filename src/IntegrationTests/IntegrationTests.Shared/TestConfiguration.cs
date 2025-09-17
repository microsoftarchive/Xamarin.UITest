using System.Runtime.Serialization;

namespace IntegrationTests.Shared
{
    [DataContract]
    public class TestConfiguration
    {
        [DataMember]
        public string Platform { get; set; }
        [DataMember]
        public string DeviceIdentifier { get; set; }
        [DataMember]
        public bool PhysicalDevice { get; set; }
        [DataMember]
        public bool PreferDeviceAgent { get; set; }
        [DataMember]
        public string XcodePath { get; set; }
        [DataMember]
        public bool Uia { get; set; }
        [DataMember]
        public bool DeviceAgent { get; set; }
        [DataMember]
        public bool Simulator { get; set; }
    }
}
