/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Net.Mokon.Utilities;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A mode for organizing batch output.
  /// </summary>
  public class OrganizeMode : Mode {

    public override string GetHelp( ) {
      return "Organizes batch mode output into an excel readable csv format.";
    }

    private TextReader In = System.Console.In;
    private TextWriter Out = null;
    private TextWriter ScatterPlots = null;
    private string GNU = null;
    private double DStarLiteSubOptMin = 0.0d;
    private double DStarLiteSubOptMax = double.MaxValue;
    private double PathLengthMin = 0.0d;
    private double PathLengthMax = double.MaxValue;
    private bool Quick = false;
    private bool Clean = false;
    private bool FilesPrint = false;

    public OrganizeMode( ) {
      base.CLH = new CommandLineHandler(
         new CommandLineFlag( this, "The output organized file, in csv format.", "Out", "o", "output" ),
         new CommandLineFlag( this, "The output organized file, in csv format for scatter plots.", "ScatterPlots", "sp", "scatterplots" ),
         new CommandLineFlag( this, "The batch csv input formated file.", "In", "i", "input" ),
         new CommandLineFlag( this, "The D* lite suboptimality min.", "DStarLiteSubOptMin", "d", "DStarSubOpt" ),
         new CommandLineFlag( this, "The D* lite suboptimality max.", "DStarLiteSubOptMax", "m", "DStarSubOptMax" ),
         new CommandLineFlag( this, "The A* optimal path length min", "PathLengthMin", "p", "PathLengthOptMin" ),
         new CommandLineFlag( this, "The A* optimal path length max", "PathLengthMax", "x", "PathLengthOptMax" ),
         new CommandLineFlag( this, "The GNU output formated file.", "GNU", "g", "gnu" ),
         new CommandLineFlag( this, "A quick output.", "Quick", "q", "quick" ),
         new CommandLineFlag( this, "Removes Ceiling Costs Results", "Clean", "c", "clean" ),
           new CommandLineFlag( this, "Prints the maps processed.", "FilesPrint", "f", "fp")
         );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( ) {
      string line;
      List<Results> ReadResults = new List<Results>( );

      int lp = 0;
      int li = 0;
      bool LastCeiling = false;
      while ( ( line = In.ReadLine( ) ) != null ) {
        if ( !Quick ) {
          System.Console.Out.WriteLine( "Processing Line {0}.", lp );
        }
        lp++;
        if ( line.StartsWith( "Ceiling Solution Cost" ) ) {
          LastCeiling = true;
          continue;
        }
        string[] lsegs = line.Split( ',' ).Where( x => !x.Equals( "" )
          ).ToArray( );
        if ( lsegs.Length !=
          Net.Mokon.Edu.Unh.CS.AI.Harness.Results.LineLength || 
          lsegs[0].Trim( ).Equals( "AStar" ) || 
          line.Equals( Net.Mokon.Edu.Unh.CS.AI.Harness.Results.Header( ) ) ) {
          li++;
          continue;
        }
        ReadResults.Add( new Results( lsegs, LastCeiling ) );
        if ( LastCeiling ) {
          LastCeiling = false;
        }
      }
      PrintLine( Out, "Processed " + lp + " lines. Ignored " + li + " lines." );
      if ( !ReadResults.Any( ) ) {
        return;
      }

      var FilesToExclude = new HashSet<string>( ( from x in ReadResults
                                                  where ( x.Algorithm.Equals( "D*Lite" ) && 
                           ( x.SubOptimality < DStarLiteSubOptMin || 
                             x.OptimalPathLength < PathLengthMin ||
                             x.SubOptimality > DStarLiteSubOptMax ||
                             x.OptimalPathLength > PathLengthMax ) ) || 
                             ( Clean && x.Ceiling )
                                                  select x.File ).Distinct( ) );

      PrintLine( Out, "Ignoring " + FilesToExclude.Count( ) + " Files." );
      foreach ( string f in FilesToExclude ) {
        PrintLine( Out, "\t" + f );
      }

      LinkedList<Results> Results = new LinkedList<Results>( ReadResults.Where(
        x => !FilesToExclude.Contains( x.File ) ) );
      PrintLine( Out, "Processing " + Results.Count( ) + " Files." );

      var Algs = new List<string>( Results.Select( x => x.Algorithm ).Distinct( ) );
      PrintLine( Out, "Processing " + Algs.Count( ) + " Algs." );
      var CompLimitTicks = new List<int>( Results.Select( x => x.ComputationLimit ).Distinct( ) );
      PrintLine( Out, "Processing " + CompLimitTicks.Count( ) + " Computation Limits." );
      var Files = new List<string>( Results.Select( x => x.File ).Distinct( ) );
      PrintLine( Out, "Processing " + Files.Count( ) + " Files." );
      CompLimitTicks.Sort( );
      PrintLine( Out, "Done Sorting" );


      int trial =  Algs.Count( )*CompLimitTicks.Count( );
      if ( trial != 0 ) {
        int numTrials = Results.Count( ) / trial;

        PrintLine( Out, "Printing Tables Based on " + numTrials + " trials." );
        PrintLine( Out, "Printing Tables for All Trials" );
        PrintTables( Results, CompLimitTicks, Algs, Files );
      }
      if ( Out!= null ) {
        Out.Flush( );
        Out.Close( );
      }
    }

    /// <summary>
    /// Prints a line to the out and std out.
    /// </summary>
    /// <param name="Out"></param>
    /// <param name="ToPrint"></param>
    private static void PrintLine( TextWriter Out, string ToPrint ) {
      if ( Out != null ) {
        Out.WriteLine( ToPrint );
        Out.Flush( );
      }
      System.Console.WriteLine( ToPrint );
    }

    public void PrintScatterPlots( IEnumerable<Results> Results,
      IEnumerable<int> CompLimitTicks, IEnumerable<string> Algs, IEnumerable<string> Files ) {
      List<STuple<string, int>> Points = new List<STuple<string, int>>( );
      foreach ( String a in Algs ) {
        foreach ( int clt in CompLimitTicks ) {
          Points.Add( new STuple<string, int>( a, clt ) );
        }
      }
      Func<Results, double> selectorX = x => x.Expansions;
      Func<Results, double> selectorY = x => x.SubOptimality;
      StringBuilder Plot = new StringBuilder( );
      Plot.AppendLine( ", Expansion, SubOptimality" );
      foreach ( var p in Points ) {
        var all = ( from i in Results
                    where p.First.Equals( i.Algorithm ) && p.Second == i.ComputationLimit
                    select i );
        Plot.Append( p.First + "-" + p.Second + ", " );
        if ( all.Any( ) ) {
          int Count = all.Count( );
          double AvgX = all.Select( selectorX ).Average( );
          double StddevX = Math.Sqrt( all.Aggregate( 0.0,
           ( sum2, v ) => sum2 + Math.Pow( selectorX( v )-AvgX, 2 ) ) / Count );
          double AvgY = all.Select( selectorY ).Average( );
          double StddevY = Math.Sqrt( all.Aggregate( 0.0,
           ( sum2, v ) => sum2 + Math.Pow( selectorY( v )-AvgY, 2 ) ) / Count );
          Plot.Append( AvgX + ", " + AvgY + ", " );
        } else {
          Plot.Append( "#N/A, #N/A" );
        }
        Plot.AppendLine( );
      }
    }

    /// <summary>
    /// Prints a bunch of tables from Results.
    /// </summary>
    /// <param name="Out"></param>
    /// <param name="Results"></param>
    /// <param name="CompLimitTicks"></param>
    /// <param name="Algs"></param>
    public void PrintTables( IEnumerable<Results> Results,
      IEnumerable<int> CompLimitTicks, IEnumerable<string> Algs, IEnumerable<string> Files ) {
      if ( GNU != null ) {
        PrintGNU( GNU, Results, CompLimitTicks, Algs, Files );
      }
      if ( ScatterPlots != null ) {
        PrintScatterPlots( Results, CompLimitTicks, Algs, Files );
      }
      if ( Out != null ) {
        PrintLine( Out, "Table SubOptimality" );
        PrintTable( Out, Results, CompLimitTicks, Algs, x => x.SubOptimality );
        if ( !Quick ) {
          PrintLine( Out, "Table Total Computation Time" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.ComputationTime );
          PrintLine( Out, "Table Average Computation Time" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.AverageComputationTime );
          PrintLine( Out, "Table Max Computation Time" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.MaxComputationTime );
          PrintLine( Out, "Table Total Memory Usage Time (Only Valid if Run on One Thread)" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.MaxMemoryUsage );
          PrintLine( Out, "Table Expansion" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.Expansions );
          PrintLine( Out, "Table Mean Expansions" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.MeanExpansions );
          PrintLine( Out, "Table Generations" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.Generations );
          PrintLine( Out, "Table PathLength" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.PathLength );
          PrintLine( Out, "Table Optimal Path Length" );
          PrintTable( Out, Results, CompLimitTicks, Algs, x => x.OptimalPathLength );
        }
      }
    }

    /// <summary>
    /// Prints a single table based on the field selector.
    /// </summary>
    /// <param name="Out"></param>
    /// <param name="Results"></param>
    /// <param name="CompLimitTicks"></param>
    /// <param name="Algs"></param>
    /// <param name="selector"></param>
    public static void PrintTable( TextWriter Out,
      IEnumerable<Results> Results, IEnumerable<int> CompLimitTicks,
      IEnumerable<string> Algs, Func<Results, double> selector ) {
      StringBuilder AverageSB = new StringBuilder( );
      StringBuilder CountSB = new StringBuilder( );
      StringBuilder StdDevSB = new StringBuilder( );
      AverageSB.Append( ", " );
      CountSB.Append( ", " );
      StdDevSB.Append( ", " );
      foreach ( string a in Algs ) {
        AverageSB.Append( a + ", " );
        CountSB.Append( a + ", " );
        StdDevSB.Append( a + ", " );
      }
      AverageSB.AppendLine( );
      CountSB.AppendLine( );
      StdDevSB.AppendLine( );

      foreach ( int clt in CompLimitTicks ) {
        System.Console.Out.WriteLine( "clt " + clt );
        AverageSB.Append( clt + ", " );
        CountSB.Append( clt + ", " );
        StdDevSB.Append( clt + ", " );
        foreach ( string a in Algs ) {
          System.Console.Out.WriteLine( "Alg " + a );
          var all = ( from i in Results
                      where a.Equals( i.Algorithm ) && clt == i.ComputationLimit
                      select i );

          if ( all.Any( ) ) {
            //double Diff = all.First( ).OptimalPathLength;
            int Count = all.Count( );
            double Avg = all.Select( selector ).Average( );
            double Stddev = Math.Sqrt( all.Aggregate( 0.0,
             ( sum2, v ) => sum2 + Math.Pow( selector( v )-Avg, 2 ) ) / Count );
            AverageSB.Append( Avg + ", " );
            CountSB.Append( Count + ", " );
            StdDevSB.Append( Stddev + ", " );
          } else {
            AverageSB.Append( "#N/A, " );
            CountSB.Append( "#N/A, " );
            StdDevSB.Append( "#N/A, " );
          }
        }
        AverageSB.AppendLine( );
        CountSB.AppendLine( );
        StdDevSB.AppendLine( );
      }
      PrintLine( Out, "Averages\r\n" + AverageSB.ToString( ) + "\r\nCount\r\n" +
        CountSB.ToString( ) + "\r\nStdDev\r\n" + StdDevSB.ToString( ) + "\r\n" );
    }

    public static void PrintGNU( string GNU,
      IEnumerable<Results> Results, IEnumerable<int> CompLimitTicks,
      IEnumerable<string> Algs, IEnumerable<string> Files ) {
      if ( !Directory.Exists( GNU ) ) {
        Directory.CreateDirectory( GNU );
      } else {
        throw new ArgumentException( "GNU Folder Exists" );
      }
      Dictionary<string, double> DStarSol = new Dictionary<string, double>( );

      foreach ( string f in Files ) {
        var DSol = Results.First( r => r.Algorithm.Equals( "D*Lite" ) &&
                        f.Equals( r.File ) );
        DStarSol.Add( f, DSol.SubOptimality );

      }
      int NumFiles = Files.Count( );
      int Inc = 1; //      (int)( NumFiles*0.05 );
      Files = Files.OrderBy( x => DStarSol.Get( x ) );
      foreach ( string a in Algs ) {
        TextWriter W = new StreamWriter( new FileStream( GNU + "/" + a.Replace( "*", "Star" ),
          FileMode.CreateNew, FileAccess.Write ) );
        System.Console.Out.WriteLine( "Alg " + a );
        foreach ( int clt in CompLimitTicks ) {
          for ( int i = 0 ; i< NumFiles ; i += Inc ) {
            var FilesSet = new HashSet<string>( Files.Skip( i ).Take( Inc ) );
            List<double> Difficulties = new List<double>( );
            foreach ( var F in FilesSet ) {
              double d;
              if ( !DStarSol.TryGetValue( F, out d ) ) {
                throw new ArgumentException( "D* Solution Missing" );
              }
              Difficulties.Add( d );
            }
            double Difficulty = Difficulties.Average( );

            var all = ( from r in Results
                        where a.Equals( r.Algorithm ) &&
                        FilesSet.Contains( r.File ) && clt == r.ComputationLimit
                        select r );
            if ( all.Any( ) ) {
              double AvgSubOpt = all.Select( x => x.SubOptimality ).Average( );
              W.WriteLine( a + ", " + clt  + ", " + AvgSubOpt + ", " + Difficulty );
              W.Flush( );
            }
          }
          W.WriteLine( );
          W.Flush( );
        }
        W.WriteLine( );
        W.Flush( );
        W.Close( );
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "organize";
    }

  }

}
