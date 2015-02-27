using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.SqlServer.Types;

namespace SpatialServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SpatialQuery" in both code and config file together.
    public class SpatialQuery : ISpatialQuery
    {

 
        [WebInvoke(Method = "GET",
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "query?x={x}&y={y}&layer={layer}")]

        //TEST
        //http://localhost/arcgis/rest/services/Layers/query?x=1264176&y=297387.15625&layer=MetroCouncil
        //OR
        //http://localhost/phonyarcserverservices/AddressSearch.SpatialQuery.svc/query?x=1264176&y=297387.15625&layer=MetroCouncil


        public Stream QuerySpatial(string Layer, double x, double y)
        {

            try
            {


                Dictionary<string, string> dicFields = new Dictionary<string, string>();

                string strCoun_Name = QuerySQL(@"Select COUN_NAME from " + Layer + @" where geom.STIntersects(geometry::Point(" + x + @", " + y + @", 2246)) = 1");
                dicFields.Add("COUN_NAME", strCoun_Name);


                string strLanduse = QuerySQL(@"Select LANDUSE_NAME from " + "landuse" + @" where geom.STIntersects(geometry::Point(" + x + @", " + y + @", 2246)) = 1");
                dicFields.Add("LANDUSE", strLanduse);

                StringBuilder sb = new StringBuilder();
                sb.Append("{\"features\" : [{\"attributes\" : ");



                string jsonAttributes = new JavaScriptSerializer().Serialize(dicFields);
                sb.Append(jsonAttributes + "}]}");


                byte[] resultBytes = Encoding.UTF8.GetBytes(sb.ToString());
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
                return new MemoryStream(resultBytes);



            }


            catch (Exception ex)
            {
                return null;

            }


        }



        private string QuerySQL(string strSQLQuery)
        {
            try
            {
                SqlDataReader sqlReader = null;


                using (SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["strSQLConn"].ConnectionString))
                {
                    using (SqlCommand cm = new SqlCommand(strSQLQuery, cn))
                    {
                        cn.Open();
                        sqlReader = cm.ExecuteReader();


                        if (sqlReader.HasRows == true)
                        {

                            while (sqlReader.Read())
                            {
                                return sqlReader[0].ToString();


                            }

                        }
                    }
                }

                return null;
            }




            catch (Exception ex)
            {
                return null;
            }

        }



        public class CouncilDistrict
        {
         //   public ObjectId _id { get; set; }
            public string COUN_NAME { get; set; }
            public Polygon Polygon { get; set; }
        }

        public class Polygon
        {
            public double[] Coordinates { get; set; }

        }


    }





}
