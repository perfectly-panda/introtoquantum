namespace Quantum.QSharpApplication1 {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Arrays;
    
    @EntryPoint()
    operation SampleRandomNumber(nQubits : Int) : Result[] {

        // We prepare a register of qubits in a uniform
        // superposition state, such that when we measure,
        // all bitstrings occur with equal probability.
        use register = Qubit[nQubits];

        // Set qubits in superposition.
        ApplyToEachA(H, register);

        // Measure all qubits and return.
        return ForEach(MResetZ, register);

    }
}

