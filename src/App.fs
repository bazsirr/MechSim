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
        let All = [ Steel; Aluminum ]

    type BeamType = Cantilever | SimplySupported

    let calculate (L: float) (P: float) (a: float) (E: float) (bt: BeamType) (x: float) =
        let I = 0.0001
        match bt with
        | Cantilever ->
            if x <= a then 
                P * x * x * (3.0 * a - x) / (6.0 * E * I)
            else 
                P * a * a * (3.0 * x - a) / (6.0 * E * I)
        | SimplySupported ->
            let b = L - a
            if x <= a then 
                P * b * x * (L*L - b*b - x*x) / (6.0 * L * E * I)
            else 
                P * a * (L - x) * (L*L - a*a - (L - x)*(L - x)) / (6.0 * L * E * I)

    type State = {
        Length: float
        Force: float
        ForcePos: float
        BeamType: BeamType
    }

    let mutable state = {
        Length = 5.0
        Force = 1000.0
        ForcePos = 2.5
        BeamType = SimplySupported
    }

    let update (render: unit -> unit) (fn: State -> State) =
        state <- fn state
        render()

    [<LitElement("mech-sim")>]
    let MechSim() =
        let _, render = Hook.useState(0)
        let forceUpdate () = render(fun n -> n + 1)

        let handleLength (e: Event) =
            let value = (e.target :?> HTMLInputElement).value |> float
            update forceUpdate (fun s -> { s with Length = value })

        let handleForce (e: Event) =
            let value = (e.target :?> HTMLInputElement).value |> float
            update forceUpdate (fun s -> { s with Force = value })

        let setCantilever (_: Event) =
            update forceUpdate (fun s -> { s with BeamType = Cantilever })

        let setSimplySupported (_: Event) =
            update forceUpdate (fun s -> { s with BeamType = SimplySupported })

        let sL = string state.Length
        let sF = string state.Force
        let scaleX = 400.0 / state.Length
        let fX = 50.0 + state.ForcePos * scaleX
        
        let pts = 
            [0.0 .. 0.5 .. state.Length]
            |> List.map (fun x ->
                let d = calculate state.Length state.Force state.ForcePos 210.0e9 state.BeamType x
                sprintf "%.1f,%.1f" (50.0 + x * scaleX) (100.0 + d * 100.0))
            |> String.concat " "

        html $"""
        <div style="font-family: sans-serif; padding: 20px; max-width: 500px; margin: auto;">
            <h3>Gerenda Szimuláció</h3>
            
            <svg width="500" height="200" style="background: #eee; border-radius: 8px;">
                <polyline points="${pts}" fill="none" stroke="blue" stroke-width="3" />
                <line x1="${fX}" y1="50" x2="${fX}" y2="95" stroke="red" stroke-width="3" />
            </svg>

            <div style="margin-top: 20px;">
                <label>Hossz: ${sL} m</label><br/>
                <input type="range" min="1" max="20" step="0.1" .value="${sL}" 
                       @input="${handleLength}" />
                <br/>
                <label>Erő: ${sF} N</label><br/>
                <input type="range" min="0" max="5000" step="10" .value="${sF}" 
                       @input="${handleForce}" />
                <br/><br/>
                <button @click="${setCantilever}">Konzol</button>
                <button @click="${setSimplySupported}">Kéttámaszú</button>
            </div>
        </div>
        """

    let register() = () 
