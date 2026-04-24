namespace MechSim

open System
open Fable.Core
open Fable.Core.JsInterop
open Lit
open Browser.Types
open Browser.Dom

module App =

    type Material = { Name: string; E: float }

    module Materials =
        let Steel = { Name = "Acél (S235)"; E = 210.0e9 }
        let Aluminum = { Name = "Alumínium"; E = 70.0e9 }
        let Copper = { Name = "Vörösréz"; E = 120.0e9 }
        let Titanium = { Name = "Titán"; E = 110.0e9 }
        let All = [ Steel; Aluminum; Copper; Titanium ]

    type Section = 
        | Rectangular of width: float * height: float
        | Circular of radius: float
        | Pipe of outerRadius: float * thickness: float

    type BeamType = Cantilever | SimplySupported

    let calculateI section =
        match section with
        | Rectangular (w, h) -> (w * (h ** 3.0)) / 12.0
        | Circular r -> (Math.PI * (r ** 4.0)) / 4.0
        | Pipe (ro, t) -> 
            let ri = ro - t
            (Math.PI * ((ro ** 4.0) - (ri ** 4.0))) / 4.0

    let calculateDeflection (L: float) (P: float) (a: float) (E: float) (I: float) (beamType: BeamType) (x: float) =
        if x < 0.0 || x > L then 0.0
        else
            match beamType with
            | Cantilever ->
                if x <= a then (P * (x**2.0) * (3.0*a - x)) / (6.0 * E * I)
                else (P * (a**2.0) * (3.0*x - a)) / (6.0 * E * I)
            | SimplySupported ->
                let b = L - a
                if x <= a then (P * b * x * (L**2.0 - b**2.0 - x**2.0)) / (6.0 * L * E * I)
                else (P * a * (L - x) * (L**2.0 - a**2.0 - (L - x)**2.0)) / (6.0 * L * E * I)

    type State = {
        Length: float
        Force: float
        ForcePos: float
        Material: Material
        Section: Section
        BeamType: BeamType
    }

    let mutable state = {
        Length = 5.0
        Force = 1000.0
        ForcePos = 2.5
        Material = Materials.Steel
        Section = Rectangular(0.1, 0.2)
        BeamType = SimplySupported
    }

    let getPoints s =
        let width = 500.0
        let margin = 50.0
        let beamY = 100.0
        let scaleX = 400.0 / s.Length
        let I = calculateI s.Section
        [0.0 .. 0.1 .. s.Length]
        |> List.map (fun x ->
            let d = calculateDeflection s.Length s.Force s.ForcePos s.Material.E I s.BeamType x
            let vx = margin + (x * scaleX)
            let vy = beamY + (d * 1000.0)
            sprintf "%f,%f" vx vy)
        |> String.concat " "

    let update (render: unit -> unit) (fn: State -> State) =
        state <- fn state
        render()

    [<LitElement("mech-sim")>]
    let MechSim() =
        let _, render = Hook.useState(0)
        let forceUpdate() = render(fun n -> n + 1)
        let pts = getPoints state
        let fX = 50.0 + (state.ForcePos * (400.0 / state.Length))

        html$"""
        <div style="font-family: sans-serif; padding: 20px; max-width: 600px; margin: auto; background: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);">
            <h2 style="color: #2c3e50; text-align: center;">Gerenda Szimulátor</h2>
            
            <div style="text-align: center; margin-bottom: 20px;">
                <svg width="500" height="200" style="background: #f8f9fa; border: 1px solid #dee2e6;">
                    <polyline points="${pts}" fill="none" stroke="#3498db" stroke-width="3" />
                    <line x1="${fX}" y1="60" x2="${fX}" y2="95" stroke="red" stroke-width="2" />
                </svg>
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
                <div>
                    <label>Hossz: ${string state.Length}m</label>
                    <input type="range" min="1" max="20" .value="${string state.Length}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Length = float (e.target :?> HTMLInputElement).value })}" style="width: 100%;" />
                    
                    <label>Erő: ${string state.Force}N</label>
                    <input type="range" min="0" max="10000" .value="${string state.Force}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Force = float (e.target :?> HTMLInputElement).value })}" style="width: 100%;" />
                </div>
                <div style="display: flex; flex-direction: column; gap: 10px;">
                    <button @input="${fun _ -> update forceUpdate (fun s -> { s with BeamType = Cantilever })}">Konzolos</button>
                    <button @input="${fun _ -> update forceUpdate (fun s -> { s with BeamType = SimplySupported })}">Kéttámaszú</button>
                    <p style="font-size: 0.8em; color: #6c757d;">Anyag: ${state.Material.Name}</p>
                </div>
            </div>
        </div>
        """

    let register() = ()
