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

    type BeamType = Cantilever | SimplySupported

    let calculateDeflection (L: float) (P: float) (a: float) (E: float) (I: float) (bt: BeamType) (x: float) =
        if x < 0.0 || x > L then 0.0
        else
            match bt with
            | Cantilever ->
                if x <= a then (P * (x**2.0) * (3.0*a - x)) / (6.0 * E * 0.0001)
                else (P * (a**2.0) * (3.0*x - a)) / (6.0 * E * 0.0001)
            | SimplySupported ->
                let b = L - a
                if x <= a then (P * b * x * (L**2.0 - b**2.0 - x**2.0)) / (6.0 * L * E * 0.0001)
                else (P * a * (L - x) * (L**2.0 - a**2.0 - (L - x)**2.0)) / (6.0 * L * E * 0.0001)

    type State = {
        Length: float
        Force: float
        ForcePos: float
        Material: Material
        BeamType: BeamType
    }

    let mutable state = {
        Length = 5.0
        Force = 1000.0
        ForcePos = 2.5
        Material = Materials.Steel
        BeamType = SimplySupported
    }

    let update (render: unit -> unit) (fn: State -> State) =
        state <- fn state
        render()

    [<LitElement("mech-sim")>]
    let MechSim() =
        let _, render = Hook.useState(0)
        let forceUpdate() = render(fun n -> n + 1)

        let scaleX = 400.0 / state.Length
        let pts = 
            [0.0 .. 0.2 .. state.Length]
            |> List.map (fun x ->
                let d = calculateDeflection state.Length state.Force state.ForcePos state.Material.E 0.0001 state.BeamType x
                let vx = 50.0 + (x * scaleX)
                let vy = 100.0 + (d * 50.0)
                sprintf "%f,%f" vx vy)
            |> String.concat " "

        let fX = 50.0 + (state.ForcePos * scaleX)
        let sL = string state.Length
        let sF = string state.Force
        let sFP = string state.ForcePos

        html$"""
        <div style="font-family: sans-serif; padding: 20px; border: 1px solid #ccc; border-radius: 10px;">
            <h2 style="text-align: center;">Mérnöki Szimulátor</h2>
            
            <div style="text-align: center; margin: 20px 0;">
                <svg width="500" height="200" style="background: #eee;">
                    <polyline points="${pts}" fill="none" stroke="blue" stroke-width="3" />
                    <line x1="${fX}" y1="50" x2="${fX}" y2="95" stroke="red" stroke-width="3" />
                </svg>
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
                <div>
                    <label>Hossz (m): ${sL}</label><br/>
                    <input type="range" min="1" max="20" .value="${sL}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Length = float (e.target :?> HTMLInputElement).value })}" />
                    <br/>
                    <label>Erő (N): ${sF}</label><br/>
                    <input type="range" min="0" max="5000" .value="${sF}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Force = float (e.target :?> HTMLInputElement).value })}" />
                </div>
                <div>
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = Cantilever })}">Konzol</button>
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = SimplySupported })}">Kéttámaszú</button>
                    <p>Anyag: ${state.Material.Name}</p>
                </div>
            </div>
        </div>
        """

    let register() = ()
