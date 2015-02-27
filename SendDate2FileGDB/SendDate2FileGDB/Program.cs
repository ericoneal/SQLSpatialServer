using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.FileGDB;

namespace SendDate2FileGDB
{
    class Program
    {
        static void Main(string[] args)
        {
            ////try
            ////{
            ////    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Sourcecode\Other\SQLSpatialServer\SendDate2FileGDB\args.txt"))
            ////    {
            ////        foreach (string line in args)
            ////        {

            ////            file.WriteLine(line);

            ////        }
            ////    }
            ////}

            ////catch (Exception ex)
            ////{
            ////    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Sourcecode\Other\SQLSpatialServer\SendDate2FileGDB\err.txt"))
            ////    {
            ////        file.WriteLine(ex.Message );
            ////    }
            ////}

           



            Geometry geometry = new Geometry();
            geometry.x = Convert.ToDouble(args[0]);
            geometry.y = Convert.ToDouble(args[1]);

            Attributes attributes = new Attributes();
            attributes.PermitNo = Convert.ToInt32(args[2]);
            attributes.ProjectNo = args[3];
            attributes.Address = args[4];
            attributes.Inspector = args[5];
            attributes.landuse = args[6];

            Permit permit = new Permit();
            permit.geometry = geometry;
            permit.attributes = attributes;


            ////Geometry geometry = new Geometry();
            ////geometry.x = 1;
            ////geometry.y = 2;

            ////Attributes attributes = new Attributes();
            ////attributes.PermitNo = 3;
            ////attributes.ProjectNo = "test1";
            ////attributes.Address = "test2";
            ////attributes.Inspector = "eao";

            ////Permit permit = new Permit();
            ////permit.geometry = geometry;
            ////permit.attributes = attributes;


            var strGDBPath = System.Configuration.ConfigurationManager.ConnectionStrings["FileGDB"].ConnectionString;
            // Open the geodatabase.
            Geodatabase geodatabase = Geodatabase.Open(strGDBPath);

            // Open the Cities table.
            Table table = geodatabase.OpenTable("\\Permits");

            // Create a new feature for Cabazon.
            Row row = table.CreateRowObject();
            row.SetShort("PermitNo", (short)permit.attributes.PermitNo);
            row.SetString("ProjectNo", permit.attributes.ProjectNo);
            row.SetString("Address", permit.attributes.Address);
            row.SetString("Inspector", permit.attributes.Inspector);
            row.SetString("landuse", permit.attributes.landuse);


            // Create and assign a point geometry.
            PointShapeBuffer geom = new PointShapeBuffer();
            geom.Setup(ShapeType.Point);

            Point point = new Point(permit.geometry.x, permit.geometry.y);
            geom.point = point;
            row.SetGeometry(geom);

            table.Insert(row);
      


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
