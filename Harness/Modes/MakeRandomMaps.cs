/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class MakeRandomMaps : Mode {

    public override string GetHelp( ) {
      return "Makes a set of random maps with a certain chance of each tile being blocked.";
    }

    private int Height = 10;
    private int Width = 10;
    private double Blocked = 0.3;
    private string Dir = "./maps/random";
    private int LOS = 7;
    private int Num = 100;
    private int Seed = (int)DateTime.Now.Ticks;


    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public MakeRandomMaps( ) {
      base.CLH = new CommandLineHandler(
         new CommandLineFlag( this, "The directory to make random maps in.", "Dir", "d", "dir" ),
         new CommandLineFlag( this, "The height of the random maps to make.", "Height", "h", "height" ),
         new CommandLineFlag( this, "The width of the random maps to make.", "Width", "w", "width" ),
         new CommandLineFlag( this, "The LOS of the random maps to make.", "LOS", "l", "LOS" ),
         new CommandLineFlag( this, "The number of random maps to make.", "Num", "n", "num" ),
         new CommandLineFlag( this, "The seed to use for the creation of random maps.", "Seed", "s", "seed" ),
         new CommandLineFlag( this, "The chance of each block being blocked.", "Blocked", "b", "blocked" )
         );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( ) {
      RandomGridWorldMaker Ran = new RandomGridWorldMaker( LOS, Blocked, Height, Width );
      string bdir = Dir + "/" + Blocked + "/";     
      int d = 0;
      while ( Directory.Exists( bdir + "/" + d ) ) {
        d++;
      }
      string rdir = bdir + "/" + d + "/";
      Directory.CreateDirectory( rdir );
      for ( int i = 0 ; i < Num ; i++ ) {
        Ran.Make( i + Seed, rdir + i + ".gwmap");
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "make-random";
    }

  }

}
