/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using Net.Mokon.Utilities ;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is a algorithm implementation that used iterative deeping with
  /// DepthFirstSearchWClosedList as the backing algorithm.
  /// </summary>
  public class IterativeDeepingSearch<SS, DS> : UnlimitedStaticAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Algorithm">The alg. to iterate with.</param>
    public IterativeDeepingSearch( LimitableAlgorithm<SS, DS> Algorithm ) {
      this.Algorithm = Algorithm;
    }

    /// <summary>
    /// The alg. to iterate with.
    /// </summary>
    private LimitableAlgorithm<SS, DS> Algorithm;

    /// <summary>
    /// Compute method to find the path with IDS.
    /// </summary>
    /// <param name="StaticState">The static state to compute off of.</param>
    /// <param name="DynamicState">The dynamic state to compute off of.</param>
    /// <param name="Goal">The goal qualifier to check with.</param>
    /// <returns></returns>
    public override Path<SS, DS> ComputeComplete( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      while( true ) {
        Algorithm.Limit = Algorithm.CurrentLowLimit;
        Algorithm.CurrentLowLimit = Metric.Zero;
        Algorithm.Expansions = 0;
        Algorithm.Generations = 0;
        try {
          return Algorithm.ComputeComplete(
            StaticState, DynamicState, Goal, Actions );
        } catch( PathNotFoundException ) {
          this.Generations += Algorithm.Generations;
          this.Expansions += Algorithm.Expansions;
          if( Algorithm.Generations == Algorithm.Expansions ) {
            throw;// State Space Exhusted
          }
        }
      }
    }

  }

}
