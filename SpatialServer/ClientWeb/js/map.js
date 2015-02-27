
var app = {};
var CurrentAddress;


function initialize() {

   
    app.map = null; app.toolbar = null; app.tool = null; app.symbols = null; app.printer = null;
    require([
      "esri/map", 
      "esri/layers/ArcGISTiledMapServiceLayer", "esri/layers/ArcGISDynamicMapServiceLayer",

      "esri/symbols/SimpleMarkerSymbol", "esri/symbols/SimpleLineSymbol",
      "esri/symbols/SimpleFillSymbol", "esri/graphic",
    "dojo/dom-class",
      "esri/geometry/Point",
      "esri/SpatialReference",
      "esri/config",
      "dojo/_base/array", "esri/Color", "dojo/parser",
      "dojo/query", "dojo/dom", "dojo/dom-construct",
      "dijit/form/CheckBox", "dijit/form/Button",
      "esri/dijit/Popup", "esri/dijit/PopupTemplate",
      "dijit/layout/BorderContainer", "dijit/layout/ContentPane", "dojo/domReady!"
    ], function (
      Map, 
      ArcGISTiledMapServiceLayer, ArcGISDynamicMapServiceLayer,

      SimpleMarkerSymbol, SimpleLineSymbol,
      SimpleFillSymbol, Graphic,domClass,
       Point,SpatialReference,
      esriConfig,
      arrayUtils, Color, parser,
      query, dom, domConstruct,
      CheckBox, Button,  Popup, PopupTemplate
    ) {
        parser.parse();

        esriConfig.defaults.io.proxyUrl = "/proxy/";
        loading = dojo.byId("loadingImg");

        var fill = new SimpleFillSymbol("solid", null, new Color("#A4CE67"));
        var popup = new esri.dijit.Popup({
            fillSymbol: fill,
            titleInBody: false
        }, domConstruct.create("div"));
        //Add the dark theme which is customized further in the <style> tag at the top of this page
        domClass.add(popup.domNode, "dark");        var template = new esri.dijit.PopupTemplate({
            title: "Address",
            description: "Address"
        });

        var initExtent = new esri.geometry.Extent(
       {
           "xmin": 1117635.72905385,
           "ymin": 185276.998528281,
           "xmax": 1331611.42349829,
           "ymax": 331544.359639392,
           "spatialReference": { "wkid": 2246 }
       });

        app.map = new Map("map", {
            extent: initExtent,
            infoWindow: popup
        });
        app.map.on("load", function () {
            hideLoading();
        });

      

        var url = "http://ags2.lojic.org/ArcGIS/rest/services/External/StreetMapGray/MapServer";
        var tiledLayer = new ArcGISTiledMapServiceLayer(url, { "id": "StreetMap" });
        app.map.addLayer(tiledLayer);

    

        // find the divs for buttons
        query(".input").forEach(function (btn) {
            var button = new Button({
                label: btn.innerHTML,
                onClick: function () {
                    SetClick(this.id);
                }
            }, btn);
        });

        function SetClick(type) {
     
            if (type == 'btnVerifyAddress') {
                VerifyAddress($('#txtAddress').val());
            }

            if (type == 'btnAddPermit') {
                AddPermit($('#txtPermitNo').val(), $('#txtProjectNo').val(), $('#txtInspector').val(), $('#txtLanduse').val());
            }
        }



        function AddPermit(PermitNo, ProjectNo,  Inspector, Landuse) {
            var geom = { "x": CurrentAddress.location.x, "y": CurrentAddress.location.y };
            var attributes = { "PermitNo": PermitNo, "ProjectNo": ProjectNo, "Address": CurrentAddress.address, "Inspector": Inspector, "landuse": Landuse }

            var adds = { "geometry": geom, "attributes": attributes }


            $.ajax({
                url: "http://localhost/SQLSPatialservices/SpatialServer.LayerEdit.svc/addFeatures?adds=" + JSON.stringify(adds)
            })
            //$.post("http://localhost/SQLSPatialservices/SpatialServer.LayerEdit.svc/addFeatures?",
            //{
            //    adds: '{ "geometry": { "x": ' + geom.x + ', "y": ' + geom.x + ' }, "attributes": { "PermitNo": ' + PermitNo + ', "ProjectNo": ' + ProjectNo + ', "Address": "' + CurrentAddress.address + '", "Inspector": "' + Inspector + '" } }'
            //},
            //function (data, status) {
            //    alert("Data: " + data + "\nStatus: " + status);
            //});
        }







        function VerifyAddress(inputAddress) {
            showLoading();
            $('#txtCouncilMan').val('');


            $.ajax({
                url: "http://localhost/sqlspatialservices/SpatialServer.Geocoder.svc/findAddressCandidates?SingleLine=" + inputAddress

            })

            .then(function (response) {

                $.each(response.candidates, function (index, value) {
                    Zoom2Address(value.address, value.location.x, value.location.y);
                    CurrentAddress = value;
                });
   
            });

        }

    });





}








function Zoom2Address(address, x,y) {
    // alert(x + "," + y);
    var point = new esri.geometry.Point(x, y, new esri.SpatialReference({ wkid: 2246 }));
    if (point !== undefined) {

        app.map.infoWindow.clearFeatures();
        //popup.clearFeatures();
        app.map.infoWindow.setTitle(address);

        app.map.infoWindow.show(point);


       
        app.map.setLevel(9);
        app.map.centerAt(point);

        GetPolygonInfo("MetroCouncil", point);
        
    }
}


function GetPolygonInfo(strLayer, point) {



    $.ajax({
        url: "http://localhost/sqlspatialservices/SpatialServer.SpatialQuery.svc/query?x=" + point.x + "&y=" + point.y + "&layer=" + strLayer

    })

  .then(function (response) {

      var feature = JSON.parse(response);
      //alert(feature);
      $('#txtCouncilMan').val(feature.features[0].attributes.COUN_NAME);
      $('#txtLanduse').val(feature.features[0].attributes.LANDUSE);

      hideLoading();


  });

}





function showLoading() {
    esri.show(loading);
    app.map.disableMapNavigation();
    app.map.hideZoomSlider();
}

function hideLoading(error) {
    esri.hide(loading);
    app.map.enableMapNavigation();
    app.map.showZoomSlider();
}