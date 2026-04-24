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

    let renderBeam state =
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

        html$"""
        <svg width="{width}" height="{height}" style="background: #f0f0f0; border-radius: 8px;">
            <polyline points="{points}" fill="none" stroke="#2c3e50" stroke-width="4" />
            {if state.BeamType = SimplySupported then 
                html$"""
                <path d="M {margin} {beamY} l -10 20 h 20 z" fill="#7f8c8d" />
                <path d="M {margin + state.Length * scaleX} {beamY} l -10 20 h 20 z" fill="#7f8c8d" />
                """
             else 
                html$"""<rect x="{margin - 10.0}" y="{beamY - 30.0}" width="10" height="60" fill="#7f8c8d" />"""
            }
            <line x1="{margin + state.ForcePos * scaleX}" y1="{beamY - 40}" 
                  x2="{margin + state.ForcePos * scaleX}" y2="{beamY - 5}" 
                  stroke="red" stroke-width="3" />
        </svg>
        """

    let update (render: unit -> unit) (fn: State -> State) =
        state <- fn state
        render()

    [<LitElement("mech-sim")>]
    let MechSim() =
        let _,
