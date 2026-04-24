
import { LitElementAttribute } from "../fable_modules/Fable.Lit.1.4.2/LitElement.fs.js";
import { Hook_getContext_343DAFF1 } from "../fable_modules/Fable.Lit.1.4.2/Hook.fs.js";
import { fmt, printf, toText, join } from "../fable_modules/fable-library-js.5.0.0/String.js";
import { map } from "../fable_modules/fable-library-js.5.0.0/List.js";
import { toList } from "../fable_modules/fable-library-js.5.0.0/Seq.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.0.0/Range.js";
import { LitHelpers_html } from "../fable_modules/Fable.Lit.1.4.2/Lit.fs.js";
import { parse } from "../fable_modules/fable-library-js.5.0.0/Double.js";

export const MechSim = (new LitElementAttribute("mech-sim")).Decorate(function () {
    let patternInput;
    const v = {
        IsCant: false,
        L: 5,
        P: 1000,
    };
    patternInput = Hook_getContext_343DAFF1(this).useState(() => v);
    const state = patternInput[0];
    const setState = patternInput[1];
    const L_1 = state.L;
    const P_1 = state.P;
    const a = L_1 / 2;
    const scaleX = 400 / L_1;
    const pts = join(" ", map((x) => {
        let b, arg0__40;
        const vx = 50 + (x * scaleX);
        const vy = 100 + ((state.IsCant ? ((x <= a) ? (((P_1 * Math.pow(x, 2)) * ((3 * a) - x)) / ((6 * 210000000000) * 0.0001)) : (((P_1 * Math.pow(a, 2)) * ((3 * x) - a)) / ((6 * 210000000000) * 0.0001))) : ((b = (L_1 - a), (x <= a) ? ((((P_1 * b) * x) * ((Math.pow(L_1, 2) - Math.pow(b, 2)) - Math.pow(x, 2))) / (((6 * L_1) * 210000000000) * 0.0001)) : ((((P_1 * a) * (L_1 - x)) * ((Math.pow(L_1, 2) - Math.pow(a, 2)) - ((arg0__40 = (L_1 - x), Math.pow(arg0__40, 2))))) / (((6 * L_1) * 210000000000) * 0.0001))))) * 500);
        return toText(printf("%f,%f"))(vx)(vy);
    }, toList(rangeDouble(0, 0.5, L_1))));
    const txtL = L_1.toString();
    const txtP = P_1.toString();
    return LitHelpers_html(fmt`
        <div style="font-family: sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px; max-width: 500px; margin: auto; background: white;">
            <h2 style="text-align: center;">Beam Simulator</h2>
            
            <svg width="500" height="200" style="background: #fafafa; border: 1px solid #eee; display: block; margin: auto;">
                <polyline points="$${pts}" fill="none" stroke="blue" stroke-width="3" />
            </svg>

            <div style="margin-top: 20px; display: grid; gap: 10px;">
                <div>
                    <label>Length: $${txtL} m</label><br/>
                    <input type="range" min="1" max="20" .value="$${txtL}" 
                        @input="$${(e) => {
        setState({
            IsCant: state.IsCant,
            L: parse(e.target.value),
            P: state.P,
        });
    }}" style="width: 100%;" />
                </div>
                <div>
                    <label>Force: $${txtP} N</label><br/>
                    <input type="range" min="0" max="5000" .value="$${txtP}" 
                        @input="$${(e_1) => {
        setState({
            IsCant: state.IsCant,
            L: state.L,
            P: parse(e_1.target.value),
        });
    }}" style="width: 100%;" />
                </div>
                <div style="display: flex; gap: 10px; margin-top: 10px;">
                    <button style="flex: 1; padding: 10px;" @click="$${(_arg) => {
        setState({
            IsCant: true,
            L: state.L,
            P: state.P,
        });
    }}">Cantilever</button>
                    <button style="flex: 1; padding: 10px;" @click="$${(_arg_1) => {
        setState({
            IsCant: false,
            L: state.L,
            P: state.P,
        });
    }}">Supported</button>
                </div>
            </div>
        </div>
        `);
});

export function register() {
}

