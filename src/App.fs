namespace MechSim

open System
open Fable.Core
open Fable.Core.JsInterop
open Lit
open Browser.Types

module Main =

    [<LitElement("mech-sim")>]
    let MechSim() =
        let state, setState = Hook.useState({| Length = 5.0; Force = 1000.0; IsCantilever = false |})

        let L = state.Length
        let P = state.Force
        let E = 210.0e9
        let I = 0.0001
        let a = L / 2.0
        let scaleX = 400.0 / L
        
        let points = 
            [0.0 .. 0.5 .. L]
            |> List.map (fun x ->
                let d = 
                    if state.IsCantilever then
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

        let sL = string state.Length
        let sF = string state.Force

        html$"""
        <div style="font-family: sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px; max-width: 500px; margin: auto; background: white;">
            <h2 style="text-align: center;">Beam Simulator</h2>
            
            <svg width="500" height="200" style="background: #fafafa; border: 1px solid #eee; display: block; margin: auto;">
                <polyline points="${points}" fill="none" stroke="blue" stroke-width="3" />
            </svg>

            <div style="margin-top: 20px; display: flex; flex-direction: column; gap: 15px;">
                <div>
                    <label>Length: ${sL} m</label>
                    <input type="range" min="1" max="20" .value="${sL}" 
                        @input="${fun (e: Event) -> 
                            let v = float (e.target :?> HTMLInputElement).value
                            setState({| state with Length = v |})}" style="width: 100%;" />
                </div>
                <div>
                    <label>Force: ${sF} N</label>
                    <input type="range" min="0" max="5000" .value="${sF}" 
                        @input="${fun (e: Event) -> 
                            let v = float (e.target :?> HTMLInputElement).value
                            setState({| state with Force = v |})}" style="width: 100%;" />
                </div>
                <div style="display: flex; gap: 10px;">
                    <button style="flex: 1; padding: 10px;" @click="${fun _ -> setState({| state with IsCantilever = true |})}">Cantilever</button>
                    <button style="flex: 1; padding: 10px;" @click="${fun _ -> setState({| state with IsCantilever = false |})}">Supported</button>
                </div>
            </div>
        </div>
        """

    let register() = ()
