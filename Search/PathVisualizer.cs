/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// This interface provides a pattern to vizualize path objects in a more
  /// advanced way then there default ToString method.
  /// </summary>
  public interface PathVisualizer<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Writes the path out in some way to the given string based on the given
    /// stream, path, and state.
    /// </summary>
    /// <param name="Stream">The stream to output to.</param>
    /// <param name="Path">The path to visualize.</param>
    /// <param name="StaticState">The static state to work off of.</param>
    /// <param name="DynamicState">The dynamic state to work off of.</param>
    void Visualize( System.IO.TextWriter Stream, Path<SS, DS> Path,
      SS StaticState, DS DynamicState );

  }

}
