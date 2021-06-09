/*
 * TimeTracker test application
 * Copyright (c) 2020, SailGP, All Rights Reserved
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerTest {
    class Program {

        static void Main( string[] args ) {
            // At what local time do we want the generator to start working
            double initial_local = 0;

            // How often do we query the Tracker for its time estimate
            double local_incr = 0.03;

            // How many times do we want to query the tracker in this test
            int n_samples = 50;

            // Create a unique comma-separated file to hold the results
            string filename = "test_" + DateTime.Now.ToString( "yyMMdd_HHmmss" ) + ".csv";
            using ( StreamWriter sw = new StreamWriter( filename )) {
                double local, remote;

                sw.WriteLine( "local, actual remote, messages, tracked remote" );

                // ----
                // Here are several different types of incoming message with different problems.
                // Use one of them.
                // ----

                // Best case - 1x speed, no delays, no jumps in time, no noise
                IGenerator gen = new GenIdeal { m_bias = 0.3 };

                // Add up to 0.3 secs delay in the receipt of the message
                //IGenerator gen = new GenDelays { m_delay_range = 0.3 };

                // The time rate is off by up to 10%
                //IGenerator gen = new GenDelays { m_rate = 1.1, m_delay_range = 0.3 };
                //IGenerator gen = new GenDelays { m_rate = 0.9, m_delay_range = 0.3 };

                // Remote time pauses for a moment, then resumes
                //IGenerator gen = new GenSegments {
                //    SegPoints = new List<GenSegments.Segment>{
                //        new GenSegments.Segment( 0, 1, 1, 1.7 ),
                //        new GenSegments.Segment( 1, 1.5, 0, 2.7 ),
                //        new GenSegments.Segment( 1.5, double.MaxValue, 1, 2.7 ),
                //    }
                //};

                // Remote time rewinds and resumes
                //IGenerator gen = new GenSegments {
                //    SegPoints = new List<GenSegments.Segment>{
                //        new GenSegments.Segment( 0, 1, 1, 1.7 ),
                //        new GenSegments.Segment( 1, 9999, 1, 1.7 ),
                //    }
                //};

                // The remote time is perfect, except for occasional garbage remote time
                //IGenerator gen = new GenGarbage() { m_odds_of_garbage = 0.1 };

                // A little of everything
                //IGenerator gen = new GenGarbage() {
                //    m_bias = 0.3,
                //    m_rate = 1.07,
                //    m_delay_range = 0.3,
                //    SegPoints = new List<GenSegments.Segment>{
                //        new GenSegments.Segment( 0, 1, 1, 1.7 ),
                //        new GenSegments.Segment( 1, 9999, 1, 1.7 ),
                //    },
                //    m_odds_of_garbage = 0.1,
                //};

                // ----
                // Replace this with an instance of your tracker

                ITracker track = new SimpleTracker();

                // ----

                // Initialize the generator, get the first message, and pass it to the tracker
                gen.Init( initial_local );
                gen.Get( out local, out remote );
                track.Set( gen.Id, local, remote );

                // Step through local time at a regular rate and query the Tracker for its time estimate.
                // Pass any incoming messages to the Tracker.
                double reporting_local = local + local_incr;
                for ( int i_sample = 0; i_sample < n_samples; ) {
                    if ( gen.PeekLocal() <= reporting_local ) {
                        // Report a new message received and update the Tracker
                        gen.Get( out local, out remote );
                        track.Set( gen.Id, local, remote );
                        sw.WriteLine( $"{local:F2},, {remote:F2}" );

                    } else {
                        // Report the actual remote time and the tracker's prediction for this time instant
                        remote = track.GetRemote( reporting_local );
                        double actual_remote = gen.GetActual( reporting_local );
                        sw.WriteLine( $"{reporting_local:F2}, {actual_remote:F2},, {remote:F2}" );
                        reporting_local += local_incr;
                        i_sample++;
                    }
                }
            }
        }
    }
}
