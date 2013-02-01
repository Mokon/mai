/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics {

  /// <summary>
  /// A heuristic class allows an algorithm to estimate the H value of a node.
  /// </summary>
  public abstract class Heuristic<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Estimates how much further until the goal state is reached given a 
    /// goal classifier.
    /// </summary>
    /// <param name="StaticState">The static state with which to based this
    /// comparision on.</param>
    /// <param name="DynamicState">The dynamic state with which to base this
    /// comparison on.</param>
    /// <param name="Goal">The goal specfier.</param>
    /// <param name="Operators">The operators valid in this domain.</param>
    /// <returns>The double value representing the cost to get to this goal
    /// state.</returns>
    public abstract Metric H( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Operators );


    /// <summary>
    /// Estimates how much further until a goal state is reached given a single
    /// dynamic state goal.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <param name="FinalState"></param>
    /// <param name="Operators"></param>
    /// <returns></returns>
    public abstract Metric H( SS StaticState,
      DS DynamicState, DS FinalState, Operator<SS, DS>[] Operators );

  }

}
