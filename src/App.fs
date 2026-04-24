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

    let renderBeam s =
        let width = 600.0
        let height = 200.0
        let margin = 50.0
        let beamY = height / 2.0
        let scaleX = (width - 2.0 * margin) / s.Length
        let I = calculateI s.Section
        
        let pts = 
            [0.0 .. 0.1 .. s.Length]
            |> List.map (fun x ->
                let d = calculateDeflection s.Length s.Force s.ForcePos s.Material.E I s.BeamType x
                let vx = margin + (x * scaleX)
                let vy = beamY + (d * 1000.0)
                sprintf "%f,%f" vx vy)
            |> String.concat " "

        let forceX = margin + s.ForcePos * scaleX
        
        html$"""
        <svg width="600" height="200" style="background: #f0f0f0; border-radius: 8px;">
            <polyline points="${pts}" fill="none" stroke="#2c3e50" stroke-width="4" />
            <line x1="${forceX}" y1="${beamY - 40.0}" x2="${forceX}" y2="${beamY - 5.0}" stroke="red" stroke-width="3" />
        </svg>
        """

    let update (render: unit -> unit) (fn: State -> State) =
        state <- fn state
        render()

    [<LitElement("mech-sim")>]
    let MechSim() =
        let _, render = Hook.useState(0)
        let forceUpdate() = render(fun n -> n + 1)

        html$"""
        <div style="font-family: sans-serif; padding: 20px; max-width: 800px; margin: auto;">
            <h1>Gerenda Szimulátor</h1>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; background: #ecf0f1; padding: 20px; border-radius: 8px;">
                <div>
                    <label>Hossz: ${state.Length}m</label><br/>
                    <input type="range" min="1" max="20" .value="${string state.Length}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Length = float (e.target :?> HTMLInputElement).value })}" />
                </div>
                <div>
                    <label>Erő: ${state.Force}N</label><br/>
                    <input type="range" min="0" max="10000" .value="${string state.Force}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Force = float (e.target :?> HTMLInputElement).value })}" />
                </div>
                <div>
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = Cantilever })}">Konzolos</button>
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = SimplySupported })}">Kéttámaszú</button>
                </div>
            </div>
            <div style="margin-top: 20px; text-align: center;">
                ${renderBeam state}
            </div>
        </div>
        """

    let register() = ()
