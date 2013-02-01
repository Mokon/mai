/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// This is an interface which can be used to load various algorithms with
  /// the factory design pattern.
  /// </summary>
  public interface AlgorithmFactory<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Returns an algorithm based on the given string and the given data
    /// fields. This will throw an AlgorithmNotFoundException if the given
    /// key is invalid based on the given implementation of this interface.
    /// </summary>
    /// <param name="Key">A string key of the algorithm which should be
    /// selected.</param>
    /// <param name="Data">object params of additional optional data
    /// to use when fetching the algorithm. What this data means depends
    /// on the implementation.</param>
    /// <returns>An algorithm based on the key.</returns>
    Algorithm<SS, DS> GetAlgorithm( string Key,
      HeuristicFactory<SS,DS> HeuristicFactory, int offset,
      params string[] Data );

  }

  /// <summary>
  /// An exception to be thrown when an algorithm factory can not find an
  /// algorithm based on the key given.
  /// </summary>
  public class AlgorithmNotFoundException : Exception {

    /// <summary>
    /// A constructor based on an error message.
    /// </summary>
    /// <param name="Message">The error message to pass along.</param>
    public AlgorithmNotFoundException( string Message )
      : base( Message ) {
    }

  }

}
