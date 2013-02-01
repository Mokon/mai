/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Harness;

namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// This interface provides a pattern to vizualize states.
  /// </summary>
  public abstract class StateVisualizer<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public delegate void KeyBoardSpecialHandler( int Key, int X, int Y );
    public delegate void KeyBoardHandler( byte Key, int X, int Y );

    public KeyBoardSpecialHandler kbsh;
    public KeyBoardHandler kbh;

    public virtual void Cleanup( ) {
    }

    /// <summary>
    /// Visualizes the given state.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    public abstract void Visualize( SS StaticState, DS DynamicState );

    /// <summary>
    /// Some initilization data that does not change
    /// through the course of a trial.
    /// </summary>
    /// <param name="Alg"></param>
    /// <param name="ComputationLimit"></param>
    /// <param name="OptimalPathLength"></param>
    /// <param name="File"></param>
    public virtual void SetInitData( string Alg,
    int ComputationLimit,
    double OptimalPathLength,
    string File, List<Results> Resultss ) {
      this.Alg  = Alg;
      this.ComputationLimit = ComputationLimit;
      this.OptimalPathLength = OptimalPathLength;
      this.File = File;
      this.Resultss = Resultss;
    }

    /// <summary>
    /// Sets the step data.
    /// </summary>
    /// <param name="Generations"></param>
    /// <param name="Expansions"></param>
    /// <param name="ComputationTime"></param>
    /// <param name="PathLength"></param>
    /// <param name="AverageComputationTime"></param>
    /// <param name="MaxMemoryUsage"></param>
    /// <param name="MaxComputationTime"></param>
    public virtual void SetStepData(
     int Generations,
     int Expansions,
     long ComputationTime,
     double PathLength,
     double AverageComputationTime,
     long MaxMemoryUsage,
     long MaxComputationTime,
     double MeanExpansions) {

      this.Generations = Generations;
      this.Expansions = Expansions;
      this.ComputationTime  = ComputationTime;
      this.PathLength = PathLength;
      this.AverageComputationTime = AverageComputationTime;
      this.MaxMemoryUsage= MaxMemoryUsage;
      this.MaxComputationTime = MaxComputationTime;
      this.MeanExpansions = MeanExpansions;
    }
    protected string Alg;
    protected int ComputationLimit;
    protected double OptimalPathLength;
    protected string File;
    protected List<Results> Resultss;

    protected int Generations;
    protected int Expansions;
    protected long ComputationTime;
    protected double PathLength;
    protected double AverageComputationTime;
    protected long MaxMemoryUsage;
    protected long MaxComputationTime ;
    protected double MeanExpansions;

    /// <summary>
    /// Visulizes an algorithm's iter step data structures.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <param name="AlgData"></param>
    public abstract void VisualizeAlg( SS StaticState, DS DynamicState,
      object AlgData );

    /// <summary>
    /// A function which viziliers can overwrite to flush their data out.
    /// </summary>
    public virtual void Flush( ) {
    }

  }

}
