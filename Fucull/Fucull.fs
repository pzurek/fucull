#light
 
open System
open Cairo
open Gtk
open Gdk
 
Gtk.Application.Init()
 
let window = new Gtk.Window("Fucull")
let drawingArea = new Gtk.DrawingArea()
drawingArea.AddEvents((int)Gdk.EventMask.ButtonPressMask)
drawingArea.AddEvents((int)EventMask.ButtonReleaseMask)
drawingArea.AddEvents((int)EventMask.KeyPressMask)
drawingArea.AddEvents((int)EventMask.PointerMotionMask)
 
let points = new ResizeArray<Cairo.Point>()

let bottom (point1:(Cairo.Point), point2:(Cairo.Point)) = 
    match point1, point2 with
    | _ when point1.Y < point2.Y -> point1
    | _ when point1.Y > point2.Y -> point2

let top (point1:(Cairo.Point), point2:(Cairo.Point)) = 
    match point1, point2 with
    | _ when point1.Y > point2.Y -> point1
    | _ when point1.Y < point2.Y -> point2

let left (point1:(Cairo.Point), point2:(Cairo.Point)) = 
    match point1, point2 with
    | _ when point1.X < point2.X -> point1
    | _ when point1.X > point2.X -> point2
    | _ when point1.X = point2.X -> bottom (point1, point2)

let right (point1:(Cairo.Point), point2:(Cairo.Point)) = 
    match point1, point2 with
    | _ when point1.X > point2.X -> point1
    | _ when point1.X < point2.X -> point2
    | _ when point1.X = point2.X -> top (point1, point2)
 
let click (x, y) =
    let p = new Cairo.Point(x, y)
    points.Add(p)
    ()
 
let sketchCircle (c:Cairo.Context, xc, yc, r) =
    c.Save()
    c.Translate (xc, yc)
    c.MoveTo (r, 0.)
    c.Arc (0., 0., r, 0., (2.*Math.PI))
    c.ClosePath()
    c.Restore()
    
let drawBackground (c:Cairo.Context, w, h) =
    c.Save()
    c.Color <- new Cairo.Color(32./255., 74./255., 135./255.) //blue
    c.LineWidth <- 1.
    c.Rectangle(0.5, 0.5, (float)w - 1., (float)h - 1.)
    c.FillPreserve()
    c.Color <- new Cairo.Color(0., 0., 0.) //black
    c.Stroke()
    c.Restore()
    
let drawPoint (c:Context, point:Cairo.Point) =
    c.Save()
    let x = (float)point.X
    let y = (float)point.Y
    let radius = 3.5
    let lineLength = 11.
    let lineThickness = 1.
    sketchCircle(c, (Math.Floor(x):float) + 0.5, (Math.Floor(y):float) + 0.5, radius)
    c.Color <- new Cairo.Color(1., 1., 1.) //white
    c.LineWidth <- lineThickness
    c.MoveTo(Math.Floor(x) + 0.5, Math.Floor(y) - 5.)
    c.LineTo(Math.Floor(x) + 0.5, Math.Floor(y) + 6.)
    c.MoveTo(Math.Floor(x) - 5., Math.Floor(y) + 0.5)
    c.LineTo(Math.Floor(x) + 6., Math.Floor(y) + 0.5)
    c.Stroke()
    c.Restore()
    
let drawPoints (c:Context) =
    ResizeArray.iter (fun p -> drawPoint(c, p)) points
 
let draw (c:Context, w, h) =
    drawBackground (c, w, h)
    drawPoints (c)
    
let doTheVooDoo(da:Gtk.DrawingArea) =
    use drawable = da.GdkWindow
    let w, h = da.Allocation.Width, da.Allocation.Height
    use cairoContext = Gdk.CairoHelper.Create (drawable)
    draw(cairoContext, (float)w, (float)h)
    
window.WindowPosition <- Gtk.WindowPosition.Center
window.SetDefaultSize(640, 480)
window.Destroyed.Add (fun _ -> Application.Quit() )
 
drawingArea.ExposeEvent.Add (fun _ -> doTheVooDoo(drawingArea))
drawingArea.ButtonReleaseEvent.Add (fun e ->
    click((int)e.Event.X, (int)e.Event.Y)
    drawingArea.QueueDraw()
    )
 
window.Add(drawingArea)
window.ShowAll()
 
Gtk.Application.Run()