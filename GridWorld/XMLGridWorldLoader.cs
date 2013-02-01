/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the grid world loader which has its file
  /// format defined by the A1 info sheet.
  /// </summary>
  public class XMLGridWorldLoader : GridWorldLoader<
    GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    /// <summary>
    /// The load method to parse the file format. This will throw a format
    /// exception if it fails.
    /// </summary>
    /// <param name="Stream">The stream to parse from.</param>
    /// <returns>The grid world.</returns>
    public override void Load( System.IO.TextReader Stream,
      out GenericGridWorldStaticState StaticState,
      out GenericGridWorldDynamicState DynamicState ) {

      XmlSerializer s = new XmlSerializer( typeof( Map ) );
      Map xml;
      xml = (Map)s.Deserialize( Stream );


      DynamicState = new GenericGridWorldDynamicState( );
      StaticState = new GenericGridWorldStaticState( xml.Height, xml.Width,
        DynamicState );

      List<Unit> Units = new List<Unit>( );
      Units.Add( new Unit( xml.Start.X, xml.Start.Y, xml.Height, xml.Width,
        xml.Goal.X, xml.Goal.Y, LOS, 0 ) );


      GridWorldFormat.FromTileStream( new StringReader(xml.Tiles),
        StaticState, DynamicState, Units );
      StaticState.SetChangeList( xml );
    }

  }

}
