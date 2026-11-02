using BackPropagation;

NeuralNet network = new NeuralNet(2, 4, 1);

network.setInputs(0, 0.5);
network.setInputs(1, 0.8);
network.setDesiredOutput(0, 1.0);

for (int iteration = 0; iteration < 1_000; iteration++)
{
    network.learn();
}

network.run();

double output = network.getOuputData(0);

Console.WriteLine($"Network output: {output:F6}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();