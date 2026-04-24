namespace MechSim

open System
open Fable.Core
open Fable.Core.JsInterop
open Lit
open Browser.Types
open Browser.Dom

module App =

    type BeamType = Cantilever | SimplySupported

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

    // A számításokat teljesen különválasztjuk
    let getDeflection (s: State) (x: float) =
        let E = 210.0e9
        let I = 0.0001
        let L = s.Length
        let P = s.Force
        let a = s.ForcePos
        match s.BeamType with
        | Cantilever ->
            if x <= a then (P * (x**2.0) * (3.0*a - x)) / (6.0 * E * I)
            else (P * (a**2.0) * (3.0*x - a)) / (6.0 * E * I)
        | SimplySupported ->
            let b = L - a
            if x <= a then (P * b * x * (L**2.0 - b**2.0 - x**2.0)) / (6.0 * L * E * I)
            else (P * a * (L - x) * (L**2.0 - a**2.0 - (L - x)**2.0)) / (6.0 * L * E * I)

    let update (render: unit -> unit) (fn: State -> State) =
        state <- fn state
        render()

    [<LitElement("mech-sim")>]
    let MechSim() =
        let _, render = Hook.useState(0)
        let forceUpdate() = render(fun n -> n + 1)

        // 1. Minden adatot ELŐRE stringgé alakítunk
        let sLength = string state.Length
        let sForce = string state.Force
        
        // 2. SVG pontok kiszámítása előre
        let scaleX = 400.0 / state.Length
        let forceX = 50.0 + (state.ForcePos * scaleX)
        let sForceX = string forceX
        
        let pointList = 
            [0.0 .. 0.5 .. state.Length]
            |> List.map (fun x ->
                let d = getDeflection state x
                let vx = 50.0 + (x * scaleX)
                let vy = 100.0 + (d * 100.0)
                sprintf "%f,%f" vx vy)
        let pointsString = String.concat " " pointList

        // 3. A HTML rész már csak egyszerű változókat használ
        html$"""
        <div style="font-family: sans-serif; padding: 20px; max-width: 500px; margin: auto; border: 1px solid #ddd; border-radius: 8px;">
            <h2 style="text-align: center;">Műszaki Szimuláció</h2>
            
            <svg width="500" height="200" style="background: #fafafa; border: 1px solid #eee;">
                <polyline points="${pointsString}" fill="none" stroke="#007bff" stroke-width="3" />
                <line x1="${sForceX}" y1="40" x2="${sForceX}" y2="95" stroke="red" stroke-width="3" />
            </svg>

            <div style="margin-top: 20px;">
                <div>
                    <label>Gerenda hossza: ${sLength} m</label><br/>
                    <input type="range" min="1" max="20" .value="${sLength}" 
                        @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Length = float (e.target :?> HTMLInputElement).value })}" style="width: 100%;" />
                </div>
                <div style="margin-top: 10px;">
                    <label>Terhelő erő: ${sForce} N</label><br/>
                    <input type="range" min="0" max="5000" .value="${sForce}" 
                        @input="${fun (e: Event) -> update forceUpdate (fun s -> { s with Force = float (e.target :?> HTMLInputElement).value })}" style="width: 100%;" />
                </div>
                <div style="margin-top: 15px; display: flex; gap: 10px;">
                    <button style="flex: 1; padding: 8px;" @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = Cantilever })}">Konzol</button>
                    <button style="flex: 1; padding: 8px;" @click="${fun _ -> update forceUpdate (fun s -> { s with BeamType = SimplySupported })}">Kéttámaszú</button>
                </div>
            </div>
        </div>
        """

    let register() = ()
