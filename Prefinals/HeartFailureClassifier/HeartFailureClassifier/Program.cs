using BackPropagation;
using HeartFailureClassifier;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BackPropagation
{
    // Represents a single patient row from heart.csv
    public class PatientSample
    {
        public double[] Inputs { get; set; } = new double[11];
        public double Target { get; set; }
    }

    // Criteria 2: Data Preprocessing & Pipeline (CSV Parsing, Encoding, Splitting, Min-Max Scaling)
    public static class DataPipeline
    {
        public static (List<PatientSample> TrainSet, List<PatientSample> TestSet) LoadAndPreprocess(string csvPath, double trainRatio = 0.8)
        {
            var dataset = new List<PatientSample>();
            string[] lines = File.ReadAllLines(csvPath);

            // Parse rows (skip header at index 0)
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] cols = lines[i].Split(',');

                var sample = new PatientSample();
                sample.Inputs[0] = double.Parse(cols[0], CultureInfo.InvariantCulture); // Age
                sample.Inputs[1] = cols[1].Trim() == "M" ? 1.0 : 0.0;                   // Sex (M=1, F=0)
                sample.Inputs[2] = cols[2].Trim() switch                                // ChestPainType
                {
                    "TA" => 0.0,
                    "ATA" => 1.0,
                    "NAP" => 2.0,
                    "ASY" => 3.0,
                    _ => 0.0
                };
                sample.Inputs[3] = double.Parse(cols[3], CultureInfo.InvariantCulture); // RestingBP
                sample.Inputs[4] = double.Parse(cols[4], CultureInfo.InvariantCulture); // Cholesterol
                sample.Inputs[5] = double.Parse(cols[5], CultureInfo.InvariantCulture); // FastingBS
                sample.Inputs[6] = cols[6].Trim() switch                                // RestingECG
                {
                    "Normal" => 0.0,
                    "ST" => 1.0,
                    "LVH" => 2.0,
                    _ => 0.0
                };
                sample.Inputs[7] = double.Parse(cols[7], CultureInfo.InvariantCulture); // MaxHR
                sample.Inputs[8] = cols[8].Trim() == "Y" ? 1.0 : 0.0;                   // ExerciseAngina (Y=1, N=0)
                sample.Inputs[9] = double.Parse(cols[9], CultureInfo.InvariantCulture); // Oldpeak
                sample.Inputs[10] = cols[10].Trim() switch                              // ST_Slope
                {
                    "Up" => 0.0,
                    "Flat" => 1.0,
                    "Down" => 2.0,
                    _ => 0.0
                };

                sample.Target = double.Parse(cols[11], CultureInfo.InvariantCulture);   // HeartDisease (0 or 1)
                dataset.Add(sample);
            }

            // Shuffle with a fixed seed so results are reproducible
            Random rng = new Random(42);
            dataset = dataset.OrderBy(_ => rng.Next()).ToList();

            // 80/20 Train-Test Split
            int trainCount = (int)(dataset.Count * trainRatio);
            var trainSet = dataset.Take(trainCount).ToList();
            var testSet = dataset.Skip(trainCount).ToList();

            // Min-Max Normalization to [0, 1] using Training Set bounds
            Normalize(trainSet, testSet, featureCount: 11);

            return (trainSet, testSet);
        }

        private static void Normalize(List<PatientSample> trainSet, List<PatientSample> testSet, int featureCount)
        {
            double[] min = new double[featureCount];
            double[] max = new double[featureCount];

            for (int f = 0; f < featureCount; f++)
            {
                min[f] = trainSet.Min(s => s.Inputs[f]);
                max[f] = trainSet.Max(s => s.Inputs[f]);
            }

            void Scale(List<PatientSample> data)
            {
                foreach (var sample in data)
                {
                    for (int f = 0; f < featureCount; f++)
                    {
                        double range = max[f] - min[f];
                        sample.Inputs[f] = range == 0 ? 0.0 : (sample.Inputs[f] - min[f]) / range;
                        // Clamp test set outliers to [0, 1]
                        sample.Inputs[f] = Math.Clamp(sample.Inputs[f], 0.0, 1.0);
                    }
                }
            }

            Scale(trainSet);
            Scale(testSet);
        }
    }

    // Criteria 4: Evaluation Metrics (Accuracy, Precision, Recall, F1, Confusion Matrix)
    public class EvaluationResult
    {
        public string ConfigName { get; set; } = "";
        public double FinalTrainMSE { get; set; }
        public double TestMSE { get; set; }
        public int TP { get; set; }
        public int TN { get; set; }
        public int FP { get; set; }
        public int FN { get; set; }

        public double Accuracy => (double)(TP + TN) / (TP + TN + FP + FN);
        public double Precision => (TP + FP) == 0 ? 0.0 : (double)TP / (TP + FP);
        public double Recall => (TP + FN) == 0 ? 0.0 : (double)TP / (TP + FN);
        public double F1Score => (Precision + Recall) == 0 ? 0.0 : 2.0 * (Precision * Recall) / (Precision + Recall);

        public void PrintDetailedReport()
        {
            Console.WriteLine($"\n--- Evaluation Report: {ConfigName} ---");
            Console.WriteLine($"Final Train MSE  : {FinalTrainMSE:F6}");
            Console.WriteLine($"Unseen Test MSE  : {TestMSE:F6}");
            Console.WriteLine($"Test Accuracy    : {Accuracy * 100:F2}%  ({TP + TN}/{TP + TN + FP + FN} correct)");
            Console.WriteLine($"Precision        : {Precision * 100:F2}%");
            Console.WriteLine($"Recall (Sens.)   : {Recall * 100:F2}%");
            Console.WriteLine($"F1-Score         : {F1Score * 100:F2}%");
            Console.WriteLine($"Confusion Matrix : [ TP: {TP,3} | FP: {FP,3} ]");
            Console.WriteLine($"                   [ FN: {FN,3} | TN: {TN,3} ]");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // Look for heart.csv in current directory or project root
            string csvPath = File.Exists("heart.csv") ? "heart.csv" : Path.Combine("..", "..", "..", "heart.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine("ERROR: Could not find 'heart.csv'.");
                Console.WriteLine("Please place 'heart.csv' in your project folder or bin/Debug/netX.0 folder.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("==================================================================");
            Console.WriteLine("  HEART FAILURE PREDICTION - 3-LAYER MLP (C# BACKPROPAGATION)");
            Console.WriteLine("==================================================================");

            var (trainSet, testSet) = DataPipeline.LoadAndPreprocess(csvPath, trainRatio: 0.8);
            Console.WriteLine($"Dataset Loaded: {trainSet.Count + testSet.Count} total patients.");
            Console.WriteLine($"Split: {trainSet.Count} Training samples (80%) | {testSet.Count} Testing samples (20%).");
            Console.WriteLine("Features (11): Age, Sex, ChestPain, RestingBP, Cholesterol, FastingBS,");
            Console.WriteLine("               RestingECG, MaxHR, ExerciseAngina, Oldpeak, ST_Slope.\n");

            // Criteria 3: Hyperparameter Tuning Configurations
            var experiments = new (string Name, int HiddenNodes, double LrpOut, double LrpIn, int Epochs)[]
            {
                ("Exp 1 (Small Net, Low LR)",     6,  0.05, 0.05, 600),
                ("Exp 2 (Baseline Default LR)",  12,  0.20, 0.15, 800),
                ("Exp 3 (Deep Hidden, Med LR)",  16,  0.15, 0.10, 1000),
                ("Exp 4 (Large Net, Fast LR)",   24,  0.30, 0.25, 800)
            };

            var allResults = new List<EvaluationResult>();
            var logCsvLines = new List<string> { "Experiment,Epoch,TrainMSE" };
            double bestF1 = -1.0;

            foreach (var exp in experiments)
            {
                Console.WriteLine($"\n>>> Running {exp.Name}: Hidden={exp.HiddenNodes}, LR_Out={exp.LrpOut}, LR_In={exp.LrpIn}, Epochs={exp.Epochs}");

                // 1. Initialize your custom NeuralNet (11 Inputs, X Hidden, 1 Output)
                NeuralNet network = new NeuralNet(11, exp.HiddenNodes, 1);

                // Optional: If you added setLearningRates to NeuralNet.cs, uncomment the next line:
                // network.setLearningRates(exp.LrpOut, exp.LrpIn);

                double currentTrainMSE = 0.0;

                // 2. Training Loop
                for (int epoch = 1; epoch <= exp.Epochs; epoch++)
                {
                    double sumSquaredError = 0.0;

                    foreach (var sample in trainSet)
                    {
                        // Feed all 11 normalized features into INeuron array
                        for (int i = 0; i < 11; i++)
                        {
                            network.setInputs(i, sample.Inputs[i]);
                        }

                        // Set target (0.0 or 1.0)
                        network.setDesiredOutput(0, sample.Target);

                        // Perform forward pass + backpropagation weight update
                        network.learn();

                        // Calculate squared error for this sample
                        double output = network.getOuputData(0);
                        double error = sample.Target - output;
                        sumSquaredError += (error * error);
                    }

                    currentTrainMSE = sumSquaredError / trainSet.Count;

                    // Deliverable 2: Print and log loss every 100 epochs
                    if (epoch == 1 || epoch % 100 == 0 || epoch == exp.Epochs)
                    {
                        Console.WriteLine($"    Epoch {epoch,4}/{exp.Epochs} | Train MSE Loss: {currentTrainMSE:F6}");
                        logCsvLines.Add($"\"{exp.Name}\",{epoch},{currentTrainMSE:F6}");
                    }
                }

                // 3. Evaluate on Unseen Test Set (Application Phase using network.run())
                var result = new EvaluationResult
                {
                    ConfigName = exp.Name,
                    FinalTrainMSE = currentTrainMSE
                };

                double testSquaredError = 0.0;
                foreach (var sample in testSet)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        network.setInputs(i, sample.Inputs[i]);
                    }

                    network.run(); // Forward pass only (no weight updates!)
                    double predictionProb = network.getOuputData(0);

                    double err = sample.Target - predictionProb;
                    testSquaredError += (err * err);

                    int predictedClass = predictionProb >= 0.5 ? 1 : 0;
                    int actualClass = (int)sample.Target;

                    if (predictedClass == 1 && actualClass == 1) result.TP++;
                    else if (predictedClass == 0 && actualClass == 0) result.TN++;
                    else if (predictedClass == 1 && actualClass == 0) result.FP++;
                    else if (predictedClass == 0 && actualClass == 1) result.FN++;
                }

                result.TestMSE = testSquaredError / testSet.Count;
                result.PrintDetailedReport();
                allResults.Add(result);

                // Save weights of the best performing model using your built-in saveWeights method!
                if (result.F1Score > bestF1)
                {
                    bestF1 = result.F1Score;
                    network.saveWeights("best_heart_model_weights.txt");
                }
            }

            // Export Deliverable 2 CSV Log
            File.WriteAllLines("training_loss_log.csv", logCsvLines);

            // Print Final Hyperparameter Comparison Table
            Console.WriteLine("\n=====================================================================================");
            Console.WriteLine("                     HYPERPARAMETER TUNING SUMMARY TABLE                             ");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine($"{"Configuration",-28} | {"Train MSE",-10} | {"Test MSE",-10} | {"Accuracy",-9} | {"Precision",-9} | {"Recall",-8} | {"F1-Score",-8}");
            Console.WriteLine(new string('-', 95));
            foreach (var r in allResults)
            {
                Console.WriteLine($"{r.ConfigName,-28} | {r.FinalTrainMSE,10:F5} | {r.TestMSE,10:F5} | {r.Accuracy * 100,8:F2}% | {r.Precision * 100,8:F2}% | {r.Recall * 100,7:F2}% | {r.F1Score * 100,7:F2}%");
            }
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("Saved training loss log to: 'training_loss_log.csv'");
            Console.WriteLine("Saved best model weights to: 'best_heart_model_weights.txt'");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}