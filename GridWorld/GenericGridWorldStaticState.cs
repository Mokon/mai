/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the static state interface which represents
  /// a vacuum world.
  /// </summary>
  public class GenericGridWorldStaticState :
    GridWorldStaticState<GenericGridWorldStaticState,
    GenericGridWorldDynamicState> {

    /// <summary>
    /// A constructor which takes the height and width of the grid and creates
    /// a jagged array based on this. This constructor DOES NOT initilize the
    /// tiles on the grid and so any use of this GridWorld object should be
    /// prefixed with an initilization of the tiles. For instance, a
    /// GridWorldLoader object would setup the grid for this.
    /// </summary>
    /// <param name="Height">The number of tiles on the y axis.</param>
    /// <param name="Width">The number of tiles on the x axis.</param>
    public GenericGridWorldStaticState( int Height, int Width,
      GenericGridWorldDynamicState ids )
      : base( Height, Width, ids ) {
    }

  }

}
