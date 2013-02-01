/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics {

  /// <summary>
  /// This is an interface which can be used to load various heurisitcs with
  /// the factory design pattern.
  /// </summary>
  public interface HeuristicFactory<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// This is a factory design pattern implemetation that returns a heuristic
    /// given a key. A HeuristicNotFoundException maybe thrown if the key is
    /// unknown by the implementing class.
    /// </summary>
    /// <param name="Key">The key to search on.</param>
    /// <returns>The heuristic based on the key.</returns>
    Heuristic<SS, DS> GetHeuristic( string Key, params string[] Data );

  }

  /// <summary>
  /// An exception to be thrown when an algorithm factory can not find an
  /// algorithm based on the key given.
  /// </summary>
  public class HeuristicNotFoundException : Exception {

    /// <summary>
    /// A constructor based on an error message.
    /// </summary>
    /// <param name="Message">The error message to pass along.</param>
    public HeuristicNotFoundException( string Message )
      : base( Message ) {
    }

  }

}
