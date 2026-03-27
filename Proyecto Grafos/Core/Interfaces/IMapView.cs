using GMap.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Grafos.Views
{
    public interface IMapView
    {
        event EventHandler ViewLoaded;
        event EventHandler AcceptButtonClicked;
        event EventHandler ReturnButtonClicked;
        event EventHandler<PointLatLng> MapDoubleClicked;
        event EventHandler<string> MarkerClicked;
        event EventHandler<string> MarkerRightClicked;
        event EventHandler<string> GridCellClicked;
        event EventHandler EscapeKeyPressed;
        event MouseEventHandler MapRightClicked;

        string Description { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }

        void AddPhotoMarker(string name, double lat, double lng, string tooltip, Bitmap photo);
        void AddStandardMarker(string name, double lat, double lng, string tooltip);
        void AddTemporaryMarker(double lat, double lng, string tooltip);
        void ClearTemporaryMarker();
        void ClearAllMarkers();
        void RefreshMap();
        void RefreshGrid(object dataSource);
        void SelectGridRow(string personName);
        void UpdateStatistics(string text);
        void UpdateSelectionInfo(string text);
        void ShowMessage(string message, string caption = "Mensaje");
        void DrawRoutes(List<List<PointLatLng>> routes);
        void DrawLabeledSegments(List<RouteSegment> segments);
        void CenterMap(double lat, double lng);
        void CloseView(bool dialogResult);
        void SetUIMode(bool isSelectionMode, bool isReadOnly);
        void AddOrUpdateMarker(string name, double lat, double lng);
        void RemoveMarker(string name);

        // ── CAMBIO ──────────────────────────────────────────────────────────────
        // Se agregan BeginUpdate y EndUpdate a la interfaz para que MapPresenter
        // pueda llamarlos sin error CS1061. La implementación ya existe en MapForm.cs.
        void BeginUpdate();
        void EndUpdate();
        // ────────────────────────────────────────────────────────────────────────
    }
}