/*
 * TimeTracker test application
 * Copyright (c) 2020, SailGP, All Rights Reserved
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerTest {

    /// <summary>
    /// An IGenerator simulates receiving a stream of time reports from a
    /// remote device which have been timestamped with their time of
    /// arrival relative to our local time reference.
    /// </summary>
    public interface IGenerator {
        int Id { get; }
        void Init( double local );
        double PeekLocal();
        void Get( out double local, out double remote );
        double GetActual( double local );
    }

    /// <summary>
    /// An Ideal generator reports time at regular intervals, with
    /// a rate in perfect step with the local time, possibly with
    /// a fixed offset, with no delays, dropouts, or noise
    /// </summary>
    public class GenIdeal : IGenerator {
        public double m_interval = 0.1;
        public double m_rate = 1;
        public double m_bias = 0;
        protected double m_next_local = 0;

        public int Id { get; } = 17;

        public virtual void Init( double local ) {
            m_next_local = local;
        }

        public virtual double PeekLocal() {
            return m_next_local;
        }

        public virtual void Get( out double localx, out double remotex ) {
            localx = m_next_local;
            remotex = GetActual( localx );
            m_next_local += m_interval;
        }

        public virtual double GetActual( double local ) {
            return local * m_rate + m_bias;
        }
    }

    /// <summary>
    /// This extends the Ideal generator with a random amount of delay -
    /// that is, the local time-of-arrival timestamp is some amount greater
    /// than what it would be in the ideal case.  Note that the several
    /// consecutive Gets() may return the same local time, but it will
    /// never go backward.
    /// </summary>
    public class GenDelays : GenIdeal {
        public double m_delay_range = 0.0;
        double m_next_local_delayed;
        Random m_rand = new Random();

        public override void Init( double local ) {
            base.Init( local );
            m_next_local_delayed = m_next_local + m_delay_range * m_rand.NextDouble();
        }

        public override double PeekLocal() {
            return m_next_local_delayed;
        }

        public override void Get( out double localx, out double remotex ) {
            base.Get( out localx, out remotex );
            localx = m_next_local_delayed;
            double next_local_delayed = m_next_local + m_delay_range * m_rand.NextDouble();
            m_next_local_delayed = Math.Max( m_next_local_delayed, next_local_delayed );
        }
    }


    /// <summary>
    /// In a Segmented generator, the remote clock may pause or rewind to
    /// an earlier point in time, then continue.  Note: the local time
    /// reported in sequential calls to Get() will never go backward.
    /// </summary>
    public class GenSegments : GenDelays {

        public struct Segment {
            public double start_local, end_local, rate, start_val;

            public Segment( double start_localx, double end_localx, double ratex, double start_valx ) {
                start_local = start_localx;
                end_local = end_localx;
                rate = ratex;
                start_val = start_valx;
            }
        }

        public List<Segment> SegPoints = new List<Segment> {
            new Segment( 0, double.MaxValue, 1, 0 ),
        };

        public override double GetActual( double local ) {
            Segment seg = SegPoints.Find( s => s.end_local > local );
            return (local - seg.start_local) * seg.rate + seg.start_val;
        }
    }


    /// <summary>
    /// The Garbage generator will occasionally return a spurious
    /// remote time.  A good tracker will ignore these.
    /// </summary>
    public class GenGarbage : GenSegments {
        public double m_odds_of_garbage = 0.0;
        Random m_rand = new Random();

        public override void Get( out double localx, out double remotex ) {
            base.Get( out localx, out remotex );
            if ( m_rand.NextDouble() < m_odds_of_garbage )
                remotex = 10 * ( 2 * m_rand.NextDouble() - 1 );
        }
    }
}
