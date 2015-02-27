using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SpatialServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISpatialQuery" in both code and config file together.
    [ServiceContract]
    public interface ISpatialQuery
    {
        [OperationContract]
        Stream QuerySpatial(string Layer, double x, double y);
    }
}
