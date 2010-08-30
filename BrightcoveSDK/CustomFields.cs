using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BrightcoveSDK
{
    [CollectionDataContract]
    public class CustomFields : Dictionary<string, string>
    { }
}
