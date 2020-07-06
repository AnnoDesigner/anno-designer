using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace InfoboxParser.Tests
{
    public class MemberDataSerializer<T> : IXunitSerializable
    {
        public T Object { get; private set; }

        public MemberDataSerializer()
        {
        }

        public MemberDataSerializer(T objectToSerialize)
        {
            Object = objectToSerialize;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Object = JsonConvert.DeserializeObject<T>(info.GetValue<string>("objValue"));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            var json = JsonConvert.SerializeObject(Object);
            info.AddValue("objValue", json);
        }
    }
}
