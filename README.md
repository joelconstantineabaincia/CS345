Mamdani Fuzzy Inference System (.NET)

Description

This project demonstrates a simple fuzzy logic controller comparing Mamdani (centroid) and Sugeno (zero-order singleton) defuzzification methods. It reads two crisp inputs (temperature and humidity), computes membership degrees, evaluates three rules, and prints Mamdani and Sugeno fan-speed outputs.

Features

- Shared-input fuzzification using triangular membership functions
- Rule evaluation with MIN / MAX operators
- Mamdani defuzzification using centroid integration
- Sugeno defuzzification using weighted average of singletons

Prerequisites

- Visual Studio (any recent version) or MSBuild
- .NET Framework / .NET SDK compatible with the project (open the solution in Visual Studio to see the targeted framework)

Build & Run

1. Open the solution file (.sln) in Visual Studio and build the solution.
2. Set the startup project to "Mamdani Fuzzy Inference System" and run.
3. The program runs as a console application and prints inputs, fuzzification results, rule strengths, and final Mamdani vs Sugeno fan speeds.

Notes / Troubleshooting

- If you run the app from an environment that does not provide a console (for example, some IDE run configurations or when Output Type is not a Console Application), Console.ReadKey() at program exit may throw:
  System.InvalidOperationException: "Cannot read keys when either application does not have a console or when console input has been redirected from a file. Try Console.Read."

  Recommended fixes:
  - Replace Console.ReadKey() with Console.Read() to allow reading from redirected input.
  - Or change the project Output Type to "Console Application" in project properties.
  - Or remove the pause when running in environments without a console.

Project structure

- Program.cs  -- main program and demo flow
- (other source files) -- membership functions, defuzzification helpers

License

This repository contains example code for demonstration and learning. No license specified.
