namespace MechSim

open System
open Fable.Core
open Fable.Core.JsInterop
open Lit
open Browser.Types

[<LitElement("mech-sim")>]
let MechSim() =
    let state, setState = Hook.useState({| Length = 5.0; Force = 1000.0; IsCantilever = false |})

    // Számítások elvégzése
    let L, P = state.Length, state.Force
    let E, I, a = 210.0e9, 0.0001, state.Length / 2.0
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
            sprintf "%f,%f" (50.0 + x * scaleX) (100.0 + d * 500.0))
        |> String.concat " "

    let sL, sF = string state.Length, string state.Force

    html$"""
    <div style="font-family: sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px; max-width: 500px; margin: auto;">
        <h2 style="text-align: center;">Mérnöki Szimulátor</h2>
        
        <svg width="500" height="200" style="background: #fdfdfd; border: 1px solid #eee;">
            <polyline points="${points}" fill="none" stroke="blue" stroke-width="3" />
        </svg>

        <div style="margin-top: 20px;">
            <label>Hossz: ${sL} m</label><br/>
            <input type="range" min="1" max="20" .value="${sL}" 
                @input="${fun (e: Event) -> setState({| state with Length = float (e.target :?> HTMLInputElement).value |})}" />
            <br/>
            <label>Erő: ${sF} N</label><br/>
            <input type="range" min="0" max="5000" .value="${sF}" 
                @input="${fun (e: Event) -> setState({| state with Force = float (e.target :?> HTMLInputElement).value |})}" />
            <br/><br/>
            <button @click="${fun _ -> setState({| state with IsCantilever = true |})}">Konzol</button>
            <button @click="${fun _ -> setState({| state with IsCantilever = false |})}">Kéttámaszú</button>
        </div>
    </div>
    """

let register() = ()
