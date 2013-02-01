/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class MakeScale : Mode {

    public override string GetHelp( ) {
      return "Scales a playlist to a new size.";
    }

    protected GridWorldLoader<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Loader = new GenericGridWorldLoader( );
    private int Height = 512;
    private int Width = 512;
    private string Playlist = null;

    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public MakeScale( ) {
      base.CLH = new CommandLineHandler(
        new GridWorldLoaderFlag( this, "", "Loader", "l", "loader" ),
         new CommandLineFlag( this, "The playlist to scale.", "Playlist", "p", "playlist" ),
         new CommandLineFlag( this, "The new height.", "Height", "h", "height" ),
         new CommandLineFlag( this, "The new width.", "Width", "w", "width" )
         );
    }

    /// <summary>
    /// Takes a playlist and scales it by the given width and height.
    /// </summary>
    public override void Run( ) {
      string bdir = "maps/scaled/" + Height + "x" + Width + "/";
      Directory.CreateDirectory( bdir );
      StreamReader pl = new StreamReader( new FileStream(
          Playlist, FileMode.Open, FileAccess.Read ) );
      string pll;

      while ( ( pll = pl.ReadLine( ) ) != null ) {
        var ppls =  pll.Split( );
        StreamReader I = new StreamReader(
          new FileStream( ppls[0], FileMode.Open, FileAccess.Read ) );
        GenericGridWorldStaticState SS;
        GenericGridWorldDynamicState DS;
        Loader.Load( I, out SS, out DS );
        SS.InitialDynamicState = DS;
        SS.Scale( Width, Height );
        int l = ppls[0].LastIndexOf( "/" );
        string d = ppls[0].Substring( 0, l );
        Directory.CreateDirectory( bdir + d );
        StreamWriter O = new StreamWriter(
          new FileStream( bdir + ppls[0], FileMode.Create, FileAccess.Write ) );
        O.Write( GridWorldFormat.ToFileString( SS, DS ) );
        O.Close( );
        I.Close( );
      }
      pl.Close( );

    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "make-scale";
    }

  }

}
