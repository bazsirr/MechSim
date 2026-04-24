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

    let calculateDeflection (L: float) (P: float) (a: float) (E: float) (bt: BeamType) (x: float) =
        let I = 0.0001
        if x < 0.0 || x > L then 0.0
        else
            match bt with
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
                let d = calculateDeflection state.Length state.Force state.ForcePos state.Material.E state.BeamType x
                let vx = 50.0 + (x * scaleX)
                let vy = 100.0 + (d * 50.0)
                sprintf "%f,%f" vx vy)
            |> String.concat " "

        let fX = 50.0 + (state.ForcePos * scaleX)
        let sL = string state.Length
        let sF = string state.Force

        html$"""
        <div style="font-family: sans-serif; padding: 20px; border: 1px solid #ccc; border-radius: 10px; max-width: 550px; margin: auto;">
            <h2 style="text-align: center;">MechSim Pro</h2>
            
            <div style="text-align: center; margin: 20px 0;">
                <svg width="500" height="200" style="background: #f8f9fa; border: 1px solid #ddd;">
                    <polyline points="${pts}" fill="none" stroke="#007bff" stroke-width="3" />
                    <line x1="${fX}" y1="50" x2="${fX}" y2="95" stroke="red" stroke-width="3" />
                </svg>
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
                <div>
                    <label>Hossz: ${sL}m</label><br/>
                    <input type="range" min="1" max="20" .value="${sL}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Length = float (e.target :?> HTMLInputElement).value })}" />
                    <br/><br/>
                    <label>Erő: ${sF}N</label><br/>
                    <input type="range" min="0" max="5000" .value="${sF}" @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Force = float (e.target :?> HTMLInputElement).value })}" />
                </div>
                <div style="display: flex; flex-direction: column; gap: 10px;">
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = Cantilever })}">Konzol</button>
                    <button @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = SimplySupported })}">Kéttámaszú</button>
                    <div style="margin-top: 10px; font-size: 0.9em;">
                        <b>Anyag:</b> ${state.Material.Name}<br/>
                        <b>E:</b> ${string (state.Material.E / 1e9)} GPa
                    </div>
                </div>
            </div>
        </div>
        """

    let register() = ()
