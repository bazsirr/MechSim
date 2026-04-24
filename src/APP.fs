namespace MechSim

open System
open Fable.Core
open Fable.Core.JsInterop
open Lit
open Browser.Types

// --- DOMAIN: Anyagok és Formák ---
type Material = { Name: string; E: float } // E = Young-modulus (Pa)

module Materials =
    let Steel = { Name = "Acél (S235)"; E = 210.0e9 }
    let Aluminum = { Name = "Alumínium"; E = 70.0e9 }
    let Copper = { Name = "Vörösréz"; E = 120.0e9 }
    let Titanium = { Name = "Titán"; E = 110.0e9 }

type Section = 
    | Rectangular of width: float * height: float
    | Circular of radius: float
    | Pipe of outerRadius: float * thickness: float

type BeamType = Cantilever | SimplySupported

type Load = { Magnitude: float; Position: float }

type Model = {
    Length: float
    Material: Material
    Section: Section
    Type: BeamType
    Load: Load
}

// --- PHYSICS: Mérnöki számítások ---
module Physics =
    let calculateInertia section =
        match section with
        | Rectangular(b, h) -> (b * (h ** 3.0)) / 12.0
        | Circular(r) -> (Math.PI * (r ** 4.0)) / 4.0
        | Pipe(ro, t) -> 
            let ri = ro - t
            (Math.PI * (ro ** 4.0 - ri ** 4.0)) / 4.0

    let getDeflection x model =
        let E = model.Material.E
        let I = calculateInertia model.Section
        let L = model.Length
        let P = model.Load.Magnitude
        let a = model.Load.Position

        match model.Type with
        | Cantilever ->
            if x <= a then (P * x**2.0 * (3.0*a - x)) / (6.0 * E * I)
            else (P * a**2.0 * (3.0*x - a)) / (6.0 * E * I)
        | SimplySupported ->
            let b = L - a
            if x <= a then (P * b * x * (L**2.0 - b**2.0 - x**2.0)) / (6.0 * L * E * I)
            else (P * a * (L - x) * (L**2.0 - a**2.0 - (L-x)**2.0)) / (6.0 * L * E * I)

// --- UI: Megjelenítés ---
[<LitElement("beam-app")>]
let BeamApp() =
    let _ = LitElement.init(fun (config: LitElementConfig) ->
        let model, setModel = config.useState({
            Length = 2.0; Material = Materials.Steel; Section = Rectangular(0.05, 0.1)
            Type = SimplySupported; Load = { Magnitude = 1000.0; Position = 1.0 }
        })

        config.render(html $"""
            <div style="background: #f4f4f4; padding: 2rem; font-family: 'Segoe UI', sans-serif;">
                <h1 style="color: #2c3e50;">Műszaki Gerenda Szimulátor (F#)</h1>
                <div style="display: flex; gap: 20px;">
                    <div style="flex: 1; background: white; padding: 15px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);">
                        <h3>Paraméterek</h3>
                        <label>Hossz (L = {model.Length} m):</label><br/>
                        <input type="range" min="0.5" max="10" step="0.5" .value={string model.Length} 
                            @input={fun (e: Event) -> setModel({ model with Length = float e.target?value })}>
                        <br/><br/>
                        <label>Anyag:</label>
                        <select @change={fun (e: Event) -> 
                            let mat = match e.target?value with "Al" -> Materials.Aluminum | "Cu" -> Materials.Copper | _ -> Materials.Steel
                            setModel({ model with Material = mat })}>
                            <option value="St">Acél</option>
                            <option value="Al">Alumínium</option>
                            <option value="Cu">Réz</option>
                        </select>
                    </div>
                    <div style="flex: 2; background: #2c3e50; color: white; padding: 15px; border-radius: 8px;">
                        <h3>Eredmények</h3>
                        <p>Max lehajlás: <strong>{(Physics.getDeflection (model.Length/2.0) model * 1000.0).ToString("F2")} mm</strong></p>
                        <svg width="100%%" height="150" viewBox="0 0 500 150">
                            <line x1="50" y1="75" x2="450" y2="75" stroke="gray" stroke-dasharray="5" />
                            <rect x="50" y="70" width="400" height="10" fill="#3498db" />
                        </svg>
                    </div>
                </div>
            </div>
        """))
    ()
