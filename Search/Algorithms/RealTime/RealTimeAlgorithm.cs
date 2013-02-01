/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Search;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  /// <summary>
  /// This is an abstract class which all Algorithms must implement. The key
  /// point is the Compute method which returns a path from the given
  /// DynamicState to the final Goal state.
  /// </summary>
  public abstract class RealTimeAlgorithm<SS, DS> : Algorithm<SS,DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {    

    /// <summary>
    /// The computation limit for this real time algorithm.
    /// </summary>
    protected int ComputationLimit;

    // TODO Niels, you added this, what does this flag do? Please document.
    protected int ComputationDone;

    /// <summary>
    /// Sets the computation limit.
    /// </summary>
    /// <param name="ComputationLimit"></param>
    public void SetComputationLimit( int ComputationLimit ) {
      this.ComputationLimit = ComputationLimit;
    }

    public int GetComputationLimit()
    {
        return this.ComputationLimit;
    }

    public void setComputationDone(int ComputationDone)
    {
        this.ComputationDone = ComputationDone;
    }

    public int GetComputationDone()
    {
        return this.ComputationDone;
    }

    /// <summary>
    /// Return that the algorithm is real time.
    /// </summary>
    /// <returns></returns>
    public override bool IsRealTime( ) {
      return true ;
    }

  }

}
