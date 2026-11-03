# Heart Failure Classifier

A custom three-layer Multi-Layer Perceptron (MLP) implemented from scratch in C# for binary classification of heart disease risk.

> **Disclaimer:** This project is for educational purposes only. It is not a medical diagnostic tool and must not be used for real clinical decisions.

---

## Table of Contents

- [Overview](#overview)
- [Dataset](#dataset)
  - [Feature Encoding](#feature-encoding)
- [Data Preprocessing](#data-preprocessing)
- [Neural Network Architecture](#neural-network-architecture)
  - [Network Components](#network-components)
- [Experiments](#experiments)
- [Evaluation Metrics](#evaluation-metrics)
  - [Formulas](#formulas)
  - [Confusion Matrix](#confusion-matrix)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [How to Run](#how-to-run)
  - [1. Place the Dataset](#1-place-the-dataset)
  - [2. Build and Run](#2-build-and-run)
- [Generated Files](#generated-files)
- [Limitations](#limitations)

---

## Overview

This project predicts whether a patient has heart disease using 11 clinical attributes from the Heart Failure Prediction Dataset.

The application implements the following features:

- CSV parsing without external dependencies
- Categorical feature encoding
- Reproducible dataset shuffling
- 80/20 training and testing split
- Min-Max feature normalization
- Sigmoid activation functions
- Custom backpropagation algorithm
- Hyperparameter tuning across multiple experiments
- Performance evaluation via Accuracy, Precision, Recall, F1-Score, and MSE
- Confusion-matrix reporting
- Training-loss export (`.csv`)
- Best-model weight persistence (`.txt`)

The neural network is implemented entirely in native C# without external machine-learning libraries.

---

## Dataset

The application expects a dataset file named `heart.csv`.

The dataset contains 918 patient records, 11 input features, and one binary target column named `HeartDisease`.

| Property | Value |
|---|---:|
| Total records | 918 |
| Input features | 11 |
| Target column | `HeartDisease` |
| Training records | 734 |
| Testing records | 184 |
| Missing values | 0 |

Target values:
- `1`: Heart disease detected
- `0`: No heart disease detected

### Feature Encoding

| Index | Feature | Encoding |
|---:|---|---|
| 0 | `Age` | Numeric value |
| 1 | `Sex` | `M = 1`, `F = 0` |
| 2 | `ChestPainType` | `TA = 0`, `ATA = 1`, `NAP = 2`, `ASY = 3` |
| 3 | `RestingBP` | Numeric value |
| 4 | `Cholesterol` | Numeric value |
| 5 | `FastingBS` | `0` or `1` |
| 6 | `RestingECG` | `Normal = 0`, `ST = 1`, `LVH = 2` |
| 7 | `MaxHR` | Numeric value |
| 8 | `ExerciseAngina` | `Y = 1`, `N = 0` |
| 9 | `Oldpeak` | Numeric value |
| 10 | `ST_Slope` | `Up = 0`, `Flat = 1`, `Down = 2` |

---

## Data Preprocessing

Preprocessing is handled by the `DataPipeline` class in `Program.cs`.

The pipeline executes the following sequence:

1. Reads `heart.csv`.
2. Skips the header row.
3. Converts categorical strings into numerical equivalents.
4. Shuffles all records deterministically using `Random(42)`.
5. Splits the dataset into:
   - **80%** training data
   - **20%** testing data
6. Calculates feature minimum and maximum values using **only** the training set.
7. Applies Min-Max normalization to both training and testing sets.
8. Clamps normalized values strictly to the range `[0, 1]`.

The normalization formula is:

$$x' = \frac{x - \min(X_{\text{train}})}{\max(X_{\text{train}}) - \min(X_{\text{train}})}$$

Using only training-set statistics for normalization prevents data leakage from the test partition.

---

## Neural Network Architecture

The network consists of a fully connected, feedforward three-layer architecture:

```plaintext
Input Layer   (11 neurons)
      |
      v
Hidden Layer  (Configurable, default 12 neurons)
      |
      v
Output Layer  (1 neuron, Sigmoid activation)
```

The output neuron produces a probability $P \in [0, 1]$. Classification is determined by a standard decision threshold:

- **Predicted Positive (`1`):** $P \ge 0.5$
- **Predicted Negative (`0`):** $P < 0.5$

### Network Components

| Class | Responsibility |
|---|---|
| `INeuron` | Represents an input node |
| `HNeuron` | Represents a hidden-layer neuron |
| `ONeuron` | Represents the output neuron |
| `NeuralNet` | Coordinates forward propagation, backpropagation, and weight serialization |
| `DataPipeline` | Parses, encodes, partitions, and scales data |
| `EvaluationResult` | Computes classification metrics and confusion matrix metrics |

---

## Experiments

`Program.cs` automatically trains and evaluates four distinct architectural and learning configurations:

| Experiment | Hidden Nodes | Output LR | Input LR | Epochs | Purpose |
|---|---:|---:|---:|---:|---|
| **Exp 1: Small Net, Low LR** | 6 | 0.05 | 0.05 | 600 | Evaluates lower capacity with cautious gradient steps |
| **Exp 2: Baseline Default LR** | 12 | 0.20 | 0.15 | 800 | Serves as the primary balanced baseline |
| **Exp 3: Deep Hidden, Med LR** | 16 | 0.15 | 0.10 | 1,000 | Tests wider hidden representation and longer training |
| **Exp 4: Large Net, Fast LR** | 24 | 0.30 | 0.25 | 800 | Tests higher capacity and aggressive convergence |

---

## Evaluation Metrics

All metrics are evaluated against the held-out 20% test partition after training concludes.

### Formulas

- **Accuracy:** Overall proportion of correct predictions.
  $$\text{Accuracy} = \frac{TP + TN}{TP + TN + FP + FN}$$

- **Precision:** Proportion of positive predictions that were true positives.
  $$\text{Precision} = \frac{TP}{TP + FP}$$

- **Recall (Sensitivity):** Proportion of actual positive cases detected.
  $$\text{Recall} = \frac{TP}{TP + FN}$$

- **F1-Score:** Harmonic mean of precision and recall.
  $$F1 = 2 \times \frac{\text{Precision} \times \text{Recall}}{\text{Precision} + \text{Recall}}$$

- **Mean Squared Error (MSE):** Average squared difference between predictions and targets.
  $$\text{MSE} = \frac{1}{N} \sum_{i=1}^{N} (y_i - \hat{y}_i)^2$$

### Confusion Matrix

```plaintext
                    Actual Positive    Actual Negative
Predicted Positive        TP                 FP
Predicted Negative        FN                 TN
```

- **TP (True Positive):** Correctly predicted heart disease.
- **TN (True Negative):** Correctly predicted no heart disease.
- **FP (False Positive):** Predicted heart disease for a healthy patient.
- **FN (False Negative):** Missed detection of heart disease.

> **Clinical Relevance:** In medical classification tasks, **Recall** is prioritized over Precision because a False Negative means a patient with heart disease goes undetected.

---

## Project Structure

```plaintext
HeartFailureClassifier/
├── Data/
│   └── heart.csv                       # Dataset (must be present)
├── Models/
│   ├── INeuron.cs                      # Input neuron definition
│   ├── HNeuron.cs                      # Hidden neuron implementation
│   ├── ONeuron.cs                      # Output neuron implementation
│   └── NeuralNet.cs                    # Network orchestration & backpropagation
├── Pipeline/
│   ├── DataPipeline.cs                 # Parsing, encoding, and normalization
│   └── EvaluationResult.cs             # Performance evaluation & confusion matrix
├── HeartFailureClassifier.csproj       # Project build configuration
└── Program.cs                          # Entry point and experiment runner
```

---

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher
- Visual Studio 2022, Visual Studio Code, or any C# compatible IDE/terminal

---

## How to Run

### 1. Place the Dataset

Ensure `heart.csv` is located in the working directory (or project root next to `Program.cs`).

### 2. Build and Run

#### Using the .NET CLI

1. Open a terminal and navigate to the project directory:
   ```bash
   cd HeartFailureClassifier
   ```
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run --project HeartFailureClassifier.csproj
   ```

#### Using Visual Studio

1. Open `HeartFailureClassifier.slnx` (or `.sln`).
2. Set `HeartFailureClassifier` as the startup project.
3. Confirm that `heart.csv` is present in the execution path.
4. Press `F5` (Debug) or `Ctrl+F5` (Run without debugging).

If `heart.csv` is missing, the application outputs a file-not-found error and terminates cleanly.

---

## Generated Files

Executing the application produces two artifacts in the root directory:

- **`training_loss_log.csv`**: Contains epoch-by-epoch training MSE loss values across all four experimental runs for visualization.
- **`best_heart_model_weights.txt`**: Serialized layer weights and bias terms belonging to the configuration that achieved the highest F1-score on the test split.

The console output concludes with a summary comparison table displaying:
- Final Training MSE
- Test MSE
- Accuracy
- Precision
- Recall
- F1-Score

---

## Limitations

- **Fixed Train-Test Split:** Uses a single deterministic 80/20 split instead of k-fold cross-validation.
- **Ordinal Encoding:** Categorical features with $>2$ classes (e.g., `ChestPainType`, `RestingECG`) are mapped ordinally rather than one-hot encoded, implying artificial ordinal relationships.
- **Non-Clinical:** Lacks clinical calibration and should not be used in medical workflows.
- **Dataset Bias:** Susceptible to class/demographic biases inherent to the original sample dataset.
- **Console Pause:** Waits for an explicit keypress before closing the terminal window.