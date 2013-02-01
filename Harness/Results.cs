/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A class to hold data about the running of an algorithm.
  /// </summary>
  public class Results {

    public bool Ceiling;

    /// <summary>
    /// Constructor given arguments.
    /// </summary>
    /// <param name="Generations"></param>
    /// <param name="Expansions"></param>
    /// <param name="ComputationTime"></param>
    /// <param name="PathLength"></param>
    public Results( string Algorithm, int ComputationLimit, uint Generations,
      uint Expansions, long ComputationTime, double PathLength,
      double OptimalPathLength, string File, double AverageComputationTime,
      long MaxMemoryUsage, long MaxComputationTime, double MeanExpansions ) {
      this.Algorithm = Algorithm;
      this.ComputationLimit = ComputationLimit;
      this.Generations = Generations;
      this.Expansions = Expansions;
      this.ComputationTime = ComputationTime;
      this.PathLength = PathLength;
      this.OptimalPathLength = OptimalPathLength;
      this.AverageComputationTime = AverageComputationTime;
      this.File = File;
      this.MaxMemoryUsage = MaxMemoryUsage;
      this.MaxComputationTime = MaxComputationTime;
      this.MeanExpansions = MeanExpansions;
      this.Ceiling = false;
    }

    /// <summary>
    /// A constructor that reads a result from a line.
    /// </summary>
    /// <param name="line"></param>
    public Results( string[] line, bool Ceiling ) {
      if ( line.Length != LineLength ) {
        throw new Exception( "Wrong Format Line Recieved" );
      }
      this.Algorithm = line[0].Trim( );
      this.ComputationLimit = int.Parse( line[1] );
      this.Expansions = uint.Parse( line[2] );
      this.Generations = uint.Parse( line[3] );
      this.ComputationTime = long.Parse( line[4] );
      this.PathLength = line[5].Trim().Equals( "Infinity" ) ?
        double.PositiveInfinity : double.Parse( line[5] );
      this.OptimalPathLength = double.Parse( line[6] );
      this.AverageComputationTime = double.Parse( line[7] );
      this.File = line[8].Trim( );
      this.MaxMemoryUsage = long.Parse( line[9] );
      this.MaxComputationTime = long.Parse( line[10] );
      this.MeanExpansions = double.Parse( line[11] );
      this.Ceiling = Ceiling;
    }

    /// <summary>
    /// The line length for parsing.
    /// </summary>
    public static readonly int LineLength = 12 ;

    /// <summary>
    /// The max memory usage on this result. This is only valid in 1 thread
    /// applications.
    /// </summary>
    public readonly long MaxMemoryUsage;

    /// <summary>
    /// The mean expansions.
    /// </summary>
    public readonly double MeanExpansions;

    /// <summary>
    /// The max computation time.
    /// </summary>
    public readonly long MaxComputationTime;

    /// <summary>
    /// The algorithm of the result.
    /// </summary>
    public readonly string Algorithm;

    /// <summary>
    /// The computation limit of the result.
    /// </summary>
    public int ComputationLimit;

    /// <summary>
    /// The generations of the result.
    /// </summary>
    public readonly uint Generations;

    /// <summary>
    /// The expansions of the result.
    /// </summary>
    public readonly uint Expansions;

    /// <summary>
    /// The computation time of the result.
    /// </summary>
    public readonly long ComputationTime;

    /// <summary>
    /// The path length of the result.
    /// </summary>
    public double PathLength;

    /// <summary>
    /// The optimal path length of the result.
    /// </summary>
    public readonly double OptimalPathLength;

    /// <summary>
    /// The file of the result.
    /// </summary>
    public readonly string File;

    /// <summary>
    /// The average computation time of the result.
    /// </summary>
    public readonly double AverageComputationTime;

    /// <summary>
    /// Returns the suboptimality of this result.
    /// </summary>
    public double SubOptimality {
      get {
        return PathLength/OptimalPathLength;
      }
    }

    /// <summary>
    /// Converts the result to a string that can be read in later.
    /// </summary>
    /// <returns></returns>
    public override string ToString( ) {
      return Algorithm + ", " + ComputationLimit + ", " + Expansions +
                  ", " + Generations + ", " + ComputationTime +
                  ", " + PathLength + ", " + OptimalPathLength  +
                  ", " + AverageComputationTime + ", " + File + 
                  ", " + MaxMemoryUsage + ", " + MaxComputationTime +
                  ", " + MeanExpansions ;
    }

    /// <summary>
    /// Returns a result header string.
    /// </summary>
    /// <returns></returns>
    public static string Header( ) {
      return "Alogorithm, Computation Limit, Expansions, Generations, " +
            "Computation Time, Path Length, Optimal Path Length, " + 
            "Average Computation Time, File, Max Memory Usage, " +
            "Max Computation Time, Mean Expansions";
    }

  }

}
