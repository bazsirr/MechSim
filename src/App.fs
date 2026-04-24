namespace MechSim

open System
open Fable.Core
open Fable.Core.JsInterop
open Lit
open Browser.Types

module Main =

    [<LitElement("mech-sim")>]
    let MechSim() =
        let state, setState = Hook.useState({| L = 5.0; P = 1000.0; IsCant = false |})

        let L = state.L
        let P = state.P
        let E = 210.0e9
        let I = 0.0001
        let a = L / 2.0
        let scaleX = 400.0 / L
        
        let pts = 
            [0.0 .. 0.5 .. L]
            |> List.map (fun x ->
                let d = 
                    if state.IsCant then
                        if x <= a then (P * (x**2.0) * (3.0*a - x)) / (6.0 * E * I)
                        else (P * (a**2.0) * (3.0*x - a)) / (6.0 * E * I)
                    else
                        let b = L - a
                        if x <= a then (P * b * x * (L**2.0 - b**2.0 - x**2.0)) / (6.0 * L * E * I)
                        else (P * a * (L - x) * (L**2.0 - a**2.0 - (L - x)**2.0)) / (6.0 * L * E * I)
                let vx = 50.0 + x * scaleX
                let vy = 100.0 + d * 500.0
                sprintf "%f,%f" vx vy)
            |> String.concat " "

        let txtL = string L
        let txtP = string P

        html$"""
        <div style="font-family: sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px; max-width: 500px; margin: auto; background: white;">
            <h2 style="text-align: center;">Beam Simulator</h2>
            
            <svg width="500" height="200" style="background: #fafafa; border: 1px solid #eee; display: block; margin: auto;">
                <polyline points="${pts}" fill="none" stroke="blue" stroke-width="3" />
            </svg>

            <div style="margin-top: 20px; display: grid; gap: 10px;">
                <div>
                    <label>Length: ${txtL} m</label><br/>
                    <input type="range" min="1" max="20" .value="${txtL}" 
                        @input="${fun (e: Event) -> 
                            let v = float (e.target :?> HTMLInputElement).value
                            setState({| state with L = v |})}" style="width: 100%%;" />
                </div>
                <div>
                    <label>Force: ${txtP} N</label><br/>
                    <input type="range" min="0" max="5000" .value="${txtP}" 
                        @input="${fun (e: Event) -> 
                            let v = float (e.target :?> HTMLInputElement).value
                            setState({| state with P = v |})}" style="width: 100%%;" />
                </div>
                <div style="display: flex; gap: 10px; margin-top: 10px;">
                    <button style="flex: 1; padding: 10px;" @click="${fun _ -> setState({| state with IsCant = true |})}">Cantilever</button>
                    <button style="flex: 1; padding: 10px;" @click="${fun _ -> setState({| state with IsCant = false |})}">Supported</button>
                </div>
            </div>
        </div>
        """

    let register() = ()
