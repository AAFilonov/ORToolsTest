using System;
using Google.OrTools.LinearSolver;
using NUnit.Framework;

namespace ORToolsTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestLinear()
    {
        // Create the linear solver with the GLOP backend.
        var solver = Solver.CreateSolver("GLOP");
        if (solver is null) return;
        // Create the variables x and y.
        var x = solver.MakeNumVar(0.0, 1.0, "x");
        var y = solver.MakeNumVar(0.0, 2.0, "y");

        // Create a linear constraint, 0 <= x + y <= 2.
        var ct = solver.MakeConstraint(0.0, 4.0, "ct");
        ct.SetCoefficient(x, 1);
        ct.SetCoefficient(y, 1);

        // Create the objective function, 3 * x + y.
        var objective = solver.Objective();
        objective.SetCoefficient(x, 3);
        objective.SetCoefficient(y, 1);
        objective.SetMaximization();

        solver.Solve();
        Console.WriteLine("Solution:");
        Console.WriteLine("Objective value = " + solver.Objective().Value());
        Console.WriteLine("x = " + x.SolutionValue());
        Console.WriteLine("y = " + y.SolutionValue());

        Assert.Pass();
    }

    [Test]
    public void TestAssigment()
    {
        // 5 работников 4 задачаи
        int[,] costs =
        {
            { 90, 80, 75, 70 }, { 35, 85, 55, 65 }, { 125, 95, 90, 95 }, { 45, 110, 95, 115 }, { 50, 100, 90, 100 }
        };
        var numWorkers = costs.GetLength(0);
        var numTasks = costs.GetLength(1);

        // Create the linear solver with the SCIP backend.
        var solver = Solver.CreateSolver("SCIP");
        if (solver is null) return;


        // x[i, j] is an array of 0-1 variables, which will be 1
        // if worker i is assigned to task j.
        var x = new Variable[numWorkers, numTasks];
        for (var i = 0; i < numWorkers; ++i)
        for (var j = 0; j < numTasks; ++j)
            x[i, j] = solver.MakeIntVar(0, 1, $"worker_{i}_task_{j}");

        
// Each worker is assigned to at most one task.
        for (int i = 0; i < numWorkers; ++i)
        {
            Constraint constraint = solver.MakeConstraint(0, 1, "");
            for (int j = 0; j < numTasks; ++j)
            {
                constraint.SetCoefficient(x[i, j], 1);
            }
        }
        
// Each task is assigned to exactly one worker.
        for (int j = 0; j < numTasks; ++j)
        {
            Constraint constraint = solver.MakeConstraint(1, 1, "");
            for (int i = 0; i < numWorkers; ++i)
            {
                constraint.SetCoefficient(x[i, j], 1);
            }
        }
        
        
        Objective objective = solver.Objective();
        for (int i = 0; i < numWorkers; ++i)
        {
            for (int j = 0; j < numTasks; ++j)
            {
                objective.SetCoefficient(x[i, j], costs[i, j]);
            }
        }
        objective.SetMinimization();
      
        
        Solver.ResultStatus resultStatus = solver.Solve();
        
        // Check that the problem has a feasible solution.
        if (resultStatus == Solver.ResultStatus.OPTIMAL || resultStatus == Solver.ResultStatus.FEASIBLE)
        {
            Console.WriteLine($"Total cost: {solver.Objective().Value()}\n");
            for (int i = 0; i < numWorkers; ++i)
            {
                for (int j = 0; j < numTasks; ++j)
                {
                    // Test if x[i, j] is 0 or 1 (with tolerance for floating point
                    // arithmetic).
                    if (x[i, j].SolutionValue() > 0.5)
                    {
                        Console.WriteLine($"Worker {i} assigned to task {j}. Cost: {costs[i, j]}");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("No solution found.");
        }
        
        Assert.Pass();
    }
}