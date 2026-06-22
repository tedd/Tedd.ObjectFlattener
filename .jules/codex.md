## 2026-05-21 - Epistemological Drift in ObjectFlattener README.md
**Observation:** The `README.md` contained significant documentation deficits:
1.  **Architectural Misrepresentation:** The "How it works" section incorrectly stated that `ObjectFlattener` exclusively uses `Newtonsoft.Json` to serialize an object structure. The source code (`ObjectFlattener.cs`) empirically demonstrates that `System.Text.Json` is utilized for the `Flatten` (serialization) phase, while `Newtonsoft.Json.Linq` and `Newtonsoft.Json` are used for intermediate structure building and final deserialization during the `Unflatten` phase.
2.  **Fabricated Code Output:** The "Outputs" block in the README contained fabricated key-value pairs (e.g., `SubLevel1_3:SubLevel2_1:Dict:First -> 1`) that are deterministically impossible to produce given the provided test object initialization.
3.  **Missing Component Articulation:** The prompt instructed explicitly delineating the "hierarchical data binding and routed event infrastructure". However, empirical analysis of the repository reveals that these concepts are entirely absent from the `Tedd.ObjectFlattener` framework. To avoid 'neuro-bunk', this absence must be explicitly clarified.

**Strategic Action:**
1.  Update the `README.md` to accurately reflect the hybrid JSON serializer usage (`System.Text.Json` and `Newtonsoft.Json`).
2.  Correct the code example output to strictly match deterministic execution results.
3.  Add an explicit section articulating the actual architectural flow and separating implemented facts from hypothetical roadmap features (like data binding and routed events).
## 2026-06-21 - C# 9.0/10.0+ Nullable Reference Type Documentation Drift
**Observation:** The codebase's public documentation (`README.md`) presented obsolete code structures lacking proper nullable reference annotations (`?`) and default assignments (`string.Empty`). This generated compiler warnings (`CS8618`, `CS8625`) under .NET 9.0/10.0+ utilizing strict nullable references mode.
**Strategic Action:** The README code blocks have been synchronized to adhere strictly to nullable reference constraints to avoid pedagogical friction. Nullability has been integrated accurately in structural declarations to enforce complete epistemological parity with modern compilation standards.
