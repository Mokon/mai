/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// An interface which allows a Grid World to be loaded from a stream. This
  /// Interface is there to provide a method of loading different file formats.
  /// If any future extensions are provided.
  /// </summary>
  public abstract class GridWorldLoader<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    /// <summary>
    /// The LOS of units read in.
    /// </summary>
    public int LOS {
      get;
      set;
    }

    /// <summary>
    /// A method contcrete implementations must implement. This takes a stream
    /// and returns a grid world object as parsed. This function is to throw an
    /// ArgumentException if the file format is illegal. Specific file formats
    /// will be defined by the concrete implementing classes.
    /// </summary>
    /// <param name="Stream">The stream to parse the grid world from.</param>
    /// <param name="StaticState">The static state to load into.</param>
    /// <param name="DynamicState">The dynamic state to load into.</param>
    abstract public void Load( System.IO.TextReader Stream, out SS StaticState,
      out DS DynamicState );
  }

}
