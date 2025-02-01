using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

[GlobalClass, Tool]
public partial class MultiplayerCamera3DComponent : Camera3D
{
	#region COMPONENT_VARIABLES
	[Export]
	private Node3D _playerContainer;

	private List<Monster> _playerList;


    private float _baseSize;
    [Export]
    public float MaxCameraSize { get; private set; } = 12f;
    [Export]
    public float MinCameraSize { get; private set; } = 8f;
    public float PlayerBoundsSizeDecrease { get; private set; } = 2.0f;
    [Export]
    public Vector2 CameraExpandMargin { get; private set; } = new Vector2(400f, 225f); //new Vector2(450f, 253.125f)
    [Export]
    public Vector2 PlayerBoundsMargin { get; private set; } = new Vector2(100f, 56.25f);
    [Export]
    public Vector2 OutsideUIMargin { get; private set; } = new Vector2(150f, 84.375f);
    [Export]
    public float ZoomOutSpeed { get; private set; } = 6f;
    [Export]
    public float ZoomInSpeed { get; private set; } = 2f;

    public static Rect2 CameraBounds { get; private set; }
    public static Rect2 PlayerBounds { get; private set; }
    public static Rect2 OutsideUIBounds { get; private set; }
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _baseSize = Size;
        _playerList = _playerContainer.GetChildrenOfType<Monster>().ToList();

        //CameraBounds = GetBoundsFromZoom(Camera.Zoom);
        //PlayerBounds = GetBoundsFromZoom(Camera.Zoom, -PlayerBoundsMargin);

        //GD.Print(GetZoomFromBounds(CameraBounds));

        //_minZoom = new Vector2(MinCameraSize, MinCameraSize);
        //_maxZoom = new Vector2(MaxCameraSize, MaxCameraSize);
        //_baseZoomInSpeed = ZoomInSpeed;
        //_baseZoomOutSpeed = ZoomOutSpeed;

    }
    public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
        if (Engine.IsEditorHint()) { return; }
        PlayerBounds = GetBoundsFromSize(Size * PlayerBoundsSizeDecrease);

        foreach (var player in _playerList)
        {
            var viewportRect = GetViewport().GetVisibleRect();
            var playerViewportPos = UnprojectPosition(player.GlobalPosition);
            if (!PlayerBounds.HasPoint(playerViewportPos))
            {
                var origSize = PlayerBounds.Size;
                var newBounds = PlayerBounds.Expand(playerViewportPos);
                var newSize = newBounds.Size;
                GD.Print("orig bounds size: ", origSize,
                    "; expanded bounds size: ", newSize);
                float xDiff = 0f;
                float zDiff = 0f;
                if (newBounds.Position.X < PlayerBounds.Position.X) {
                    xDiff = 1 - (newSize.X / origSize.X); }
                else { 
                    xDiff = (newSize.X / origSize.X) - 1; }
                GD.Print("orig bounds begin: ", PlayerBounds.Position,
                    "; orig bounds end: ", PlayerBounds.End);
                GD.Print("new bounds begin: ", newBounds.Position,
                    "; new bounds end: ", newBounds.End);
                if (newBounds.Position.Y < PlayerBounds.Position.Y) {
                    zDiff = 1 - (newSize.Y / origSize.Y);
                }
                else {
                    zDiff = (newSize.Y / origSize.Y) - 1;
                }
                var offsetPos = new Vector3(xDiff, 0, zDiff);
                offsetPos = offsetPos.Rotated(Vector3.Up, Rotation.Y);
                GD.Print("offset poss: ", offsetPos);
                GlobalPosition += offsetPos;
            }
        }
    }
    #endregion
    #region COMPONENT_HELPER
    //public void SetCameraZoom(float delta)
    //{
    //    var minBounds = GetBoundsFromZoom(_maxZoom, -CameraExpandMargin);
    //    //GD.Print("max zoom with margin: ", GetZoomFromBounds(minBounds));
    //    var maxBounds = GetBoundsFromZoom(_minZoom, -CameraExpandMargin);
    //    foreach (var robberPair in _global.Robbers)
    //    {
    //        var robber = robberPair.Value;
    //        if (GetParent() == robber) { continue; } //camera will move on it's own

    //        if (!minBounds.HasPoint(robber.GlobalPosition))
    //        {
    //            //var maybeBounds = CameraBounds.Expand(robber.Position);
    //            minBounds = minBounds.Expand(robber.GlobalPosition);
    //            var rotatedPos = Camera.GetScreenCenterPosition() + (robber.GlobalPosition - Camera.GetScreenCenterPosition()).Rotated(Mathf.Pi);
    //            minBounds = minBounds.Expand(rotatedPos); //expand both sides
    //            //GD.Print("screencenter: ", _camera.GetScreenCenterPosition(), ", pos: ", robber.Position, ", globpos: ", robber.GlobalPosition, ", negGlob: ", -robber.GlobalPosition,
    //            //    ", rotated: ", rotatedPos);
    //            //GD.Print("minBound size: ", minBounds.Size, ", CameraBoundsExpand Size: ", maybeBounds.Size, ", camera bounds size: ", CameraBounds.Size);
    //            if (minBounds.Size >= maxBounds.Size)
    //            {
    //                //_camera.Zoom = _minZoom;
    //                //return;
    //            }
    //        }
    //    }
    //    var newMarginZoom = GetZoomFromBounds(minBounds, -CameraExpandMargin);
    //    var lockedZoom = Mathf.Min(newMarginZoom.X, newMarginZoom.Y);
    //    var finalMarginZoom = new Vector2(lockedZoom, lockedZoom);
    //    var newBounds = GetBoundsFromSize(finalMarginZoom);//, CameraExpandMargin);
    //    //var newTestZoom = GetZoomFromBounds(newBounds);//, -CameraExpandMargin);

    //    //var zoomRatio = marginZoom / finalMarginZoom;
    //    //var newZoomTest = _camera.Zoom / zoomRatio;

    //    var newZoom = Mathf.Max(MinCameraZoom, finalMarginZoom.X);
    //    var finalZoom = Mathf.Min(MaxCameraZoom, newZoom);
    //    var finalZoomVec = new Vector2(finalZoom, finalZoom);

    //    //_camera.Zoom = new Vector2(finalZoom, finalZoom);
    //    var zoomDiff = finalZoom - Camera.Zoom.X;
    //    //float zoomChange;
    //    //if (zoomDiff > 0) //zooming in
    //    //{
    //    //    zoomChange = Mathf.Min(zoomDiff, ZoomSpeed);
    //    //}
    //    //else if (zoomDiff < 0) //zooming out
    //    //{
    //    //    if (zoomDiff / -ZoomSpeed > 5)
    //    //    {
    //    //        zoomChange = -ZoomSpeed * 2;
    //    //    }
    //    //    else
    //    //    {
    //    //        zoomChange = Mathf.Max(zoomDiff, -ZoomSpeed);
    //    //    }
    //    //}
    //    //else // no change
    //    //{
    //    //    return;
    //    //}
    //    //_camera.Zoom += new Vector2(zoomChange, zoomChange);
    //    if (zoomDiff > 0) // ZOOMING IN
    //    {
    //        Camera.Zoom = Camera.Zoom.Lerp(finalZoomVec, delta * ZoomInSpeed);
    //    }
    //    else if (zoomDiff < 0) // ZOOMING OUT
    //    {
    //        Camera.Zoom = Camera.Zoom.Lerp(finalZoomVec, delta * ZoomOutSpeed);
    //    }
    //    //GD.Print("curr bounds: ", CameraBounds, ", new bounds: ", newBounds, ", newMaginzoom: ", newMarginZoom, ", lockedZoom: ", lockedZoom, ", newZoom: ", finalZoom, ", zoomDiff: ", zoomDiff);
    //}
    public Rect2 GetBoundsFromSize(float size, Vector2? viewportOffset = null)
    {
        var sizePercentage = size / _baseSize;
        var viewportRect = GetViewport().GetVisibleRect();
        var viewportSize = viewportRect.Abs().Size;
        if (viewportOffset.HasValue)
        {
            viewportSize += viewportOffset.Value;
        }
        var cameraSize = viewportSize / sizePercentage;
        return new Rect2(viewportRect.GetCenter() /*_camera.GetTargetPosition()*/ - cameraSize / 2, cameraSize);
    }
    public float GetSizeFromBounds(Rect2 bounds, Vector2? viewportOffset = null)
    {
        var viewportRect = GetViewport().GetVisibleRect();
        var viewportSize = viewportRect.Abs().Size;
        if (viewportOffset.HasValue)
        {
            return _baseSize * ((viewportSize + viewportOffset.Value) / bounds.Size).Y;
        }
        return _baseSize * (viewportSize / bounds.Size).Y;
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
