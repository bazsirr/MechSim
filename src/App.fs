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
                if x <= a then
                    (P * (x**2.0) * (3.0*a - x)) / (6.0 * E * I)
                else
                    (P * (a**2.0) * (3.0*x - a)) / (6.0 * E * I)
            | SimplySupported ->
                let b = L - a
                if x <= a then
                    (P * b * x * (L**2.0 - b**2.0 - x**2.0)) / (6.0 * L * E * I)
                else
                    (P * a * (L - x) * (L**2.0 - a**2.0 - (L - x)**2.0)) / (6.0 * L * E * I)

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

    let renderBeam forceUpdate state =
        let width = 600.0
        let height = 200.0
        let margin = 50.0
        let beamY = height / 2.0
        let scaleX = (width - 2.0 * margin) / state.Length
        
        let I = calculateI state.Section
        let points = 
            [0.0 .. 0.05 .. state.Length]
            |> List.map (fun x ->
                let defl = calculateDeflection state.Length state.Force state.ForcePos state.Material.E I state.BeamType x
                let visualY = beamY + (defl * 1000.0)
                let visualX = margin + (x * scaleX)
                sprintf "%f,%f" visualX visualY)
            |> String.concat " "

        let supports = 
            if state.BeamType = SimplySupported then 
                let x1 = margin
                let x2 = margin + state.Length * scaleX
                html$"""
                <path d=${"M " + string x1 + " " + string beamY + " l -10 20 h 20 z"} fill="#7f8c8d" />
                <path d=${"M " + string x2 + " " + string beamY + " l -10 20 h 20 z"} fill="#7f8c8d" />
                """
            else 
                html$"""<rect x="${margin - 10.0}" y="${beamY - 30.0}" width="10" height="60" fill="#7f8c8d" />"""

        html$"""
        <svg width="600" height="200" style="background: #f0f0f0; border-radius: 8px;">
            <polyline points="${points}" fill="none" stroke="#2c3e50" stroke-width="4" />
            ${supports}
            <line x1="${margin + state.ForcePos * scaleX}" y1="${beamY - 40.0}" 
                  x2="${margin + state.ForcePos * scaleX}" y2="${beamY - 5.0}" 
                  stroke="red" stroke-width="3" />
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
            <h1 style="color: #2c3e50;">Gerenda Szimulátor</h1>
            
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 20px;">
                <div style="background: #ecf0f1; padding: 15px; border-radius: 8px;">
                    <h3>Paraméterek</h3>
                    <label>Hossz (m): ${state.Length}</label><br/>
                    <input type="range" min="1" max="20" step="0.5" .value="${state.Length}" 
                        @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Length = float (e.target :?> HTMLInputElement).value })}" /><br/>
                    
                    <label>Erő (N): ${state.Force}</label><br/>
                    <input type="range" min="0" max="10000" step="100" .value="${state.Force}"
                        @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Force = float (e.target :?> HTMLInputElement).value })}" /><br/>

                    <label>Erő helye (m): ${state.ForcePos}</label><br/>
                    <input type="range" min="0" max="${state.Length}" step="0.1" .value="${state.ForcePos}"
                        @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with ForcePos = float (e.target :?> HTMLInputElement).value })}" />
                </div>

                <div style="background: #ecf0f1; padding: 15px; border-radius: 8px;">
                    <h3>Anyag és Konfiguráció</h3>
                    <select @change="${fun (e: Event) -> 
                        let idx = int (e.target :?> HTMLSelectElement).value
                        update forceUpdate (fun s -> { s with Material = Materials.All.[idx] })}">
                        ${Materials.All |> List.mapi (fun i m -> html$"""<option value="${i}">${m.Name}</option>""")}
                    </select><br/><br/>

                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = Cantilever })}">Konzolos</button>
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = SimplySupported })}">Kéttámaszú</button>
                </div>
            </div>

            <div style="text-align: center;">
                ${renderBeam forceUpdate state}
            </div>

            <div style="margin-top: 20px;">
                <p>Anyag: ${state.Material.Name} (${state.Material.E / 1e9} GPa)</p>
            </div>
        </div>
        """

    let register() = ()
