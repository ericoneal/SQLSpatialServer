using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using DuoVia.FuzzyStrings;
using System.Text.RegularExpressions;
using FuzzyString;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.IO;

namespace SpatialServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Geocoder" in both code and config file together.
    public class Geocoder : IGeocoder
    {



        
        [WebInvoke(Method = "GET",
                      ResponseFormat = WebMessageFormat.Json,
                      UriTemplate = "findAddressCandidates?SingleLine={id}")]

     

        //TEST
        //http://localhost/arcgis/rest/services/General/LWCComposite/GeocodeServer/findAddressCandidates?SingleLine=4225%20brownsboro%20glen
        //or
        //http://localhost/phonyarcserverservices/AddressSearch.findAddressCandidates.svc/findAddressCandidates?SingleLine=4225%20brownsboro%20glen


        public lstCandidates Geocode(string strAddress)
        {
            try
            {

                List<AddressData> lstAllAddresses = new List<AddressData>();
                List<string> lstStreetNames = new List<string>();
                string strFullAddress = "";
                string strStreetName = "";
                lstAllAddresses = QueryALLAddresses(System.Configuration.ConfigurationManager.ConnectionStrings["SQLQueryAllAddresses"].ConnectionString);

                foreach (AddressData addr in lstAllAddresses)
                {

                    strFullAddress = addr.FULL_ADDRESS;
                    strStreetName = StripHouseNo(strFullAddress);
                    lstStreetNames.Add(strStreetName.ToUpper().Trim());
                }


                HashSet<string> hshUniqueStreets = new HashSet<string>(lstStreetNames);

                List<Address> lstMatchAddresses = new List<Address>();
                string strStreetOnly = StripHouseNo(strAddress);
                string strHouseNo = GetHouseNO(strAddress);

                foreach (string address in hshUniqueStreets)
                {

                    if (address.ToUpper().Trim() == strAddress.ToUpper().Trim())
                    {
                        Address newAddress = new Address();
                        newAddress.address = address;
                        newAddress.score = 100;

                        lstMatchAddresses.Clear();
                        lstMatchAddresses.Add(newAddress);
                        break;
                    }


                    List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();
                    options.Add(FuzzyStringComparisonOptions.UseJaccardDistance);
                    options.Add(FuzzyStringComparisonOptions.UseNormalizedLevenshteinDistance);



                    bool bMatched = (address.ToUpper().Trim().ApproximatelyEquals(strStreetOnly.ToUpper().Trim(), options, FuzzyStringComparisonTolerance.Strong));

                    if (bMatched)
                    {
                        bool isEqual = address.ToUpper().Trim().FuzzyEquals(strAddress.ToUpper().Trim(), 0.3);
                        double coefficient = address.ToUpper().Trim().FuzzyMatch(strAddress.ToUpper().Trim());
                        double iMatchScore = Math.Round(coefficient * 100, 2);

                        if (isEqual)
                        {
                            string sql = System.Configuration.ConfigurationManager.ConnectionStrings["SQLQueryOneAddress"].ConnectionString + strHouseNo + " " + address.ToUpper().Trim() + "'";
                            List<Address> lst = QuerySQLAddress(sql);
                            foreach (Address newAddress in lst)
                            {

                                Location location = new Location();
                                newAddress.score = iMatchScore;
                                lstMatchAddresses.Add(newAddress);

                            }
                        }
                    }

                }




                lstCandidates Candidates = new lstCandidates();
                Candidates.candidates = lstMatchAddresses;
                return Candidates;



            }
            catch (Exception ex)
            {
                return null;
            }


        }



        private List<AddressData> QueryALLAddresses(string strSQLQuery)
        {
           
                SqlDataReader sqlReader = null;
                List<AddressData> lstAddress = new List<AddressData>();
                AddressData address;

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
                                address = new AddressData();
                                address.FULL_ADDRESS = sqlReader[System.Configuration.ConfigurationManager.ConnectionStrings["SQLFullAddressField"].ConnectionString].ToString();

                                lstAddress.Add(address);
                            }

                        }
                    }
                }

                return lstAddress;
            }




        private List<Address> QuerySQLAddress(string strSQLQuery)
        {
            try
            {
                SqlDataReader sqlReader = null;
                List<Address> lst = new List<Address>();
                Address address;
                Location location;
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
                                address = new Address();
                                address.address = sqlReader[System.Configuration.ConfigurationManager.ConnectionStrings["SQLFullAddressField"].ConnectionString].ToString();

                                SqlGeometry geom = DeserializeSqlGeometry(sqlReader, 1);
                              
                                location = new Location();
                                location.x = geom.STX.ToString();
                                location.y = geom.STY.ToString();
                                address.location = location;
                                lst.Add(address);

                              
                            }

                        }
                    }
                }

                return lst;
            }




            catch (Exception ex)
            {
                return null;
            }

        }



        private SqlGeometry DeserializeSqlGeometry(SqlDataReader sqlDataReader, int geometryColumnIndex)
        {
            SqlGeometry sqlGeometry = new SqlGeometry();
            System.Data.SqlTypes.SqlBytes bytes = sqlDataReader.GetSqlBytes(geometryColumnIndex);
            using (MemoryStream stream = new MemoryStream(bytes.Buffer))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    sqlGeometry.Read(reader);
                }
            }
            return sqlGeometry;
        }

        private string StripHouseNo(string strFullAddress)
        {
            StringBuilder sb = new StringBuilder();
            bool foundAlpha = false;
       
            foreach (char c in strFullAddress)
            {
                if (foundAlpha)
                {
                    sb.Append(c.ToString());
                }
                if ((!c.ToString().Any(Char.IsDigit)))
                {
                    if (!foundAlpha)
                    {
                        sb.Append(c.ToString());
                        if (c != ' ')
                        {
                            foundAlpha = true;
                        }
                    }

                }

            }
            return sb.ToString();
        }


        private string GetHouseNO(string strFullAddress)
        {
            StringBuilder sbHouseNo = new StringBuilder();

            foreach (char c in strFullAddress)
            {

                if ((c.ToString().Any(Char.IsDigit)))
                {
                    sbHouseNo.Append(c.ToString());
                }
                else
                {
                    return sbHouseNo.ToString();
                }

            }
            return sbHouseNo.ToString();

        }



    }



    public class AddressData
    {
     //   public ObjectId _id { get; set; }
        public Location location { get; set; }
        public string FULL_ADDRESS { get; set; }
        public long X_COORD { get; set; }
        public long Y_COORD { get; set; }
    }



    public class lstCandidates
    {
        public List<Address> candidates { get; set; }
    }

    public class Address
    {
        public string address { get; set; }
        public Location location { get; set; }
        public double score { get; set; }
        public Attributes attributes { get; set; }
    }

    public class Location
    {
        public string x { get; set; }
        public string y { get; set; }
    }

    public class Attributes
    {
        public string Loc_name { get; set; }
    }
}
