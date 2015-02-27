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
namespace SpatialServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "LayerEdit" in both code and config file together.
    public class LayerEdit : ILayerEdit
    {


       

          [WebInvoke(Method = "GET",
                 RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "addFeatures?adds={adds}")]

        //TEST:
        //http://localhost/arcgis/rest/services/Layers/EditLayer/addFeatures?adds={adds}
        //adds	 {     "geometry" : { "x" : -118.37, "y" : 34.086 },       "attributes" : {       "PermitNo" : 1,       "ProjectNo" : 2,       "Address" : "6905 Norway Dr",       "Inspector" : "EAO"     }   }	QUERY	RESOURCE




        public Stream  addFeatures(string adds)
        {

          
            try
            {

                var deserializer = new JavaScriptSerializer();
                Permit permit = deserializer.Deserialize<Permit>(adds);

                InsertPermit(permit);
                byte[] resultBytes = Encoding.UTF8.GetBytes(permit.attributes.PermitNo.ToString());
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
                return new MemoryStream(resultBytes);
            }

            catch (Exception ex)
            {
                byte[] resultBytes = Encoding.UTF8.GetBytes("failed");
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
                return new MemoryStream(resultBytes);
            }

           

         
        }



          private void InsertPermit(Permit permit)
          {
              try
              {
                  
                  string strSQLQuery = String.Format(@"INSERT INTO permits (geom, permitno, projectno, address, inspector, landuse)
                                                      VALUES (geometry::Point({0}, {1}, 2246), '{2}', '{3}', '{4}', '{5}','{6}')", permit.geometry.x, permit.geometry.y, permit.attributes.PermitNo, permit.attributes.ProjectNo, permit.attributes.Address, permit.attributes.Inspector, permit.attributes.landuse);


                  using (SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["strSQLConn"].ConnectionString))
                  {
                      using (SqlCommand cm = new SqlCommand(strSQLQuery, cn))
                      {
                          cn.Open();
                          cm.ExecuteNonQuery();


                      }
                  }

                 
              }




              catch (Exception ex)
              {
                 
              }

          }

          private class Permit
          {

              public Geometry geometry { get; set; }
              public Attributes attributes { get; set; }
          }

          private class Geometry
          {
              public double x { get; set; }
              public double y { get; set; }
          }

          private class Attributes
          {
              public int PermitNo { get; set; }
              public string ProjectNo { get; set; }
              public string Address { get; set; }
              public string Inspector { get; set; }
              public string landuse { get; set; }
          }
    }
}
