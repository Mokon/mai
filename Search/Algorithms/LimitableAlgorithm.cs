/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is an abstract class which all Algorithms must implement. The key
  /// point is the Compute method which returns a path from the given
  /// DynamicState to the final Goal state.
  /// </summary>
  public abstract class LimitableAlgorithm<SS, DS> : UnlimitedStaticAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="DepthBound">The depth</param>
    public LimitableAlgorithm( Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[],
      Metric> LimitingExpresion, Metric Limit ) {
      this.Limit = Limit;
      this.UseLimit = Limit != NO_LIMIT;
      this.LimitingExpresion = LimitingExpresion;
    }

    /// <summary>
    /// An expression to compute the limit value for a node.
    /// </summary>
    protected Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[], Metric>
      LimitingExpresion;

    /// <summary>
    /// A depth bound for the search on this tree.
    /// </summary>
    internal protected Metric Limit;

    /// <summary>
    /// A boolean test on whether or not to use the depth bound.
    /// </summary>
    protected bool UseLimit;

    /// <summary>
    /// The current low limit in the search.
    /// </summary>
    public Metric CurrentLowLimit;

    /// <summary>
    /// An identifier to allow no depth bound specification.
    /// </summary>
    public static readonly Metric NO_LIMIT = Metric.Zero;


  }

}
