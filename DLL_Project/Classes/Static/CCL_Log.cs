﻿using System.Reflection;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    internal static class CCL_Log
    {

#if DEVELOPER
        private static System.IO.FileStream logFile;

        static                              CCL_Log()
        {
            OpenStream();
        }

        private static void                 OpenStream()
        {
            if( logFile == null )
            {
                logFile = System.IO.File.Open( "ccl_log.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read );
            }
        }

        public static void                  CloseStream()
        {
            if( logFile != null )
            {
                logFile.Close();
                logFile = null;
            }
        }

        public static void                  Write( string s )
        {
            if(
                ( s.NullOrEmpty() )||
                ( logFile == null )
            )
            {
                return;
            }

            s += "\n";
            // Have to copy to a byte array as dotNet doesn't allow getting the
            // pointer to a string nor can it cast a string to a char array
            byte[] b = new byte[ s.Length ];

            for( int i = 0; i < s.Length; ++i )
            {
                b[ i ] = (byte) s[ i ];
            }

            logFile.Write( b, 0, s.Length );
            logFile.Flush();
        }
#endif

        public static bool                  AppendSection( ref StringBuilder s, string str, bool addSectionDivider = true )
        {
            if( addSectionDivider )
            {
                s.Append( " :: " );
            }
            s.Append( str );
            return true;
        }

        public static bool                  AppendSectionNewLine( ref StringBuilder s, string str, bool addSectionDivider = true )
        {
            if( addSectionDivider )
            {
                s.Append( " :: " );
            }
            s.Append( str );
            s.Append( "\n" );
            return true;
        }

        /*
        private static void                 AppendTrace( ref StringBuilder s, Def atFault, Verbosity Severity, string content, string category = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            var mod = Find_Extensions.ModByDef( atFault );
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            if( !_TraceFor( ref modHelperDef, Severity, atFault ) )
            {
                return;
            }
            s.Append( "\t" );
            _BuildTrace( ref s, modHelperDef, Severity, content, atFault, category, false );
            s.Append( "\n" );
        }

        private static void                 AppendTrace( ref StringBuilder s, LoadedMod mod, Verbosity Severity, string content, string category = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            if( !_TraceFor( ref modHelperDef, Severity, null ) )
            {
                return;
            }
            s.Append( "\t" );
            _BuildTrace( ref s, modHelperDef, Severity, content, null, category, false );
            s.Append( "\n" );
        }
        */

        public static StringBuilder         BaseMessage( string content = null, string category = null )
        {
            var s = new StringBuilder();
            s.Append( Controller.Data.UnityObjectName );

            if( category != null )
            {
                AppendSection( ref s, category );
            }

            if( content != null )
            {
                AppendSection( ref s, content );
            }

            return s;
        }

        private static StringBuilder        captureTarget = null;
        private static Verbosity            captureVerbosity = Verbosity.Default;

        public static bool                  CaptureBegin( StringBuilder target )
        {
            if( captureTarget == null )
            {
                captureTarget = target;
                captureVerbosity = Verbosity.Default;
                return true;
            }
            if( captureTarget == target )
            {
                CCL_Log.Error( "Already capturing log", "Log Capture" );
                return true;
            }
            return false;
        }

        public static bool                  CaptureEnd( StringBuilder target, string status = "" )
        {
            if( captureTarget == null )
            {
                CCL_Log.Error( "Log isn't being captured, no need to end capture", "Log Capture" );
                return true;
            }
            if( captureTarget != target )
            {
                CCL_Log.Error( "Cannot end a capture on a different object", "Log Capture" );
                return false;
            }
            var captureStatus = status + "\n";
            captureTarget.Insert( 0, captureStatus );
            captureVerbosity = Verbosity.Default;
            captureTarget = null;
            return true;
        }

        /// <summary>
        /// Write a log => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  Message( string content, string category = null )
        {
            var s = captureTarget;
            if( s == null )
            {
                s = BaseMessage( content, category );
                Verse.Log.Message( s.ToString() );
#if DEVELOPER
                Write( s.ToString() );
#endif
            }
            else
            {
                s.Append( "\t" );
                bool prefixNext = false;
                if( category != null )
                {
                    prefixNext = AppendSection( ref s, category, false );
                }
                AppendSectionNewLine( ref s, content, prefixNext );
            }
        }

        /// <summary>
        /// Write an error => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  Error( string content, string category = null )
        {
            var s = captureTarget;
            if( s == null )
            {
                s = BaseMessage( content, category );
                Verse.Log.Error( s.ToString() );
#if DEVELOPER
                Write( s.ToString() );
#endif
            }
            else
            {
                s.Append( "\t" );
                bool prefixNext = false;
                if( category != null )
                {
                    prefixNext = AppendSection( ref s, category, false );
                }
                AppendSectionNewLine( ref s, content, prefixNext );
            }
        }

        public static void                  Trace( Verbosity Severity, string content, string category = null )
        {
            _Trace( Controller.Data.Trace_Current_Mod, Severity, content, null, category );
        }

        public static void                  TraceMod( Def atFault, Verbosity Severity, string content, string category = null )
        {
            var mod = Find_Extensions.ModByDef( atFault );
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            _Trace( modHelperDef, Severity, content, atFault, category );
        }

        public static void                  TraceMod( LoadedMod mod, Verbosity Severity, string content, string category = null )
        {
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            _Trace( modHelperDef, Severity, content, null, category );
        }

        public static void                  TraceMod( ModHelperDef modHelperDef, Verbosity Severity, string content, string category = null )
        {
            _Trace( modHelperDef, Severity, content, null, category );
        }

        private static void                 _Trace( ModHelperDef modHelperDef, Verbosity Severity, string content, Def atFault = null, string category = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            if( !_TraceFor( ref modHelperDef, Severity, atFault ) )
            {
                return;
            }
            var s = captureTarget;
            if( s == null )
            {
                s = BaseMessage();
            }
            else
            {
                s.Append( "\t" );
            }

            _BuildTrace( ref s, modHelperDef, Severity, content, atFault, category, ( captureTarget == null ) );

            if( captureTarget == null )
            {
#if DEVELOPER
                Write( s.ToString() );
#endif
                if( Severity <= Verbosity.NonFatalErrors )
                {
                    // Error
                    Verse.Log.Error( s.ToString() );
                }
                else if ( Severity == Verbosity.Warnings )
                {
                    // Warning
                    Verse.Log.Warning( s.ToString() );
                }
                else
                {
                    // Wall of text
                    Verse.Log.Message( s.ToString() );
                }
            }
            else
            {
                s.Append( "\n" );
            }
        }

        private static bool                 _TraceFor( ref ModHelperDef modHelperDef, Verbosity Severity, Def atFault )
        {
            Verbosity TraceAt = Verbosity.Default;
            if(
                ( !Controller.Data.Mods.NullOrEmpty() )&&
                ( !Controller.Data.ModHelperDefs.NullOrEmpty() )
            )
            {
                if(
                    ( modHelperDef == null )&&
                    ( atFault != null )
                )
                {
                    // Try to find the mod associated with this def

                    var mod = Find_Extensions.ModByDef( atFault );

                    if( mod != null )
                    {
                        modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
                    }
                }
                if( modHelperDef != null )
                {
                    TraceAt = modHelperDef.Verbosity;
                    if( modHelperDef.Verbosity > captureVerbosity )
                    {
                        captureVerbosity = modHelperDef.Verbosity;
                    }
                }
                else if ( atFault == null )
                {
                    TraceAt = captureVerbosity;
                }
                else
                {
                    TraceAt = Find_Extensions.HightestVerbosity;
                }
            }
            return ( TraceAt >= Severity );
        }

        private static void                 _BuildTrace( ref StringBuilder s, ModHelperDef modHelperDef, Verbosity Severity, string content, Def atFault = null, string category = null, bool addInitialSectionDivider = true )
        {
            if( Severity <= Verbosity.NonFatalErrors )
            {
                addInitialSectionDivider = AppendSection( ref s, "(Error)", addInitialSectionDivider );
            }
            else if( Severity == Verbosity.Warnings )
            {
                addInitialSectionDivider = AppendSection( ref s, "(Warning)", addInitialSectionDivider );
            }
            if(
                ( modHelperDef != null )&&
                ( modHelperDef != Controller.Data.cclHelperDef )
            )
            {
                addInitialSectionDivider = AppendSection( ref s, modHelperDef.ModName, addInitialSectionDivider );
            }
            if( category != null )
            {
                addInitialSectionDivider = AppendSection( ref s, category, addInitialSectionDivider );
            }
            if( atFault != null )
            {
                // Name of class
                var defType = atFault.GetType().ToString();
                addInitialSectionDivider = AppendSection( ref s, defType, addInitialSectionDivider );
                addInitialSectionDivider = AppendSection( ref s, atFault.defName, addInitialSectionDivider );
            }

            addInitialSectionDivider = AppendSection( ref s, content, addInitialSectionDivider );
        }

    }

}
